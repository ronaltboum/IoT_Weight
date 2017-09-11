using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiRunner2
{
    class ManageHTTPServer
    {
        public const string PAGE = "SettingsPage.html";
        public const int PORT = 9000;

        private HTTPServer socket;

        public HTTPServer Socket { get => socket; }

        public ManageHTTPServer()
        {
            this.socket = new HTTPServer(PAGE,PORT);
            socket.OnDataRecived += Socket_OnDataRecived;
            socket.OnError += socket_OnError;
            socket.Start();

            System.Diagnostics.Debug.WriteLine("web server created");
        }

        public async void Socket_OnDataRecived(string data)
        {
            string[] fields;
            if (data.Split(' ').Length > 1)
            {
                string query = data.Split(' ')[1];
                if (query.IndexOf("/?") == 0)
                {
                    query = query.Substring(2);
                    System.Diagnostics.Debug.WriteLine(query);
                    fields = query.Split('&');
                    foreach (string field in fields)
                    {
                        string prop, val;
                        string[] sep = field.Split('=');
                        prop = sep[0];
                        val = sep[1];
                        System.Diagnostics.Debug.WriteLine(sep[0] + " -> " + sep[1]); //TODO do something with the data
                    }
                }
            }
            string html = await socket.getHTMLAsync();
            string response = wrapWithHTTPHeaders(html);
            socket.Send(response);
        }

        public string wrapWithHTTPHeaders(string msg)
        {
            string response = "";
            response += "HTTP/1.1 200 OK\n";
            response += "Server: TAU_IoT_Workshop\n";
            response += "Content-Type: text/html\n";
            response += "Content-Length: " + msg.Length.ToString() + "\n";
            response += "\n";
            response += msg;
            return response;
        }

        public void socket_OnError(string message)
        {
            System.Diagnostics.Debug.WriteLine("Internal Server Error: " + message);
        }
    }
}
