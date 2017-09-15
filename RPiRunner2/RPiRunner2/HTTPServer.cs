using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using System.Diagnostics;
using System.IO;
using Windows.Storage.Streams;
using Windows.Storage;

namespace RPiRunner2
{
    /// <summary>
    /// Create a Web Server that will run on the device.
    /// </summary>
    public class HTTPServer
    {
        private readonly int _port;
        public int Port { get { return _port; } }

        public bool Restricted { get => restricted; set => restricted = value; }
        public DateTime Last_login { get => last_login; set => last_login = value; }

        private StreamSocketListener listener;
        private DataWriter _writer;

        public delegate void DataRecived(string data, HTTPServer sender);
        public event DataRecived OnDataRecived;

        public delegate void Error(string message, HTTPServer sender);
        public event Error OnError;

        private bool restricted;
        private DateTime last_login;

        public const int SESSION = 20; // session time in minutes;

        private string username;
        private string password;

        /// <summary>
        /// Initialize a new web server.
        /// </summary>
        /// <param name="page"> the name of the file (HTML Document) to send to each new client who connects to the server</param>
        /// <param name="port">the port to listen to.</param>
        public HTTPServer(int port, string username = "admin", string password="admin")
        {
            _port = port;
            this.restricted = true;
            this.username = username;
            this.password = password;
        }

        public bool isSessionEnded()
        {
            return (DateTime.Now - last_login).Minutes == SESSION;
        }

        public bool validate(string username, string password)
        {
            return username.Equals(this.username) && password.Equals(this.password);
        }

        public void changeCredentials(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Retrives the HTML Document
        /// </summary>
        /// <returns>A string contains the content of the file</returns>
        public async Task<string> getHTMLAsync(string page)
        {
            StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile sampleFile = await storageFolder.GetFileAsync(page);
            string text = await FileIO.ReadTextAsync(sampleFile);
            return text;
        }

        /// <summary>
        /// Start listenng to new Connections
        /// </summary>
        public async void Start()
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
                if (OnError != null)
                    OnError(e.Message, this);
                Debug.WriteLine("error listen");
            }
        }

        /// <summary>
        /// This event activated for each new connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.WriteLine("connected to " + args.Socket.Information.RemoteAddress + " my ip: " + args.Socket.Information.LocalAddress);

            var reader = new DataReader(args.Socket.InputStream);
            _writer = new DataWriter(args.Socket.OutputStream);
            string data = "";
            bool error = false;
            while (true)
            {
                Task<uint> load_task = reader.LoadAsync(1).AsTask();
                
                bool isReceived = load_task.Wait(10000);
                if (!isReceived) {
                    Debug.WriteLine("disconnected :-(");
                    error = true;
                    break;
                }
                data += reader.ReadString(load_task.Result);
                
                if(data.Length > 3 && data.ToUpper().IndexOf("GET") < 0)
                {
                    Debug.WriteLine("Illegal message (non HTTP)");
                    error = true;
                    break;
                }
                
                if (data.IndexOf("\n") >= 0 || data.IndexOf("\r") >= 0)
                    break;
            }
            if (!error)
                OnDataRecived(data, this);
            else
                OnError("Disconnected during data transfer.", this);
        }

        /// <summary>
        /// Sends data back to the currently connected client.
        /// If there is no ongoing connections, this function will take no action.
        /// </summary>
        /// <param name="message">The message to send</param>
        public async void Send(string message)
        {
            if (_writer != null)
            {
                //Envia a string em si
                _writer.WriteString(message);

                try
                {
                    //Faz o Envio da mensagem
                    await _writer.StoreAsync();
                    //Limpa para o proximo envio de mensagem
                    await _writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    if (OnError != null)
                        OnError(ex.Message, this);
                }
            }
        }
    }
}