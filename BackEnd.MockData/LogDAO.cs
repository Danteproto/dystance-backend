using BackEnd.DAO;
using BackEnd.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.MockData
{
    public class LogDAO : ILogDAO
    {
        public async Task<string> CreateLog(UsersLog usersLog) => await Task.FromResult("true");
    }
}
