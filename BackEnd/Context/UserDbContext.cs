using BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace BackEnd.Context
{
    public class UserDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base (options)
        {

        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }
        public DbSet<UsersLog> UserLog { get; set; }
        public DbSet<AttendanceReports> AttendanceReports { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {            base.OnModelCreating(builder);

            builder.Entity<AppUser>(entity => { entity.ToTable(name: "Users"); });
            builder.Entity<AppRole>(entity => { entity.ToTable(name: "Roles"); });
            builder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("UserRoles"); });
            builder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("UserClaims"); });
            builder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("UserLogins"); });
            builder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("UserTokens"); });
            builder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("RoleClaims"); });

        }
    }
}
