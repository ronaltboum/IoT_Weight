using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiRunner
{
    class ManageSockets
    {
        public const int PORT = 9888;
        private TCPListener tcp;
        public ManageSockets()
        {
            tcp = new TCPListener(PORT);
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
