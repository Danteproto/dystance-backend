﻿using BackEnd.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BackEnd.Test
{
   public class ConnectionFactory : IDisposable  
    {  
 
        #region IDisposable Support  
        private bool disposedValue = false; // To detect redundant calls  
  
        public UserDbContext CreateContextForInMemory()  
        {  
            var option = new DbContextOptionsBuilder<UserDbContext>().UseInMemoryDatabase(databaseName: "Test_Database").Options;  
  
            var context = new UserDbContext(option);  
            if (context != null)  
            {  
                context.Database.EnsureDeleted();  
                context.Database.EnsureCreated();  
            }  
  
            return context;  
        }  
  
        protected virtual void Dispose(bool disposing)  
        {  
            if (!disposedValue)  
            {  
                if (disposing)  
                {  
                }  
  
                disposedValue = true;  
            }  
        }  
  
        public void Dispose()  
        {  
            Dispose(true);  
        }  
        #endregion  
    }  
}
