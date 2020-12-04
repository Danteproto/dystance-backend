using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.IntegrationTest
{
    public class UserApiTest
    {
        private readonly HttpClient _client;

        public UserApiTest()
        {
            var server = new TestServer(new WebHostBuilder()
               .UseEnvironment("Development")
               .UseStartup<Startup>());
            _client = server.CreateClient();
        }

        //Test get all users
        [Theory]
        public async Task UserLogin()
        {
            // Arrange
            //------------------------------Get token
            


            //------------------------------------------

            //var request = new HttpRequestMessage(new HttpMethod("GET"), "/api/Album/");
            //_client.DefaultRequestHeaders.Accept.Clear();


            //// Act
            //var response = await _client.SendAsync(request);

            //// Assert
            //response.EnsureSuccessStatusCode();
            //Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        }



    }
}
