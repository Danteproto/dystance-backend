using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Interfaces
{
    public interface IUserService
    {

        public Task<IActionResult> Register(RegisterModel model);

        public Task<IActionResult> Login(LoginModel model);

    }
}
