using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using System.Diagnostics;
using System.IO;

namespace ManualDataSend2
{
    public class HTTPServer
    {
        private const int DEFAULT_PORT = 8000;
        private string page;
        private int port;
        private bool isRunning;
        public HTTPServer(string page)
        {
            this.Page = page;
            this.Port = DEFAULT_PORT;
            this.isRunning = false;
        }
        public HTTPServer(string page, int port)
        {
            this.Page = page;
            this.Port = port;
            this.isRunning = false;
        }

        public string Page { get => page; set => page = value; }
        public int Port { get => port; set => port = value; }
        public bool IsRunning { get => isRunning; }

        public async void start()
        {
            isRunning = true;
            try
            {
                //Create a StreamSocketListener to start listening for TCP connections.
               StreamSocketListener socketListener = new StreamSocketListener();

                //Hook up an event handler to call when connections are received.
                socketListener.ConnectionReceived += SocketListener_ConnectionReceived;

                //Start listening for incoming TCP connections on the specified port. You can specify any port that' s not currently in use.
                await socketListener.BindServiceNameAsync(port.ToString());
            }
            catch (Exception e)
            {
                isRunning = false;
                Debug.WriteLine("Error while trying to listen for connections");
            }
        }

        private async void SocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            //Read line from the remote client.
            Stream inStream = args.Socket.InputStream.AsStreamForRead();
            StreamReader reader = new StreamReader(inStream);
            string request = await reader.ReadLineAsync();

            Debug.WriteLine(request);
            //Send the line back to the remote client.
            //Stream outStream = args.Socket.OutputStream.AsStreamForWrite();
            //StreamWriter writer = new StreamWriter(outStream);
            //await writer.WriteLineAsync(request);
            //await writer.FlushAsync();
        }
        public void stop()
        {
            isRunning = false;
        }
    }
}
