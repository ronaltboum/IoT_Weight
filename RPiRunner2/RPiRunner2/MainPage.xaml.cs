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
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Networking.Connectivity;

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

        public const int WEIGH_AVG = 1000;
        public const float CALIB_FACTOR = 1.75f;

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

       // public MobileServiceClient client;
        const string applicationURL = @"https://iotweight.azurewebsites.net";

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

            PermanentData.LoadFromMemory();
            PermanentData.CurrIP = GetLocalIp();
            if (!PermanentData.Serial.Equals(PermanentData.NULL_SYMBOL))
                putRecordInDatabase(PermanentData.CurrIP, PermanentData.Serial);
            if(uhl != null)
                uhl.setParameters(PermanentData.Offset, PermanentData.Scale);

            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            System.Diagnostics.Debug.WriteLine("Net status has changed!");
            try
            {
                string newIP = GetLocalIp();
                if (!newIP.Trim().Equals(""))
                {
                    PermanentData.CurrIP = newIP;
                    putRecordInDatabase(PermanentData.CurrIP, PermanentData.Serial);
                    System.Diagnostics.Debug.WriteLine("new IP: " + PermanentData.CurrIP);
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Could not get IP address, it's fine if there is no internet connection.");
            }
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

        float nullweight;
        float knownWeight;
        float rawWeight;
        /// <summary>
        /// This method is activated every time a message has received from the user.
        /// </summary>
        /// <param name="data">The first row of the HTTP request query.</param>
        /// <param name="sender">the HTTPServer object that handles the current connection.</param>
        public async System.Threading.Tasks.Task http_OnDataRecived(string data)
        {
            System.Diagnostics.Debug.WriteLine(data);
            Dictionary<string, string> fields = getQuery(data);

            
            string html = await HTTPServer.getHTMLAsync(PAGE);

            //some browsers might ask for the page's icon.
            if (data.IndexOf("favicon.ico") >= 0)
            {
                http.Send(CreateHTTP.Code204_NoContent());
                return;
            }
            if (fields.Keys.Contains("chname"))
            {
                System.Diagnostics.Debug.WriteLine("got: " + fields["name"]);
                PermanentData.Devname = fields["name"];
                html = HTTPServer.HTMLRewrite(html, "span", "name_feedback", "Your device's name was changed to  " + PermanentData.Devname);
            }
            if (fields.Keys.Contains("register"))
            {/*
                System.Diagnostics.Debug.WriteLine("got: " + fields["serial"]);
                long serial = long.Parse(fields["serial"]);
                Dictionary<string, string> entry = new Dictionary<string, string>();
                entry.Add("serial", fields["serial"]);
                entry.Add("IPaddr", "192.168.1.199");
                //await AzureIoTHub.SendDeviceToCloudMessageAsync(JsonConvert.SerializeObject(entry));
                html = HTTPServer.HTMLRewrite(html, "span", "serial_feedback", "The serial number  " + serial + " was registered to the cloud!");
                */
                string serial = fields["serial"];
                string ip = GetLocalIp();
                putRecordInDatabase(ip, serial);
                PermanentData.Serial = serial;
                PermanentData.CurrIP = ip;
            }
            if (fields.Keys.Contains("sscale"))
            {
                rawWeight = uhl.getWeight(WEIGH_AVG * CALIB_FACTOR);
                System.Diagnostics.Debug.WriteLine("rawWeight: " + rawWeight);
                html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "The scale calculated successfully");
            }
            if (fields.Keys.Contains("soffset"))
            {
                nullweight = uhl.getWeight(WEIGH_AVG * CALIB_FACTOR);
                System.Diagnostics.Debug.WriteLine("nullweight: " + rawWeight);
                html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "The offset calculated successfully");
            }
            if (fields.Keys.Contains("calibrate"))
            {
                knownWeight = float.Parse(fields["known"]);
                uhl.setParameters(nullweight, rawWeight, knownWeight);
                html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "OK! the device's parameters are:<br />OFFSET: " + uhl.Offset + "<br />SCALE: " + uhl.Scale);
                PermanentData.Scale = uhl.Scale;
                PermanentData.Offset = uhl.Offset;
            }

            string response =  CreateHTTP.Code200_Ok(html);
            http.Send(response);
            PermanentData.WriteToMemory();

            /*
                        //logout
                        if (fields.Keys.Contains("logout") || http.isSessionEnded())
                        {
                            http.Restricted = true;
                        }

                        //login
                        if(fields.Keys.Contains("username") && fields.Keys.Contains("password"))
                        {
                            bool init_restricted = http.Restricted;
                            http.Restricted = http.Restricted && !http.validate(fields["username"], fields["password"]);
                            if(init_restricted == true && http.Restricted == false)
                                http.Last_login = DateTime.Now;
                        }

                        if (http.Restricted)
                        {
                            //sending the login page for unauthorised clients
                            string html = await HTTPServer.getHTMLAsync(LOGIN);
                            string response = CreateHTTP.Code200_Ok(html);
                            http.Send(response);
                        }
                        else
                        {
                            string html = await HTTPServer.getHTMLAsync(PAGE);

                            //change Password
                            if (fields.Keys.Contains("chpass"))
                            {
                                //TODO: fix that.
                                if (!http.validate("admin", fields["curr_pass"]))
                                {
                                    html = HTTPServer.HTMLRewrite(html, "span", "chpass_feedback", "invalid password");
                                }
                                else if (!fields["curr_pass"].Equals(fields["confirm"]))
                                {
                                    html = HTTPServer.HTMLRewrite(html, "span", "chpass_feedback", "passwords do not match");
                                }
                                else
                                {
                                    http.changeCredentials("admin", fields["password"]);
                                    html = HTTPServer.HTMLRewrite(html, "span", "chpass_feedback", "password has changed successfully!");
                                }
                            }

                            //register to the cloud
                            else if (fields.Keys.Contains("register"))
                            {
                                long serial = long.Parse(fields["serial"]);

                                html = HTTPServer.HTMLRewrite(html, "span", "serial_feedback", "the device was registered!");
                            }


                            //sending the setting page for authorised clients (client that were successfully logged-in)

                            http.Restricted = false;
                            string response = CreateHTTP.Code200_Ok(html);
                            http.Send(response);
                        }

                        */
        }

        public void http_OnError(string message)
        {
            System.Diagnostics.Debug.WriteLine("Internal Server Error: " + message);
        }

        //Returns the device's IP Address
        private string GetLocalIp()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter == null) return null;
            var hostname =
                NetworkInformation.GetHostNames()
                    .SingleOrDefault(
                        hn =>
                            hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId);

            // the ip address
            return hostname?.CanonicalName;
        }

        //Ron
        //This function inserts to the SQL database a record with the Raspberry's Ip address and QR code.
        //Also: In case the Raspberry's Ip address was changed,  this function updates the database with the current IP.
        public async void putRecordInDatabase(string ipAdd, string QR_Code)
        {
            var client = new MobileServiceClient(applicationURL);
            IMobileServiceTable<RaspberryTable> raspberryTableRef = client.GetTable<RaspberryTable>();
            try
            {
                List<RaspberryTable> ipAddressList = await raspberryTableRef.Where(item => (item.QRCode == QR_Code)).ToListAsync();
                if (ipAddressList.Count == 0)
                {
                    var record1 = new RaspberryTable
                    {
                        QRCode = QR_Code,
                        IPAddress = ipAdd,
                    };
                    await raspberryTableRef.InsertAsync(record1);
                }
                else    //case where we want to update an old ip address
                {
                    var address = ipAddressList[0];
                    address.IPAddress = ipAdd;
                    await raspberryTableRef.UpdateAsync(address);
                }


                //System.Diagnostics.Debug.WriteLine("i'm after insert");
            }
            catch (Exception e)
            {
                //TODO Ron
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /* end of installtion methods */
        /*  ============================================================================ */


        /* socket functions */
        public async void socket_onDataReceived(string message)
        {
            System.Diagnostics.Debug.WriteLine("received: " + message);
            if (message.Equals("@welcome@"))
            {
                tcp.Send("TAUIOT@devname=" + PermanentData.Devname);
                return;
            }
            DRP msg;
            try
            {
                msg = DRP.deserializeDRP(message);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("non-DRP message received.");
                return;
            }

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
                        float w = uhl.getWeight(WEIGH_AVG);
                        DRP response = new DRP(DRPDevType.RBPI, msg.UserName, msg.DestID, msg.SourceID, new List<float>() { w }, 0, DRPMessageType.DATA);
                        tcp.Send(response.ToString());
                        System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());

                        //sending to cloud
                        Dictionary<string, string> jsend = new Dictionary<string, string>();
                        jsend.Add("username", uhl.currentServedUser().Username);
                        jsend.Add("weigh", w.ToString());
                        jsend.Add("createdAt", DateTime.Now.ToString());

                        string sendToCloud = JsonConvert.SerializeObject(jsend);
                        await AzureIoTHub.SendDeviceToCloudMessageAsync(sendToCloud);

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
