using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RPiRunner2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public const int WEB_PORT = 9000;
        public const int SOCKET_PORT = 9888;

        const byte DOUT_PIN = 26;
        const byte SLK_PIN = 19;

        public const bool easyDebug = false;

        public const string PAGE = "SettingsPage.html";
        public const string LOGIN = "Login.html";

        private TCPListener tcp;
        private HTTPServer http;
        private UserHardwareLinker uhl;

        private GpioController gpioController;
        private GpioPin dout, clk;
        public MainPage()
        {
            this.InitializeComponent();

            gpioController = GpioController.GetDefault();
            dout = gpioController.OpenPin(DOUT_PIN);
            clk = gpioController.OpenPin(SLK_PIN);
            uhl = new UserHardwareLinker(clk, dout);
            System.Diagnostics.Debug.WriteLine("Connected to Hardware via GPIO.");

            tcp = new TCPListener(WEB_PORT, easyDebug);
            tcp.OnDataReceived += socket_onDataReceived;
            tcp.OnError += socket_onError;
            tcp.ListenAsync();
            System.Diagnostics.Debug.WriteLine("socket created");

            this.http = new HTTPServer(SOCKET_PORT);
            http.OnDataRecived += http_OnDataRecived;
            http.OnError += http_OnError;
            http.Start();
            System.Diagnostics.Debug.WriteLine("web server created");
        }


        /* web server functions */
        public Dictionary<string, string> getQuery(string data)
        {
            Dictionary<string, string> queryFields = new Dictionary<string, string>();

            string[] fields;
            string query;
            if (data.Split(' ').Length > 1)
            {
                query = data.Split(' ')[1];
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
                        queryFields.Add(sep[0], sep[1]);
                    }
                }
            }
            return queryFields;
        }

        public async void http_OnDataRecived(string data, HTTPServer sender)
        {
            Dictionary<string, string> fields = getQuery(data);

            if (sender.Restricted ||
                !(fields.Keys.Contains("username") && fields.Keys.Contains("password") && sender.validate(fields["username"], fields["password"])))
            {
                string html = await http.getHTMLAsync(LOGIN);
                string response = CreateHTTP.Code200_Ok(html);
                http.Send(response);
            }
            else
            {
                sender.Restricted = false;
                string html = await http.getHTMLAsync(PAGE);
                string response = CreateHTTP.Code200_Ok(html);
                http.Send(response);
            }


        }

        public void http_OnError(string message, HTTPServer senders)
        {
            System.Diagnostics.Debug.WriteLine("Internal Server Error: " + message);
        }




        /* socket functions */
        public void socket_onDataReceived(string message)
        {
            //TODO: do not assume for DRP, needs to be checked!
            DRP msg = DRP.deserializeDRP(message);

            /* taking care of illegal messages */
            if (msg.DevType == DRPDevType.RBPI)
            {
                DRP response = new DRP(DRPDevType.RBPI, "", msg.DestID, msg.SourceID, new List<float>(), 0, DRPMessageType.ILLEGAL);
                tcp.Send(response.ToString());
                return;
            }

            if (msg.MessageType == DRPMessageType.DATA || msg.MessageType == DRPMessageType.HARDWARE_ERROR || msg.MessageType == DRPMessageType.IN_USE)
            {
                DRP response = new DRP(DRPDevType.RBPI, "", msg.DestID, msg.SourceID, new List<float>(), 0, DRPMessageType.ILLEGAL);
                tcp.Send(response.ToString());
                return;
            }

            /* taking care of SCANNED messages */
            if (msg.MessageType == DRPMessageType.SCANNED)
            {
                if (uhl.currentServedUser() == null)
                {
                    //if no user uses the weight
                    TempProfile profile = new TempProfile(msg.UserName, msg.Token, msg.SourceID);
                    uhl.StartUser(profile);
                    try
                    {
                        float w = uhl.getWeight(10);
                        DRP response = new DRP(DRPDevType.RBPI, msg.UserName, msg.DestID, msg.SourceID, new List<float>() { w }, 0, DRPMessageType.DATA);
                        tcp.Send(response.ToString());
                        uhl.FinishUser();
                        return;
                    }
                    catch
                    {
                        DRP response = new DRP(DRPDevType.RBPI, msg.UserName, msg.DestID, msg.SourceID, new List<float>(), 0, DRPMessageType.HARDWARE_ERROR);
                        tcp.Send(response.ToString());
                        return;
                    }

                }
                else
                {
                    //if somwone already uses the weight
                    DRP response = new DRP(DRPDevType.RBPI, msg.UserName, msg.DestID, msg.SourceID, new List<float>() { }, 0, DRPMessageType.IN_USE);
                    tcp.Send(response.ToString());
                }
            }
            else if (msg.MessageType == DRPMessageType.ILLEGAL)
            {
                DRP response = new DRP(DRPDevType.RBPI, "", msg.DestID, msg.SourceID, new List<float>(), 0, DRPMessageType.ACK);
                tcp.Send(response.ToString());
            }
            else if (msg.MessageType == DRPMessageType.ACK)
            {
                //TODO: need to wait for ACK.
            }
            else
            {
                throw new Exception();
            }

        }
        public void socket_onError(string message)
        {
            /*
             * Here goes Ramy's code
             */
        }
    }
}
