using BackEnd.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;

namespace BackEnd.Test
{
    public class FakeSignInManager : SignInManager<AppUser>
    {
        public FakeSignInManager()
                : base(new FakeUserManager(),
                     new Mock<IHttpContextAccessor>().Object,
                     new Mock<IUserClaimsPrincipalFactory<AppUser>>().Object,
                     new Mock<IOptions<IdentityOptions>>().Object,
                     new Mock<ILogger<SignInManager<AppUser>>>().Object,
                     new Mock<IAuthenticationSchemeProvider>().Object,
                     new Mock<IUserConfirmation<AppUser>>().Object)
        { }
    }

}
