using BackEnd.Models;
using Microsoft.AspNetCore.SignalR;
using Nancy.Json.Simple;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public static Dictionary<string, List<Whiteboard>[]> _whiteBoards
            = new Dictionary<string, List<Whiteboard>[]>();
        public enum RoomType
        {
            Join,
            Leave,
            Chat,
            Kick,
            Mute,
        }
        public async Task RoomAction(string roomId, int t, string userId)
        {
            var type = (RoomType)t;
            switch (type)
            {
                case RoomType.Join:
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                        if (!_currentUsers.ContainsKey(roomId))
                        {
                            _currentUsers.Add(roomId, new List<SocketUser>());
                        }

                        if (_currentUsers[roomId].Any(user => user.UserId == userId))
                        {
                            _currentUsers[roomId].First(user => user.UserId == userId).connectionId = Context.ConnectionId;
                        }

                        if (!_currentUsers[roomId].Any(user => user.UserId == userId))
                        {
                            _currentUsers[roomId].Add(new SocketUser
                            {
                                UserId = userId,
                                connectionId = Context.ConnectionId
                            });
                        }

                        await Clients.Group(roomId).SendAsync("RoomAction",
                            JsonConvert.SerializeObject(JObject.FromObject(new
                            {
                                type = t,
                                payload = _currentUsers[roomId].Select(x => x.UserId).ToList()
                            })));
                        break;
                    }
                case RoomType.Leave:
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                        _currentUsers[roomId].Remove(_currentUsers[roomId].Where(x => x.UserId == userId).FirstOrDefault());

                        await Clients.Group(roomId).SendAsync("RoomAction",
                            JsonConvert.SerializeObject(JObject.FromObject(new
                            {
                                type = t,
                                payload = _currentUsers[roomId].Select(x => x.UserId).ToList()
                            })));
                        break;
                    }
                case RoomType.Chat:
                    {
                        await Clients.Group(roomId).SendAsync("RoomAction",
                            JsonConvert.SerializeObject(JObject.FromObject(new
                            {
                                type = t,
                                payload = _currentUsers[roomId].Select(x => x.UserId).ToList()
                            })));
                        break;
                    }
                default:
                    {
                        await Clients.Client(
                            _currentUsers[roomId].Where(user => user.UserId == userId).Select(user => user.connectionId).FirstOrDefault())
                            .SendAsync("RoomAction",
                            JsonConvert.SerializeObject(JObject.FromObject(new
                            {
                                type = t,
                                payload = userId
                            })));
                        break;
                    }
            }
        }
        public async Task JoinRoom(string roomId, string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            if (!_currentUsers.ContainsKey(roomId))
            {
                _currentUsers.Add(roomId, new List<SocketUser>());
            }
            if (!_currentUsers[roomId].Any(users => users.UserId == userId))
            {
                _currentUsers[roomId].Add(new SocketUser
                {
                    UserId = userId,
                    connectionId = Context.ConnectionId
                });
            }

            await Clients.Group(roomId).SendAsync("UserListChange", JsonConvert.SerializeObject(_currentUsers[roomId].Select(x => x.UserId).ToList()));
        }

        public async Task LeaveRoom(string roomId, string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            _currentUsers[roomId].Remove(_currentUsers[roomId].Where(x => x.UserId == userId).FirstOrDefault());

            await Clients.Group(roomId).SendAsync("UserListChange", JsonConvert.SerializeObject(_currentUsers[roomId].Select(x => x.UserId).ToList()));
        }

        public async Task NewChat(string roomId)
        {
            await Clients.Group(roomId).SendAsync("NewChat", $"{Context.ConnectionId} add new chat");

        }

        public async Task DrawToWhiteboard(string roomId, string whiteBoard)
        {
            Whiteboard wb = JsonConvert.DeserializeObject<Whiteboard>(whiteBoard);

            switch (wb.Tool)
            {
                case "clear":
                    {
                        _whiteBoards[roomId] = new List<Whiteboard>[2];
                        _whiteBoards[roomId][0] = new List<Whiteboard>();
                        _whiteBoards[roomId][1] = new List<Whiteboard>();
                        break;
                    }
                case "undo":
                    {
                        if (!_whiteBoards.ContainsKey(roomId) || _whiteBoards[roomId] == null)
                        {
                            _whiteBoards[roomId] = new List<Whiteboard>[2];
                        }
                        if (_whiteBoards[roomId][1] == null)
                        {
                            _whiteBoards[roomId][1] = new List<Whiteboard>();
                        }
                        if (_whiteBoards[roomId][0] != null)
                        {
                            var lastUserBoard = _whiteBoards[roomId][0].Where(board => board.UserId == wb.UserId).LastOrDefault();
                            var tempWhiteboards = _whiteBoards[roomId][0].Where(board => board.UserId == lastUserBoard.UserId && board.DrawId == lastUserBoard.DrawId).ToList();
                            _whiteBoards[roomId][0].RemoveAll(x => tempWhiteboards.Any(y => x == y));
                            _whiteBoards[roomId][1].AddRange(tempWhiteboards);
                        }
                        break;
                    }
                case "redo":
                    {
                        if (!_whiteBoards.ContainsKey(roomId) || _whiteBoards[roomId] == null)
                        {
                            _whiteBoards[roomId] = new List<Whiteboard>[2];
                        }
                        if (_whiteBoards[roomId][0] == null)
                        {
                            _whiteBoards[roomId][0] = new List<Whiteboard>();
                        }
                        if (_whiteBoards[roomId][1] != null)
                        {
                            var lastUserBoard = _whiteBoards[roomId][1].Where(board => board.UserId == wb.UserId).LastOrDefault();
                            var tempWhiteboards = _whiteBoards[roomId][1].Where(board => board.UserId == lastUserBoard.UserId && board.DrawId == lastUserBoard.DrawId).ToList();
                            _whiteBoards[roomId][1].RemoveAll(x => tempWhiteboards.Any(y => x == y));
                            _whiteBoards[roomId][0].AddRange(tempWhiteboards);
                        }
                        break;
                    }
                default:
                    {
                        var listTools = new string[] {
                                        "line",
                                        "pen",
                                        "rect",
                                        "circle",
                                        "eraser",
                                        "addImgBG",
                                        "recSelect",
                                        "eraseRec",
                                        "addTextBox",
                                        "setTextboxText",
                                        "removeTextbox",
                                        "setTextboxPosition",
                                        "setTextboxFontSize",
                                        "setTextboxFontColor"
                        };
                        if (listTools.Contains(wb.Tool))
                        {
                            if (!_whiteBoards.ContainsKey(roomId) || _whiteBoards[roomId] == null)
                            {
                                _whiteBoards[roomId] = new List<Whiteboard>[2];
                            }
                            if (_whiteBoards[roomId][0] == null)
                            {
                                _whiteBoards[roomId][0] = new List<Whiteboard>();
                            }

                            if (wb.Tool == "setTextboxText")
                            {
                                var tempWhiteboard = _whiteBoards[roomId][0]
                                    .Where(board => board.Tool == "setTextboxText" && board.Distance[0] == wb.Distance[0])
                                    .LastOrDefault();
                                _whiteBoards[roomId][0].Remove(tempWhiteboard);
                            }
                            _whiteBoards[roomId][0].Add(wb);
                        }
                        break;
                    }
            }

            await Clients.Group(roomId).SendAsync("DrawToWhiteboard", JsonConvert.SerializeObject(wb, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            }));
        }
        public async Task JoinWhiteboard(string roomId)
        {
            var x = JObject.FromObject(new
            {
                common = new
                {
                    onWhiteboardLoad = new
                    {
                        setReadOnly = false,
                        displayInfo = false
                    },
                    showSmallestScreenIndicator = true,
                    imageDownloadFormat = "png",
                    performance = new
                    {
                        refreshInfoFreq = 5,
                        pointerEventsThrottling = new List<Object>
                    {
                        new { fromUserCount = 0,
                            minDistDelta = 1,
                            maxFreq = 30 },
                        new { fromUserCount = 10,
                            minDistDelta = 5,
                            maxFreq = 10 }
                    }
                    }
                }
            });
            await Clients.Client(Context.ConnectionId).SendAsync("WhiteboardConfig", JsonConvert.SerializeObject(x, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            }));
        }
        //private async Task JsonWhiteBoardReturn(string roomId, Whiteboard whiteboard)
        //{
        //    await Clients.Group(roomId).SendAsync("DrawToWhiteboard", JsonConvert.SerializeObject(wb, new JsonSerializerSettings
        //    {
        //        ContractResolver = new DefaultContractResolver
        //        {
        //            NamingStrategy = new CamelCaseNamingStrategy()
        //        },
        //        Formatting = Formatting.Indented
        //    }));
        //}
    }
}
