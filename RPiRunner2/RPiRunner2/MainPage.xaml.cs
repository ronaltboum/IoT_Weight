using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using System.Text.RegularExpressions;
using System.Text;
using System.Net;

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

        public const int MAX_PASS_LENGTH = 12;
        public const int MIN_PASS_LENGTH = 4;

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

        //Note: cannot be async as it is the first function to run in the proccess (thus no father proccess to return the control with 'await').
        public MainPage()
        {
            this.InitializeComponent();
            Task.Run(() => MainPageAsync());
        }

        public async Task MainPageAsync()
        {

            //Initializing GPIO 
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

            //Initializing socket listener
            tcp = new TCPListener(SOCKET_PORT, easyDebug);
            tcp.OnDataReceived += socket_onDataReceived;
            tcp.OnError += socket_onError;
            Task SocketListenTask = tcp.ListenAsync();
            System.Diagnostics.Debug.WriteLine("socket created");

            //Initializing web server
            this.http = new HTTPServer(WEB_PORT);
            http.OnDataRecived += http_OnDataRecived;
            http.OnError += http_OnError;
            Task httpTask = http.Start();
            System.Diagnostics.Debug.WriteLine("web server created");

            //loading permanent data from memory
            Task loadTask = PermanentData.LoadFromMemoryAsync();
            await loadTask;

            Task putRecordTask = null;
            PermanentData.CurrIP = GetLocalIp();
            if (!PermanentData.Serial.Equals(PermanentData.NULL_SYMBOL))
                putRecordTask = putRecordInDatabase(PermanentData.CurrIP, PermanentData.Serial);
            if (uhl != null)
                uhl.setParameters(PermanentData.Offset, PermanentData.Scale);

            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

            if (putRecordTask != null)
            {
                await putRecordTask;
            }

            await SocketListenTask;
            await httpTask;
        }
        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            System.Diagnostics.Debug.WriteLine("Net status has changed!");
            try
            {
                string newIP = GetLocalIp();
                if (!newIP.Trim().Equals(""))
                {
                    PermanentData.CurrIP = newIP;
                    await putRecordInDatabase(PermanentData.CurrIP, PermanentData.Serial);
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

        /// <summary>
        /// Receives an HTTP request and check if it contains login details and if these details are correct
        /// </summary>
        /// <param name="headers">HTTP request splitted to rows</param>
        /// <returns>True if login details are exist and correct</returns>
        public bool authenticate(string[] headers)
        {
            bool auth = false;
            string[] temp;
            string basic;
            foreach (string header in headers)
            {
                if (header.ToLower().IndexOf("authorization") >= 0)
                {
                     temp = header.Split(' ');
                    basic= temp[temp.Length - 1].Trim();

                    byte[] decbasic = Convert.FromBase64String(basic);
                    string decodedString = Encoding.UTF8.GetString(decbasic);

                    if (decodedString.Equals(PermanentData.auth()))
                    {
                        auth = true;
                    }
                }
            }
            return auth;
        }

        float nullweight;
        float knownWeight;
        float rawWeight;
        /// <summary>
        /// This method is activated every time a message has received from the user.
        /// </summary>
        /// <param name="data">The first row of the HTTP request query.</param>
        /// <param name="sender">the HTTPServer object that handles the current connection.</param>
        public async Task http_OnDataRecived(string data, Windows.Storage.Streams.DataWriter writer)
        {
            System.Diagnostics.Debug.WriteLine(data);
            string[] headers = data.Split('\n');
            Dictionary<string, string> fields = getQuery(headers[0]);
            
            if (!authenticate(headers))
            {
                System.Diagnostics.Debug.WriteLine("@@Unauthorized");
                await http.Send(CreateHTTP.Code401_Unauthorized(), writer);
                return;
            }
            
            string html = await HTTPServer.getHTMLAsync(PAGE);

            html = HTTPServer.HTMLInputFill(html, "text", "name", PermanentData.Devname);
            html = HTTPServer.HTMLInputFill(html, "text", "serial", PermanentData.Serial);
            html = HTTPServer.HTMLInputFill(html, "text", "man_offset", PermanentData.Offset.ToString());
            html = HTTPServer.HTMLInputFill(html, "text", "man_scale", PermanentData.Scale.ToString());
            //some browsers might ask for the page's icon.
            if (data.IndexOf("favicon.ico") >= 0)
            {
                await http.Send(CreateHTTP.Code204_NoContent(), writer);
                return;
            }
            Task<float> hardwtask = null;
            try
            {
                if (fields.Keys.Contains("chname"))
                {
                    System.Diagnostics.Debug.WriteLine("got: " + fields["name"]);
                    PermanentData.Devname = WebUtility.UrlDecode(fields["name"]);
                    html = HTTPServer.HTMLRewrite(html, "span", "name_feedback", "Your device's name was changed to  " + PermanentData.Devname);
                }
                if (fields.Keys.Contains("chpass"))
                {
                    string currpass = WebUtility.UrlDecode(fields["curr_pass"]);
                    string newpass = WebUtility.UrlDecode(fields["password"]);
                    string confirm = WebUtility.UrlDecode(fields["confirm"]);

                    if (!newpass.Equals(confirm))
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "chpass_feedback", "Passwords do not match");
                    }
                    else if (!currpass.Equals(PermanentData.Password))
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "chpass_feedback", "Password incorrect");
                    }
                    else if (HTTPServer.passwordValidation(newpass, MIN_PASS_LENGTH, MAX_PASS_LENGTH) < 0)
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "chpass_feedback", "Password invalid<br/>The password should contain only letters and numbers and underscore ('_')<br/>Password must be 4-12 digits long.");
                    }
                    else
                    {
                        PermanentData.Password = newpass;
                        html = HTTPServer.HTMLRewrite(html, "span", "chpass_feedback", "Your password has changed successfully");
                    }
                }
                if (fields.Keys.Contains("register"))
                {
                    string serial = WebUtility.UrlDecode(fields["serial"]);
                    if (HTTPServer.passwordValidation(serial) < 0)
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "serial_feedback", "Serial invalid<br/>The Serial should contain only letters and numbers and underscore ('_')");
                    }
                    else
                    {
                        string ip = GetLocalIp();
                        await putRecordInDatabase(ip, serial);
                        PermanentData.Serial = serial;
                        PermanentData.CurrIP = ip;
                        html = HTTPServer.HTMLRewrite(html, "span", "serial_feedback", "Your device's serial is now  " + PermanentData.Serial);
                    }
                }
                
                if (fields.Keys.Contains("soffset") || fields.Keys.Contains("sscale"))
                {
                    if (uhl != null)
                    {
                        hardwtask = uhl.getRawWeightAsync((int)(WEIGH_AVG * CALIB_FACTOR));
                    }
                    else
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "Error: Your device does not have the sufficient hardware requerments.<br/>Operation did not complete.");
                    }
                }
                if (fields.Keys.Contains("calibrate"))
                {
                    try
                    {
                        if (uhl != null)
                        {
                            knownWeight = float.Parse(fields["known"]);
                            if (uhl.Scale != 0)
                            {
                                uhl.setParameters(nullweight, rawWeight, knownWeight);
                                html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "OK! the device's parameters are:<br />OFFSET: " + uhl.Offset + "<br />SCALE: " + uhl.Scale);
                                PermanentData.Scale = uhl.Scale;
                                PermanentData.Offset = uhl.Offset;
                            }
                            else
                            {
                                html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "the SCALE cannot be zero.");
                            }
                        }
                        else
                        {
                            html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "Error: Your device does not have the sufficient hardware requerments.<br/>Operation did not complete.");
                        }
                    }
                    catch
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "Operation failed.");
                    }
                }
                if (fields.Keys.Contains("man_calib"))
                {
                    if (uhl != null)
                    {
                        float offset = float.Parse(fields["man_offset"]);
                        float scale = float.Parse(fields["man_scale"]);
                        if (scale != 0)
                        {
                            uhl.setParameters(offset, scale);
                            html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "OK! the device's parameters are:<br />OFFSET: " + uhl.Offset + "<br />SCALE: " + uhl.Scale);
                            PermanentData.Scale = uhl.Scale;
                            PermanentData.Offset = uhl.Offset;
                        }
                        else
                        {
                            html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "the SCALE cannot be zero.");
                        }
                    }
                    else
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "Error: Your device does not have the sufficient hardware requerments.<br/>Operation did not complete.");
                    }
                }
                if (fields.Keys.Contains("best_calib"))
                {
                    if (uhl != null)
                    {
                        float offset = PermanentData.BEST_OFFSET;
                        float scale = PermanentData.BEST_SCALE;
                        uhl.setParameters(offset, scale);
                        html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "OK! the device's parameters are:<br />OFFSET: " + uhl.Offset + "<br />SCALE: " + uhl.Scale);
                        PermanentData.Scale = uhl.Scale;
                        PermanentData.Offset = uhl.Offset;
                    }
                    else
                    {
                        html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "Error: Your device does not have the sufficient hardware requerments.<br/>Operation did not complete.");
                    }
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            string response = CreateHTTP.Code200_Ok(html);

            Task writeTask = PermanentData.WriteToMemoryAsync();

            html = HTTPServer.HTMLInputFill(html, "text", "name", PermanentData.Devname);
            html = HTTPServer.HTMLInputFill(html, "text", "serial", PermanentData.Serial);
            html = HTTPServer.HTMLInputFill(html, "text", "man_offset", PermanentData.Offset.ToString());
            html = HTTPServer.HTMLInputFill(html, "text", "man_scale", PermanentData.Scale.ToString());

            Task sendTask = http.Send(response, writer);
            

            if(hardwtask != null)
            {
                if (fields.Keys.Contains("soffset"))
                {
                    nullweight = await hardwtask;
                    System.Diagnostics.Debug.WriteLine("nullweight: " + nullweight);
                    html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "The sensor returned value: " + nullweight);
                }
                if (fields.Keys.Contains("sscale"))
                {
                    rawWeight = await hardwtask;
                    System.Diagnostics.Debug.WriteLine("rawWeight: " + rawWeight);
                    html = HTTPServer.HTMLRewrite(html, "span", "calibration_feedback", "The sensor returned value: " + rawWeight);
                }
            }

            await writeTask;
            await sendTask;

            System.Diagnostics.Debug.WriteLine("HTTP: page sent back to user");
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
                            == icp.NetworkAdapter.NetworkAdapterId && hn.CanonicalName.Split('.').Length == 4);
            // the ip address
            return hostname?.CanonicalName;
        }

        //Ron
        //This function inserts to the SQL database a record with the Raspberry's Ip address and QR code.
        //Also: In case the Raspberry's Ip address was changed,  this function updates the database with the current IP.
        public async Task putRecordInDatabase(string ipAdd, string QR_Code)
        {
            var client = new MobileServiceClient(applicationURL);
            //TODO: there is not async version od GetTable, but we need to make it async somwhow
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
        public async Task socket_onDataReceived(string message, Windows.Storage.Streams.DataWriter writer)
        {
            System.Diagnostics.Debug.WriteLine("received: " + message);
            if (message.Equals("@welcome@"))
            {
                await tcp.Send("TAUIOT@devname=" + PermanentData.Devname, writer);
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
                DRP response = new DRP(DRPDevType.RBPI, "",PermanentData.Serial, PermanentData.Devname,0, 0, DRPMessageType.ILLEGAL);
                await tcp.Send(response.ToString(), writer);
                return;
            }


            if (msg.MessageType == DRPMessageType.DATA || msg.MessageType == DRPMessageType.HARDWARE_ERROR || msg.MessageType == DRPMessageType.IN_USE)
            {
                DRP response = new DRP(DRPDevType.RBPI, "",PermanentData.Serial, PermanentData.Devname, 0, 0, DRPMessageType.ILLEGAL);
                await tcp.Send(response.ToString(), writer);
                return;
            }

            /* taking care of SCANNED messages */
            if (msg.MessageType == DRPMessageType.SCANNED)
            {
                if(uhl == null)
                {
                    DRP response = new DRP(DRPDevType.RBPI, msg.UserName, PermanentData.Serial, PermanentData.Devname, 0, 0, DRPMessageType.HARDWARE_ERROR);
                    await tcp.Send(response.ToString(), writer);
                    System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());
                    return;
                }
                TempProfile profile = new TempProfile(msg.UserName, msg.Token, 0);
                if (uhl.currentServedUser() == null)
                {
                    //if no user uses the weight
                    uhl.StartUser(profile);
                    try
                    {
                        float w = uhl.getWeight(WEIGH_AVG);
                        DRP response = new DRP(DRPDevType.RBPI, msg.UserName, PermanentData.Serial, PermanentData.Devname,  w , 0, DRPMessageType.DATA);
                        Task sendTask = tcp.Send(response.ToString(), writer);
                        System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());

                        //sending to cloud
                        Dictionary<string, string> jsend = new Dictionary<string, string>();
                        jsend.Add("username", msg.UserName);
                        jsend.Add("weigh", w.ToString());
                        //jsend.Add("createdAt", DateTime.Now.ToString());

                        string sendToCloud = JsonConvert.SerializeObject(jsend);
                        Task cloudTask = AzureIoTHub.SendDeviceToCloudMessageAsync(sendToCloud);

                        await sendTask;
                        uhl.FinishUser();
                        await cloudTask;
                        return;
                    }
                    catch
                    {
                        DRP response = new DRP(DRPDevType.RBPI, msg.UserName, PermanentData.Serial, PermanentData.Devname, 0, 0, DRPMessageType.HARDWARE_ERROR);
                        await tcp.Send(response.ToString(), writer);
                        System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());
                        uhl.FinishUser();
                        return;
                    }

                }
                else
                {
                    //if somwone already uses the weight
                    DRP response = new DRP(DRPDevType.RBPI, msg.UserName, PermanentData.Serial, PermanentData.Devname, 0, 0, DRPMessageType.IN_USE);
                    await tcp.Send(response.ToString(), writer);
                    System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());
                }
            }
            else if (msg.MessageType == DRPMessageType.ILLEGAL)
            {
                DRP response = new DRP(DRPDevType.RBPI, "", PermanentData.Serial, PermanentData.Devname, 0, 0, DRPMessageType.ACK);
                await tcp.Send(response.ToString(), writer);
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
            System.Diagnostics.Debug.WriteLine("There was an error in socket: " + message);
        }
    }
}
