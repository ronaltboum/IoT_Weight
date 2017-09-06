using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RPiSocket
{
    public class TCPListener
    {
        private const int DEFAULT_PORT = 9000;
        private int port;

        private StreamSocketListener listener;


        private DataWriter writer;

        public int Port { get => port; set => port = value; }

        public TCPListener(int port)
        {
            this.port = port;
        }
        public TCPListener()
        {
            this.port = DEFAULT_PORT;
        }

        public async void ListenAsync()
        {
            try
            {
                if (listener != null)
                {
                    await listener.CancelIOAsync();
                    listener.Dispose();
                    listener = null;
                }

                listener = new StreamSocketListener();

                listener.ConnectionReceived += Listener_ConnectionReceived;
                await listener.BindServiceNameAsync(Port.ToString());
                Debug.WriteLine("listening");
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            //We assume here that the first 4 bytes of the message will always contain the message's length.
            var reader = new DataReader(args.Socket.InputStream);
            writer = new DataWriter(args.Socket.OutputStream);

            while (true)
            {
                uint msize = await reader.LoadAsync(sizeof(uint)); //receiving the message length
                if (msize < sizeof(uint))
                {
                    Debug.WriteLine("disconnected during data transfer. :-(");
                    return;
                }
                uint length = reader.ReadUInt32();

                uint loaded = await reader.LoadAsync(length);
                if (loaded < length)
                {
                    Debug.WriteLine("disconnected during data transfer. :-(");
                   // return;
                }

                string msg = reader.ReadString(loaded);

                OnDataReceived(msg);

            }
        }

        public async void Send(string message)
        {
            string send = "All the fish will say \"" + message + "\" until the end of days.";
            if (writer != null)
            {
                writer.WriteInt32(send.Length); //write message length
                writer.WriteString(send); //write message
                try
                {
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    OnError(ex.Message);
                }
            }
        }

        public virtual void OnDataReceived(string data)
        {
            //echoing the data
            Send(data);
        }
        public virtual void OnError(string data)
        {
            Send("Internal Server Error: " + data);
        }
    }
}
