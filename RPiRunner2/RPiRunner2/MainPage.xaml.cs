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
            if (gpioController != null)
            {
                dout = gpioController.OpenPin(DOUT_PIN);
                clk = gpioController.OpenPin(SLK_PIN);
                uhl = new UserHardwareLinker(clk, dout);
                System.Diagnostics.Debug.WriteLine("Connected to Hardware via GPIO.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Your machine does not support GPIO!");
            }
            tcp = new TCPListener(SOCKET_PORT, easyDebug);
            tcp.OnDataReceived += socket_onDataReceived;
            tcp.OnError += socket_onError;
            tcp.ListenAsync();
            System.Diagnostics.Debug.WriteLine("socket created");

            this.http = new HTTPServer(WEB_PORT);
            http.OnDataRecived += http_OnDataRecived;
            http.OnError += http_OnError;
            http.Start();
            System.Diagnostics.Debug.WriteLine("web server created");
        }


        /*  ============================================================================ */
        /*  This segment contains the functions that belongs to the installation system */
        /*  ============================================================================ */

       /// <summary>
       /// parse the HTTP request that was received from the user to get the data that he/she has sent/
       /// </summary>
       /// <param name="data">the HTTP request query</param>
       /// <returns>
       /// A dictionary of the data received.
       /// e.g. if the user tries to login by sending the username and the password, then this method will return a dictionary where:
       /// "username" -> "myuser"
       /// "password" -> "Aa123456"
       /// </returns>
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
                        queryFields.Add(sep[0], sep[1]);//TODO do something with the data
                    }
                }
            }
            return queryFields;
        }


        /// <summary>
        /// This method is activated every time a message has received from the user.
        /// </summary>
        /// <param name="data">The first row of the HTTP request query.</param>
        /// <param name="sender">the HTTPServer object that handles the current connection.</param>
        public async void http_OnDataRecived(string data, HTTPServer sender)
        {
            System.Diagnostics.Debug.WriteLine(data);
            Dictionary<string, string> fields = getQuery(data);

            //some browsers might ask for the page's icon.
            if(data.IndexOf(".ico") >= 0)
            {
                http.Send(CreateHTTP.Code404_NotFound());
            }

            //logout
            if (fields.Keys.Contains("logout") || sender.isSessionEnded())
            {
                sender.Restricted = true;
            }

            //login
            if(fields.Keys.Contains("username") && fields.Keys.Contains("password"))
            {
                bool init_restricted = sender.Restricted;
                sender.Restricted = sender.Restricted && !sender.validate(fields["username"], fields["password"]);
                if(init_restricted == true && sender.Restricted == false)
                    sender.Last_login = DateTime.Now;
            }

            if (sender.Restricted)
            {
                //sending the login page for unauthorised clients
                string html = await http.getHTMLAsync(LOGIN);
                string response = CreateHTTP.Code200_Ok(html);
                http.Send(response);
            }
            else
            {
                //sending the setting page for authorised clients (client that were successfully logged-in)
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

        /* end of installtion methods */
        /*  ============================================================================ */


        /* socket functions */
        public void socket_onDataReceived(string message)
        {
            System.Diagnostics.Debug.WriteLine("received: " + message);
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
                        //float w = uhl.getWeight(10);
                        float w = 87.433f;
                        DRP response = new DRP(DRPDevType.RBPI, msg.UserName, msg.DestID, msg.SourceID, new List<float>() { w }, 0, DRPMessageType.DATA);
                        tcp.Send(response.ToString());
                        System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());
                        uhl.FinishUser();
                        return;
                    }
                    catch
                    {
                        DRP response = new DRP(DRPDevType.RBPI, msg.UserName, msg.DestID, msg.SourceID, new List<float>(), 0, DRPMessageType.HARDWARE_ERROR);
                        tcp.Send(response.ToString());
                        System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());
                        return;
                    }

                }
                else
                {
                    //if somwone already uses the weight
                    DRP response = new DRP(DRPDevType.RBPI, msg.UserName, msg.DestID, msg.SourceID, new List<float>() { }, 0, DRPMessageType.IN_USE);
                    tcp.Send(response.ToString());
                    System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());
                }
            }
            else if (msg.MessageType == DRPMessageType.ILLEGAL)
            {
                DRP response = new DRP(DRPDevType.RBPI, "", msg.DestID, msg.SourceID, new List<float>(), 0, DRPMessageType.ACK);
                tcp.Send(response.ToString());
                System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());
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
