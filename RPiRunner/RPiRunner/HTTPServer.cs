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

namespace RPiRunner
{
    /// <summary>
    /// Create a Web Server that will run on the device.
    /// </summary>
    public class HTTPServer
    {
        private readonly int _port;
        public int Port { get { return _port; } }

        private StreamSocketListener listener;
        private DataWriter _writer;

        public delegate void DataRecived(string data);
        public event DataRecived OnDataRecived;

        public delegate void Error(string message);
        public event Error OnError;

        private string page;

        /// <summary>
        /// Initialize a new web server.
        /// </summary>
        /// <param name="page"> the name of the file (HTML Document) to send to each new client who connects to the server</param>
        /// <param name="port">the port to listen to.</param>
        public HTTPServer(string page, int port)
        {
            _port = port;
            this.page = page;
        }

        /// <summary>
        /// Retrives the HTML Document
        /// </summary>
        /// <returns>A string contains the content of the file</returns>
        public async Task<string> getHTMLAsync()
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
                    OnError(e.Message);
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
                bool finished = !load_task.Wait(4000);
                if (!finished)
                {
                    if (load_task.Result == 0)
                    {
                        Debug.WriteLine("disconnected :-(");
                        error = true;
                        break;
                    }
                    data += reader.ReadString(load_task.Result);
                    
                   
                }
                else
                {
                    break;
                }
            }
            if (!error)
                OnDataRecived(data);
            else
                OnError("Disconnected during data transfer.");
            _writer = null;
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
                        OnError(ex.Message);
                }
            }
        }
    }
}