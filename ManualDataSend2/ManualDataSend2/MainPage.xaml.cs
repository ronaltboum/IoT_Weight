using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Newtonsoft.Json;
using Windows.Devices.Gpio;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ManualDataSend2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        HX711 hx711;

        const byte DOUT_PIN = 24;
        const byte SLK_PIN = 23;

        private GpioPin dout;
        private GpioPin slk;
        private GpioController gpio;

        private Profile profile = null;

        private CloudMessagesListener cml;
        public MainPage()
        {
            gpio = GpioController.GetDefault();
            dout = gpio.OpenPin(DOUT_PIN);
            slk = gpio.OpenPin(SLK_PIN);
            hx711 = new HX711(slk, dout);
            cml = new CloudMessagesListener(0, 0);

           // HTTPServer httpserver = new HTTPServer("y");
           // httpserver.start();

            this.InitializeComponent();


           // MEP msg = new MEP(MEPDevType.RBPI, 0x112233445566l, 0xffeeddcc, 0x66776677, MEPCallbackAction.Acknowledge);

        }
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //this.txt_received.Text = "Receiving...";
            //string message = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
            //this.txt_received.Text = message;
            Queue<MEP> messages = cml.MepServer.History;
            txt_msglog.Text = "==\n";
            foreach(MEP message in messages)
            {
                txt_msglog.Text += message.ToString() + "\n";
            }

        }

        
        private long addrToLong(string addr)
        {
            string[] bytes = addr.Split(':');
            long result = 0;
            long mult = 1;
            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                result += long.Parse(bytes[i], System.Globalization.NumberStyles.HexNumber) * mult;
                mult *= 256;
            }
            return result;
        }
        private async void btn_upload_Click(object sender, RoutedEventArgs e)
        {
            if(profile == null)
            {
                MessageDialog dialog = new MessageDialog("Profile not Set","Error");
                await dialog.ShowAsync();
                return;
            }

            string data = JsonConvert.SerializeObject(profile.ToString());
            await AzureIoTHub.SendDeviceToCloudMessageAsync(data);
        }
        private async void btn_example_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> userData = new Dictionary<string, string>();
            userData.Add("username", txt_username.Text);
            userData.Add("weigh", txt_enterWeight.Text);
            userData.Add("createdAt", DateTime.Now.ToString());

            string data = JsonConvert.SerializeObject(userData);
            await AzureIoTHub.SendDeviceToCloudMessageAsync(data);

            MessageDialog dialog = new MessageDialog(data);
            await dialog.ShowAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (profile == null)
            {
                return;
            }
            profile.Weight = hx711.PoweOnAndGetGram();
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            hx711.calibrate(int.Parse(txt_enterWeight.Text));

            MessageDialog dialog = new MessageDialog("Callibrated.");
            await dialog.ShowAsync();
        }


        private void Button_getMac_Click(object sender, RoutedEventArgs e)
        {
            //TODO: get Real Data!
            foreach(long mac in cml.MepServer.UsersNearby.Keys)
                profile.addMAC(mac);
        }

        private void btn_register_Click(object sender, RoutedEventArgs e)
        {
            if (profile == null)
                profile = new Profile("", 0, 0, new List<long>());
            profile.Username = txt_username.Text;
            profile.Weight = float.Parse(txt_enterWeight.Text);
            profile.Fat = float.Parse(txt_enterFat.Text);
        }

        private void btn_show_Click(object sender, RoutedEventArgs e)
        {
            //if (profile == null)
            //    return;
            //txt_username.Text = profile.Username;
            //txt_enterWeight.Text = profile.Weight.ToString();
            //txt_enterFat.Text = profile.Fat.ToString();

            txt_around.Text = "==\n";
            string ip, mac;
            foreach (KeyValuePair<long,uint> user in cml.MepServer.UsersNearby)
            {
                mac = AddressConvert.MACtoRepresentalString(user.Key);
                ip = AddressConvert.IPtoRepresentalString(user.Value);
                txt_around.Text += mac + " - " + ip + "\n";
            }
        }

        private void btn_listen_Click(object sender, RoutedEventArgs e)
        {
            Task task = Task.Run((Action)cml.start);
            
        }
    }
}
