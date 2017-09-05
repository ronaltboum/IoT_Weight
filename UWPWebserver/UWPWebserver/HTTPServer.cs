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

namespace UWPWebserver
{
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

        public HTTPServer(string page, int port)
        {
            _port = port;
            this.page = page;
        }

        public string getHTMLAsync()
        {
           string text = "<!DOCTYPE html>"+
                    "<html lang = \"en\" xmlns = \"http://www.w3.org/1999/xhtml\">"+
                        "<head>"+
                            "<meta charset = \"utf-8\">"+
                            "<title> Weight Installation Page</title>"+
                        "</head>"+
                        "<body bgcolor = \"cyan\">"+
                            "Please install that."+
                        "</body>"+
                    "</html> ";
            return text;
        }

        public async void Start()
        {
            try
            {
                //Fecha a conexão com a porta que está escutando atualmente
                if (listener != null)
                {
                    await listener.CancelIOAsync();
                    listener.Dispose();
                    listener = null;
                }

                //Criar uma nova instancia do listerner
                listener = new StreamSocketListener();

                //Adiciona o evento de conexão recebida ao método Listener_ConnectionReceived
                listener.ConnectionReceived += Listener_ConnectionReceived;
                //Espera fazer o bind da porta
                await listener.BindServiceNameAsync(Port.ToString());
                Debug.WriteLine("listening");
            }
            catch (Exception e)
            {
                //Caso aconteça um erro, dispara o evento de erro
                if (OnError != null)
                    OnError(e.Message);
                Debug.WriteLine("error listen");
            }
        }
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var reader = new DataReader(args.Socket.InputStream);
            _writer = new DataWriter(args.Socket.OutputStream);
            string data = "";
            while (true)
            {
                uint sizeFieldCount = await reader.LoadAsync(1);
                if (sizeFieldCount == 0)
                {
                    Debug.WriteLine("disconnected :-(");
                }
                data += reader.ReadString(sizeFieldCount);
                Debug.WriteLine(data);
                if (data.Contains("GET"))
                {
                    OnDataRecived(data);
                    data = "";
                    //return;
                }
            }
        }
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
        /*private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var reader = new DataReader(args.Socket.InputStream);
            string str = reader.ReadString(5);
            _writer = new DataWriter(args.Socket.OutputStream);
            Debug.WriteLine("Connected!");
            try
            {
                while (true)
                {
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    // In case a disconnection occurs
                    if (sizeFieldCount != sizeof(uint))
                    {
                        Debug.WriteLine("disconnected :-(");
                        return;
                    }

                    //String size
                    uint stringLenght = reader.ReadUInt32();
                    //Read inputStream data
                    uint actualStringLength = stringLenght;
                    //If a disconnection occurs
                    if (stringLenght != actualStringLength)
                    {
                        Debug.WriteLine("disconnected :-(");
                        return;
                    }
                    // Fires data event received
                    if (OnDataRecived != null)
                    {
                        //read the string with the last size
                        string data = reader.ReadString(actualStringLength);
                        //Fires data event received
                        OnDataRecived(data);
                    }
                }

            }
            catch (Exception ex)
            {
                // Dispara evento em caso de erro, com a mensagem de erro
                if (OnError != null)
                    OnError(ex.Message);
            }
        }
        */

    }
}