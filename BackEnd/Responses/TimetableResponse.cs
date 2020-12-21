using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Responses
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class TimetableResponse
    {
        public string Id { get; set; }
        public int EventType { get; set; }
        public string RoomId { get; set; }
        public string Title { get; set; }
        public string TeacherId { get; set; }
        public string Description { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}
