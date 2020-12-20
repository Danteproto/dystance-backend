using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BackEnd.Models;
using System.Diagnostics.CodeAnalysis;

namespace BackEnd.DBContext
{
    [ExcludeFromCodeCoverage]
    public class RoomDBContext:DbContext
    {
        public RoomDBContext(DbContextOptions<RoomDBContext> options): base(options)
        {

        }
        public DbSet<Room> Room { get; set; }
        public DbSet<RoomChat> RoomChat { get; set; }
        public DbSet<RoomUserLink> RoomUserLink { get; set; }
        public DbSet<Timetable> TimeTable { get; set; }
     
        public DbSet<Semester> Semester { get; set; }
    }
}
