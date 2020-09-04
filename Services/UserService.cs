using BackEnd.Context;
using BackEnd.Models;
using BackEnd.Errors;
using BackEnd.Interfaces;
using BackEnd.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Diagnostics;
using BackEnd.Ultilities;

namespace BackEnd.Services
{
    public interface IUserService
    {
        public Task<AuthenticateResponse> Authenticate(AuthenticateRequest model);
        AuthenticateResponse RefreshToken(string token);
        bool RevokeToken(string token, string ipAddress);
        IEnumerable<User> GetAll();
        AppUser GetById(string id);
    }

    public class UserService : IUserService
    {
        private UserDbContext _context;
        private readonly IMapper _mapper;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppSettings _appSettings;

        public UserService(
            UserDbContext context,
            IOptions<AppSettings> appSettings,
            IMapper mapper,
            SignInManager<AppUser> signInManager)
        {
            _context = context;
            _mapper = mapper;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest model)
        {
            AppUser appUser = null;
            if(String.IsNullOrWhiteSpace(model.Username))
            {
                if (EmailUtil.CheckIfValid(model.Email))
                {
                    appUser = _context.Users.SingleOrDefault(x => x.Email == model.Email);
                }
                else
                {
                    throw new RestException(HttpStatusCode.BadRequest, new { type = "0", message = "Email not found!" });
                }
            }
            else
            {
                appUser = _context.Users.SingleOrDefault(x => x.UserName == model.Username);
            }
            // return null if user not found
            if (appUser == null)
            {
                throw new RestException(HttpStatusCode.BadRequest, new { type = "0", message = "Username not found" });
            }

            if(!appUser.EmailConfirmed)
            {
                throw new RestException(HttpStatusCode.BadRequest, new { type = "1", message = "You must confirm your email before login" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(appUser, model.Password, false);



            if (result.Succeeded)
            {

                // authentication successful so generate jwt and refresh tokens
                User user = _mapper.Map<User>(appUser);

                var jwtToken = generateJwtToken(user);
                var refreshToken = generateRefreshToken();

                // save refresh token
                appUser.RefreshTokens.Add(refreshToken);
                _context.Update(appUser);
                _context.SaveChanges();

                return new AuthenticateResponse(user, jwtToken.token, refreshToken.Token, toSeconds(DateTime.Parse(jwtToken.ExpireDate), DateTime.Parse(jwtToken.StartDate)));
            }
            else 
            {
                throw new RestException(HttpStatusCode.BadRequest, new { type = "0", message = "Password is not correct" });
            }
        }

        public AuthenticateResponse RefreshToken(string token)
        {
            var appUser = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // return null if no user found with token
            if (appUser == null) return null;

            
            var refreshToken = appUser.RefreshTokens.Single(x => x.Token == token);

            // return null if token is no longer active
            if (!refreshToken.IsActive) return null;

            // replace old refresh token with a new one and save
            var newRefreshToken = generateRefreshToken();
            refreshToken.Revoked = DateTime.UtcNow;
            //refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            appUser.RefreshTokens.Add(newRefreshToken);
            _context.Update(appUser);
            _context.SaveChanges();

            // generate new jwt
            User user = _mapper.Map<User>(appUser);
            var jwtToken = generateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken.token, newRefreshToken.Token, toSeconds(DateTime.Parse(jwtToken.ExpireDate), DateTime.Parse(jwtToken.StartDate)));
        }

        public bool RevokeToken(string token, string ipAddress)
        {
            var appUser = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // return false if no user found with token
            if (appUser == null) return false;

            var refreshToken = appUser.RefreshTokens.Single(x => x.Token == token);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _context.Update(appUser);
            _context.SaveChanges();

            return true;
        }

        public IEnumerable<User> GetAll()
        {
            return _mapper.Map<List<AppUser>, List<User>>(_context.Users.ToList());

        }

        public AppUser GetById(string id)
        {
            return _context.Users.Find(id);
        }

        // helper methods

        private JwtToken generateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new JwtToken(tokenHandler.WriteToken(token), token.ValidTo.ToString(), token.ValidFrom.ToString());
        }

        private RefreshToken generateRefreshToken()
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow
                    //CreatedByIp = ipAddress
                };
            }
        }

        public string toSeconds(DateTime second, DateTime first)
        {
            return second.Subtract(first).TotalSeconds.ToString();
        }
    }
}
