using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    public class LogRequest
    {
        public string UserId { get; set; }
        public IFormFile Log { get; set; }
    }
}
