using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Constant
{
    public class Log
    {
        public enum LogType
        {
            ATTENDANCE_JOIN,
            ATTENDANCE_LEAVE,
            PRIVATE_CHAT_IMAGE,
            PRIVATE_CHAT_FILE,
            PRIVATE_CHAT_MESSAGE,
            ROOM_CHAT_IMAGE,
            ROOM_CHAT_FILE,
            ROOM_CHAT_TEXT,
            KICK,
            MUTE,
            GOT_KICKED,
            GOT_MUTED,
            TOGGLE_WHITEBOARD,
            DEADLINE_CREATE,
            DEADLINE_UPDATE,
            DEADLINE_DELETE,
            REMOTE_CONTROL_PERMISSION,
            REMOTE_CONTROL_ACCEPT,
            REMOTE_CONTROL_REJECT,
            REMOTE_CONTROL_STOP,
            WHITEBOARD_ALLOW,
            WHITEBOARD_DISABLE,
            GROUP_CREATE,
            GROUP_DELETE,
            GROUP_START,
            GROUP_STOP,
            GROUP_JOIN,
            GROUP_LEAVE,
            PRIVATE_CHAT_TEXT,
            SCREEN_SHARE_START,
            SCREEN_SHARE_STOP
        }
    }
}
