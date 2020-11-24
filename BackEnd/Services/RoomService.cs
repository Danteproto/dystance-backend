using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using EmailService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Ultilities;
using Newtonsoft.Json.Schema;
using BackEnd.Responses;
using Newtonsoft.Json.Serialization;
using System.Security.Policy;
using System.Diagnostics.CodeAnalysis;

namespace BackEnd.Services
{
    public class RoomService
    {
        public enum ChatType
        {
            Text,
            Image,
            File
        }

        public async static Task<IActionResult> CreateRoom(RoomDBContext context, HttpRequest request, IWebHostEnvironment _env)
        {

            Room room = new Room
            {
                Subject = request.Form["name"],
                CreatorId = request.Form["creatorId"],
                StartDate = Convert.ToDateTime(request.Form["startDate"]),
                EndDate = Convert.ToDateTime(request.Form["endDate"]),
                Group = false
            };


            var result = await RoomDAO.Create(context, room);
            var lastRoom = RoomDAO.GetLastRoom(context);

            var timetables = JsonConvert.DeserializeObject<List<Timetable>>(request.Form["roomTimes"]);
            timetables.ForEach(timetable => timetable.RoomId = lastRoom.RoomId);
            result = await TimetableDAO.Create(context, timetables);

            var roomUserLink = new RoomUserLink
            {
                UserId = request.Form["CreatorId"],
                RoomId = lastRoom.RoomId,
            };

            string imgPath;
            string imgName = "";
            string extension = "";
            IFormFile img = null;

            imgName = "default";
            extension = ".png";
            img = Ultilities.Extensions.GetRoomDefaultAvatar(_env);


            lastRoom.Image = $"api/rooms/getImage?roomId={lastRoom.RoomId}&imgName={imgName + extension}";

            RoomDAO.UpdateRoom(context, lastRoom);
            await RoomUserLinkDAO.Create(context, roomUserLink);

            return result;
        }
        public async static Task<IActionResult> DeleteRoom(RoomDBContext context, int roomId, IWebHostEnvironment env)
        {
            var room = RoomDAO.Get(context, roomId);
            if (room != null)
            {
                var roomUserLinks = RoomUserLinkDAO.GetRoomLink(context, roomId);
                var roomChats = RoomChatDAO.GetChatByRoomId(context, roomId);
                var roomTimetables = TimetableDAO.GetByRoomId(context, roomId);
                var groups = RoomDAO.GetGroupByRoom(context, roomId);

                var result = await RoomUserLinkDAO.Delete(context, roomUserLinks);
                result = await RoomChatDAO.DeleteRoomChat(context, roomChats);
                result = await TimetableDAO.DeleteTimeTable(context, roomTimetables);
                foreach (var group in groups)
                {
                    result = await DeleteRoom(context, group.RoomId, env);
                }

                var path = Path.Combine(env.ContentRootPath, $"Files/{roomId}");
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                result = await RoomDAO.Delete(context, room);
                return result;
            }
            return new BadRequestObjectResult(new { message = "Class now exist!" });
        }
        public static Room GetRoomById(RoomDBContext context, int Id)
        {
            return RoomDAO.Get(context, Id);
        }
        public static async Task<IActionResult> CreateRoomChat(RoomDBContext context, HttpRequest request, IWebHostEnvironment env)
        {
            var type = (ChatType)Convert.ToInt32(request.Form["chatType"]);
            var roomId = Convert.ToInt32(request.Form["roomId"]);
            switch (type)
            {
                case ChatType.Text:
                    {
                        var roomChat = new RoomChat
                        {
                            RoomId = roomId,
                            UserId = request.Form["userId"],
                            Content = request.Form["content"],
                            Date = DateTime.Now,
                            Type = (int)ChatType.Text
                        };
                        return await RoomChatDAO.Create(context, roomChat);
                    }
                case ChatType.Image:
                    {
                        var img = request.Form.Files[0];
                        var extension = Path.GetExtension(img.FileName);

                        var imgName = Convert.ToBase64String(
                                System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())
                            );
                        var path = Path.Combine(env.ContentRootPath, $"Files/{roomId}/Chat/Images");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        var imgPath = Path.Combine(path, imgName + extension);
                        if (img.Length > 0)
                        {
                            using var fileStream = new FileStream(imgPath, FileMode.Create);
                            img.CopyTo(fileStream);
                        }
                        var roomChat = new RoomChat
                        {
                            RoomId = roomId,
                            UserId = request.Form["userId"],
                            Content = $"api/rooms/chat/getFile?fileName={imgName + extension}&roomId={roomId}&type={(int)ChatType.Image}&realName={Path.GetFileName(img.FileName)}",
                            Date = DateTime.Now,
                            Type = (int)ChatType.Image,
                            FileName = Path.GetFileName(img.FileName)
                        };
                        return await RoomChatDAO.Create(context, roomChat);
                    }
                case ChatType.File:
                    {
                        var file = request.Form.Files[0];
                        var extension = Path.GetExtension(file.FileName);
                        var fileName = Convert.ToBase64String(
                                System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())
                            );
                        var path = Path.Combine(env.ContentRootPath, $"Files/{roomId}/Chat/Files");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        var filePath = Path.Combine(path, fileName + extension);
                        if (file.Length > 0)
                        {
                            using var fileStream = new FileStream(filePath, FileMode.Create);
                            file.CopyTo(fileStream);
                        }
                        var roomChat = new RoomChat
                        {
                            RoomId = roomId,
                            UserId = request.Form["userId"],
                            Content = $"api/rooms/chat/getFile?fileName={fileName + extension}&roomId={roomId}&type={(int)ChatType.File}&realName={Path.GetFileName(file.FileName)}",
                            Date = DateTime.Now,
                            Type = (int)ChatType.File,
                            FileName = Path.GetFileName(file.FileName)
                        };
                        return await RoomChatDAO.Create(context, roomChat);
                    }
                default:
                    return new ObjectResult(new { message = "wrong chat type" })
                    {
                        StatusCode = 500,
                    };
            }

        }

        public static List<string> GetUsersByRoom(RoomDBContext context, int roomId)
        {
            return RoomUserLinkDAO.GetUsersByRoom(context, roomId);
        }

        public static List<RoomResponse> GetRoomByUserId(RoomDBContext context, string userId)
        {
            var rooms = RoomDAO.GetRoomsByUserId(context, userId);
            var response = new List<RoomResponse>();
            foreach (var room in rooms)
            {
                response.Add(new RoomResponse
                {
                    RoomId = room.RoomId,
                    RoomName = room.Subject + "-" + room.ClassName,
                    CreatorId = room.CreatorId,
                    Image = room.Image,
                    StartDate = room.StartDate,
                    EndDate = room.EndDate,
                });
            }
            return response;
        }
        public static List<RoomChat> GetChatByRoomId(RoomDBContext context, int roomId)
        {
            return RoomChatDAO.GetChatByRoomId(context, roomId);
        }

        public static RoomChat GetLastChat(RoomDBContext context, int roomId)
        {
            return RoomChatDAO.GetLastChat(context, roomId);
        }

        public static async Task<IActionResult> Invite(
            RoomDBContext roomContext, UserDbContext userContext,
            HttpRequest request, IEmailSender emailSender)
        {
            var roomId = Convert.ToInt32(request.Form["roomId"]);
            var room = RoomDAO.Get(roomContext, roomId);
            var message = request.Form["message"].ToString();
            string emailLists = request.Form["emailList"];
            var list = emailLists.Split(",").ToList();
            var userIds = userContext.Users.Where(user => list.Any(x => user.Email == x)).Select(user => user.Id).ToList();
            var roomUserLinks = userIds.Select(userId => new RoomUserLink
            {
                RoomId = roomId,
                UserId = userId
            }).ToList();
            var existLink = roomContext.RoomUserLink.Where(link => link.RoomId == roomId).ToList();
            if (existLink.Count != 0)
            {
                roomUserLinks = roomUserLinks.Where(link => !existLink.Any(x => x.UserId == link.UserId)).ToList();
            }
            var result = await RoomUserLinkDAO.Create(roomContext, roomUserLinks);
            roomUserLinks.ForEach(async link =>
            {
                var email = userContext.Users.Where(user => user.Id == link.UserId).Select(user => user.Email).FirstOrDefault().ToString();

                var mailMessage = new Message(new string[] { email }, "Invite To Class", message == "" ? $"You have been invite to class {room.Subject}-{room.ClassName}" : message, null);

                await emailSender.SendEmailAsync(mailMessage);
            });
            return result;
        }

        public static async Task<IActionResult> KickFromRoom(RoomDBContext context, int roomId, string userId)
        {
            var roomUserLink = RoomUserLinkDAO.GetRoomUserLink(context, roomId, userId);
            if (roomUserLink != null)
            {
                return await RoomUserLinkDAO.Delete(context, roomUserLink);
            }
            else
            {
                return new ObjectResult(new { type = 1, message = "user not in the room" })
                {
                    StatusCode = 200,
                };
            }
        }
        public static IActionResult WhiteboardUploadImage(HttpRequest request, IWebHostEnvironment env)
        {
            Console.WriteLine(request.Form);
            var roomId = Convert.ToInt32(request.Form["whiteboardId"]);
            string rawdata = request.Form["imagedata"];
            var imagedata = Convert.FromBase64String(rawdata.Split(",")[1]);
            var path = Path.Combine(env.ContentRootPath, $"Files/{roomId}/Whiteboard");
            var imgName = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())) + ".png";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var imgPath = Path.Combine(path, imgName);
            File.WriteAllBytes(imgPath, imagedata);
            return new OkObjectResult(JsonConvert.SerializeObject(new
            {
                imageUrl = $"api/rooms/whiteboard/img?id={roomId}&imgName={imgName}"
            }));
        }

        public async static Task<IActionResult> UpdateRoom(RoomDBContext context, HttpRequest request, IWebHostEnvironment _env)
        {
            var room = RoomDAO.Get(context, Convert.ToInt32(request.Form["roomId"]));
            if (room == null)
            {
                return new BadRequestObjectResult(new { type = 0, message = "Not found room" });
            }
            string imgPath;
            string imgName = "";
            string extension = "";
            IFormFile img = null;
            //if avatar is empty, use default

            if (request.Form.Files.Any())
            {
                img = request.Form.Files[0];
                extension = Path.GetExtension(img.FileName);

                imgName = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())
                    );
                var path = Path.Combine(_env.ContentRootPath, $"Files/{room.RoomId}/Images");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                imgPath = Path.Combine(path, imgName + extension);
                if (img.Length > 0)
                {
                    using var fileStream = new FileStream(imgPath, FileMode.Create);
                    img.CopyTo(fileStream);
                }
                room.Image = $"api/rooms/getImage?roomId={room.RoomId}&imgName={imgName + extension}";
            }

            room.Subject = request.Form["name"].Any() ? request.Form["name"].ToString() : room.Subject;
            room.CreatorId = request.Form["creatorId"].Any() ? request.Form["creatorId"].ToString() : room.CreatorId;
            room.StartDate = request.Form["startDate"].Any() ? Convert.ToDateTime(request.Form["startDate"]) : room.StartDate;
            room.EndDate = request.Form["endDate"].Any() ? Convert.ToDateTime(request.Form["endDate"]) : room.EndDate;
            var result = RoomDAO.UpdateRoom(context, room);

            if (request.Form["roomTimes"].Any())
            {
                var timetables = TimetableDAO.GetByRoomId(context, room.RoomId);
                await TimetableDAO.DeleteTimeTable(context, timetables);
                timetables = JsonConvert.DeserializeObject<List<Timetable>>(request.Form["roomTimes"]);
                timetables.ForEach(timetable => timetable.RoomId = room.RoomId);
                result = await TimetableDAO.Create(context, timetables);
            }

            return result;
        }

        public static GroupResponse CreateGroup(RoomDBContext context, HttpRequest request)
        {
            var now = DateTime.Now;

            Room room = new Room
            {
                Subject = request.Form["name"],
                CreatorId = request.Form["creatorId"],
                MainRoomId = Convert.ToInt32(request.Form["roomId"]),
                Group = true,
                StartDate = now,
                EndDate = now
            };

            var result = RoomDAO.Create(context, room);
            var group = RoomDAO.GetLastRoom(context);

            var listUserIds = JsonConvert.DeserializeObject<List<string>>(request.Form["userIds"]);
            var roomUserLinks = listUserIds.Select(userId => new RoomUserLink
            {
                RoomId = group.RoomId,
                UserId = userId
            }).ToList();
            var existLink = context.RoomUserLink.Where(link => link.RoomId == group.RoomId).ToList();
            if (existLink.Count != 0)
            {
                roomUserLinks = roomUserLinks.Where(link => !existLink.Any(x => x.UserId == link.UserId)).ToList();
            }
            result = RoomUserLinkDAO.Create(context, roomUserLinks);

            return new GroupResponse
            {
                GroupId = group.RoomId,
                Name = group.Subject,
                UserIds = RoomUserLinkDAO.GetRoomLink(context, group.RoomId).Select(item => item.UserId).ToList(),
                StartTime = group.StartDate.ToUniversalTime(),
                EndTime = group.EndDate.ToUniversalTime()
            };

        }

        public static GroupResponse GroupUpdate(RoomDBContext context, HttpRequest request)
        {
            var groupId = Convert.ToInt32(request.Form["groupId"]);
            var listUserIds = (IEnumerable<string>)JsonConvert.DeserializeObject<List<string>>(request.Form["userIds"]);
            var existLinks = (IEnumerable<string>)context.RoomUserLink.Where(link => link.RoomId == groupId).Select(link => link.UserId).ToList();

            var group = RoomDAO.Get(context, groupId);
            if (group != null)
            {
                var deleteUserIds = existLinks.Except(listUserIds).ToList();
                var addUserIds = listUserIds.Except(existLinks).ToList();

                var deleteLinks = deleteUserIds.Select(item => RoomUserLinkDAO.GetRoomUserLink(context, groupId, item)).ToList();

                var addLinks = addUserIds.Select(item => new RoomUserLink
                {
                    UserId = item,
                    RoomId = group.RoomId
                }).ToList();

                var result = RoomUserLinkDAO.Create(context, addLinks);
                result = RoomUserLinkDAO.Delete(context, deleteLinks);

                return new GroupResponse
                {
                    GroupId = group.RoomId,
                    Name = group.Subject,
                    UserIds = RoomUserLinkDAO.GetRoomLink(context, group.RoomId).Select(item => item.UserId).ToList(),
                    StartTime = group.StartDate.ToUniversalTime(),
                    EndTime = group.EndDate.ToUniversalTime()
                };
            }
            return null;
        }
        public static List<GroupResponse> GetGroupByRoomId(RoomDBContext context, int roomId)
        {
            var groups = RoomDAO.GetGroupByRoom(context, roomId);
            var responses = new List<GroupResponse>();
            foreach (var group in groups)
            {
                var response = new GroupResponse
                {
                    GroupId = group.RoomId,
                    Name = group.Subject,
                    UserIds = RoomUserLinkDAO.GetRoomLink(context, group.RoomId).Select(item => item.UserId).ToList(),
                    StartTime = group.StartDate.ToUniversalTime(),
                    EndTime = group.EndDate.ToUniversalTime()
                };
                responses.Add(response);
            }
            return responses;
        }

        public async static Task<IActionResult> ResetGroup(RoomDBContext context, int groupId, IWebHostEnvironment env)
        {
            var roomUserLinks = RoomUserLinkDAO.GetRoomLink(context, groupId);
            var roomChats = RoomChatDAO.GetChatByRoomId(context, groupId);

            var result = await RoomUserLinkDAO.Delete(context, roomUserLinks);
            result = await RoomChatDAO.DeleteRoomChat(context, roomChats);

            var path = Path.Combine(env.ContentRootPath, $"Files/{groupId}");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            var group = RoomDAO.Get(context, groupId);
            var now = DateTime.Now;
            group.EndDate = now;
            group.StartDate = now;
            RoomDAO.UpdateRoom(context, group);

            return result;
        }
        public async static Task<IActionResult> SetGroupTime(RoomDBContext context, HttpRequest request)
        {
            try
            {
                var roomId = Convert.ToInt32(request.Form["roomId"]);
                var groups = RoomDAO.GetGroupByRoom(context, roomId);
                var duration = Convert.ToInt32(request.Form["duration"]);
                foreach (var group in groups)
                {
                    group.StartDate = Convert.ToDateTime(request.Form["startTime"]);
                    group.EndDate = Convert.ToDateTime(request.Form["startTime"]).AddMinutes(duration);
                    RoomDAO.UpdateRoom(context, group);
                }
                return new OkObjectResult("successful");
            }
            catch (Exception e)
            {
                return new ObjectResult(new { message = e.Message })
                {
                    StatusCode = 500,
                };
            }
        }
        public async static Task<IActionResult> GetRoomsBySemesterId(RoomDBContext context, int semesterId)
        {
            try
            {
                var listRoom = (from rooms in context.Room
                                where rooms.SemesterId == semesterId
                                select new { roomId = rooms.RoomId.ToString(), 
                                            semesterId =rooms.SemesterId.ToString(), 
                                            roomName = rooms.ClassName, 
                                            teacherId = rooms.CreatorId,} ).ToList();


                return new OkObjectResult(listRoom);
            }
            catch (Exception e)
            {
                return new ObjectResult(new { message = e.Message })
                {
                    StatusCode = 500,
                };
            }
            
        }

    }
}
