using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Constant
{
    public class EmailTemplate
    {
        public static string HTML_CONTENT = "Your account has been created on the DYSTANCE system by your organization. Use the information below to login: <br />" +
                                             "Email: {0} <br />" +
                                             "Username: {1} <br />" +
                                             "Password: {2} <br />" +
                                             "<a href='{3}'><h1 style='color:red;'>Click this link to active it first</h1><br/></a>";
    }
}
