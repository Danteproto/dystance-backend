using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Semester
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
