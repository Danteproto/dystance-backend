using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    public class AddSemesterRequest
    {
        public string Name { get; set; }
        public IFormFile File { get; set; }
    }
}
