using BackEnd.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Socket
{
    public class SocketHub : Hub
    {
        private static Dictionary<string, List<SocketUser>> _currentUsers = new Dictionary<string, List<SocketUser>>();
        public async Task JoinRoom(string groupName, string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            if (!_currentUsers.ContainsKey(groupName))
            {
                _currentUsers.Add(groupName, new List<SocketUser>());
            }
            _currentUsers[groupName].Add(new SocketUser
            {
                UserId = userId,
                LastPing = DateTime.Now
            });
            await Clients.Group(groupName).SendAsync("join", JsonConvert.SerializeObject(_currentUsers[groupName].Select(x => x.UserId).ToList(), new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            }));
        }

        public async Task LeaveRoom(string groupName, string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _currentUsers[groupName].Remove((SocketUser)_currentUsers[groupName].Where(x => x.UserId == userId));

            await Clients.Group(groupName).SendAsync("Leave", JsonConvert.SerializeObject(_currentUsers[groupName], new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            }));
        }

        public async Task Broadcast(string groupName)
        {
            await Clients.Group(groupName).SendAsync("Broadcast", $"{Context.ConnectionId} add new chat");

        }
    }
}
