using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Widget;
using Android.OS;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace AppSocket
{
    class TCPSender
    {
        private const int DEFAULT_PORT = 9000;
        private int port;
        private TcpClient tcp;
        private NetworkStream nets;

        public int Port { get => port; set => port = value; }

        public TCPSender(int port)
        {
            this.port = port;
            tcp = new TcpClient();
        }
        public TCPSender()
        {
            this.port = DEFAULT_PORT;
            tcp = new TcpClient();
        }

        public virtual bool Connect(string host)
        {
            try
            {
                tcp.Client.Connect(host, port);
                nets = tcp.GetStream();
                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }
        }

        protected virtual void Send(string msg)
        {
            byte[] msgAsBytes = Encoding.ASCII.GetBytes(msg);
            byte[] len = BitConverter.GetBytes(msg.Length);
            byte[] len4 = new byte[sizeof(uint)];

            Buffer.BlockCopy(len, 0, len4, sizeof(uint) - len.Length, len.Length);

            byte[] buf = new byte[msgAsBytes.Length + sizeof(uint)];
            Buffer.BlockCopy(len4, 0, buf, 0, sizeof(uint));
            Buffer.BlockCopy(msgAsBytes, 0, buf, sizeof(uint), msgAsBytes.Length);

            nets.WriteAsync(buf, 0, buf.Length);
            nets.FlushAsync();
        }

        protected virtual string Receive()
        {
            byte[] bmsize = new byte[sizeof(uint)];
            byte[] bmsg;

            int msize;
            string msg;

            nets.Read(bmsize, 0, sizeof(uint));
            msize = BitConverter.ToInt32(bmsize, 0);


            bmsg = new byte[msize];
            nets.Read(bmsg, 0, msize);

            msg = Encoding.ASCII.GetString(bmsg);

            return msg;
        }
    }
}