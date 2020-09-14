using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BackEnd.Models;
using BackEnd.DBContext;

namespace BackEnd.DAO
{
    public class RoomUserLinkDAO
    {
        public static void Create(RoomDBContext context, RoomUserLink roomUserLink)
        {
            context.RoomUserLink.Add(roomUserLink);
            context.SaveChanges();
        }
    }
}
