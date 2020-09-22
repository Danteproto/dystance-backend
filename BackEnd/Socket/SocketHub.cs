using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Socket
{
    public class SocketHub : Hub
    {
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("AddToGroup", $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("RemoveFromGroup", $"{Context.ConnectionId} has left the group {groupName}.");
        }

        public async Task SendChat(string groupName)
        {
            await Clients.Group(groupName).SendAsync("SendChat", $"{Context.ConnectionId} add new chat");

        }
    }
}
