﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class UserSemesters
    {
        [Key]
        public string UserId { get; set; }
        public string SemesterId { get; set; }
    }
}
