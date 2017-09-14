using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiRunner2
{
    class CreateHTTP
    {
        public static string Code200_Ok(string content)
        {
            string response = "";
            response += "HTTP/1.0 200 OK\r\n";
            response += "Server: TAU_IoT_Workshop\r\n";
            response += "Content-Type: text/html\r\n";
            response += "Content-Length: " + content.Length.ToString() + "\r\n";
            response += "\r\n";
            response += content;
            return response;
        }
        public static string Code400_BadRequest()
        {
            return "";
        }
        //before popping the login window
        public static string Code401_Unauthorized()
        {
            return "HTTP/1.0 401 Unauthorized\r\n" +
                    "Server: TAU_IoT_Workshop\r\n" +
                    "WWW - Authenticate: Basic realm = \"Login is required to access the weight scale installation page.\"\r\n";
        }
        //after the user entered invalid login details
        public static string Code403_Forbidden()
        {
            return "";
        }
        public static string Code500_InternalServerError()
        {
            return "";
        }
    }
}
