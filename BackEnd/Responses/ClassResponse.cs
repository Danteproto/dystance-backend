﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Responses
{
    public class ClassResponse
    {
        public string id { get; set; }
        public string subject { get; set; }
        public string @class { get; set; }
        public string teacher { get; set; }
        public List<string> students { get; set; }
    }
}
