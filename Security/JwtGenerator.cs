using AutoMapper;
using BackEnd.Context;
using BackEnd.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Security
{
    public interface IJwtGenerator
    {
        string CreateToken(AppUser user);
        public JwtToken generateJwtToken(User user);
        public RefreshToken generateRefreshToken();
        public int toSeconds(DateTime second, DateTime first);
        public AuthenticateResponse VerifyAndReturnToken(AppUser appUser);
    }

    public class JwtGenerator
    {
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly UserDbContext _context;

        public JwtGenerator(IOptions<AppSettings> appSettings, IMapper mapper, UserDbContext context)
        {
            _appSettings = appSettings.Value;
            _mapper = mapper;
            _context = context;
        }
        public JwtToken generateJwtToken(User user)
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

        public RefreshToken generateRefreshToken()
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

        public int toSeconds(DateTime second, DateTime first)
        {
            return Convert.ToInt32(second.Subtract(first).TotalSeconds);
        }

        public AuthenticateResponse VerifyAndReturnToken(AppUser appUser)
        {
            User user = _mapper.Map<User>(appUser);

            var jwtToken = generateJwtToken(user);
            var refreshToken = generateRefreshToken();

            // save refresh token
            appUser.RefreshTokens.Add(refreshToken);
            _context.Update(appUser);
            _context.SaveChanges();

            return new AuthenticateResponse(user, jwtToken.token, refreshToken.Token, toSeconds(DateTime.Parse(jwtToken.ExpireDate), DateTime.Parse(jwtToken.StartDate)));
        }
    }
}
