using BackEnd.Context;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DAO
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class UserSemesterDAO
    {
        public static async Task<int> Create(UserDbContext context, UserSemesters userSemester)
        {
            await context.UserSemesters.AddAsync(userSemester);
            return await context.SaveChangesAsync();
        }
        public static async Task<int> Update(UserDbContext context, UserSemesters userSemester)
        {
            context.UserSemesters.Update(userSemester);
            return await context.SaveChangesAsync();
        }
        public static async Task<int> Delete(UserDbContext context, UserSemesters userSemester)
        {
            context.UserSemesters.Remove(userSemester);
            return await context.SaveChangesAsync();
        }

    }
}
