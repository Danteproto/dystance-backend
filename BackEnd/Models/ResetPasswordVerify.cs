﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    public class ResetPasswordVerify
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
