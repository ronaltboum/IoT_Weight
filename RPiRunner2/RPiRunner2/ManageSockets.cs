using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiRunner2
{
    class ManageSockets
    {
        public const int PORT = 9888;
        private TCPListener tcp;
        public ManageSockets(bool easyDebug = false)
        {
            tcp = new TCPListener(PORT,easyDebug);
            tcp.OnDataReceived += socket_onDataReceived;
            tcp.OnError += socket_onError;
            tcp.ListenAsync();
            System.Diagnostics.Debug.WriteLine("socket created");
        }

        public TCPListener Tcp { get => tcp; }

        public void socket_onDataReceived(string message)
        {
            tcp.Send("All the fish will say " + message + "\n");
            /*
             * Here goes Ramy's code
             */
        }
        public void socket_onError(string message)
        {
            /*
             * Here goes Ramy's code
             */
        }
    }
}
