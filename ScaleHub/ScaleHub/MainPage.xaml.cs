using Microsoft.WindowsAzure.MobileServices;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ScaleHub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string applicationURL = @"https://iotweight.azurewebsites.net";
        const string WELCOME_MSG = "@welcome@";
        const int PORT = 9888;
        List<RaspTableWithDevname> listItems;
        public MainPage()
        {
            this.InitializeComponent();
            listItems = new List<RaspTableWithDevname>();
           

            lv_scalesnearby.ItemsSource = listItems;
            Task.Run(() => MainPageAsync());
        }

        public async Task MainPageAsync()
        {
            List<RaspberryTable> rasps = await getListOfRasps();

            //FilterByMask(rasps, "192.168.1.0", 24);

            Dictionary<RaspberryTable,Task<string>> connections = new Dictionary<RaspberryTable, Task<string>>();
            foreach (RaspberryTable rasp in rasps)
            {
                connections.Add(rasp,tryConnect(rasp.IPAddress));
            }


            await Task.WhenAll(connections.Values);

            int count = 0;
            foreach(RaspberryTable rasp in rasps)
            {
                if (connections[rasp].Result != null)
                {
                    System.Diagnostics.Debug.WriteLine(connections[rasp].Result + " (" + rasp.QRCode + ") -- " + rasp.IPAddress);
                    listItems.Add(RaspTableWithDevname.initFromTable(rasp, connections[rasp].Result));
                    count++;
                }
            }

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                lv_scalesnearby.ItemsSource = new List<RaspTableWithDevname> (listItems);
                tb_wait.Text = count + " devices found.";
            });

            System.Diagnostics.Debug.WriteLine("That's all folks.");
        }

        
        public async Task<List<RaspberryTable>> getListOfRasps()
        {
            var client = new MobileServiceClient(applicationURL);
            IMobileServiceTable<RaspberryTable> raspberryTableRef = client.GetTable<RaspberryTable>();
            try
            {
                List<RaspberryTable> raspsList = await raspberryTableRef.ToListAsync();
                return raspsList;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return null;
            }
        }

        public uint addrToUint(string addr)
        {
            string[] strbytes = addr.Split('.');
            uint addrbits = 0;
            uint mul = 1;
            for (int i = 0; i < 4; i++)
            {
                addrbits += byte.Parse(strbytes[3 - i]) * mul;
                mul *= 256;
            }
            return addrbits;
        }

        public void FilterByMask(List<RaspberryTable> addrs, string mask, int length)
        {
            uint bmask = addrToUint(mask);
            uint baddr;
            uint setmask;
            string addr;
            for(int i=addrs.Count-1;i>=0;i--)
            {
                addr = addrs[i].IPAddress;
                baddr = addrToUint(addr);
                baddr >>= (32-length);
                setmask = bmask >> (32-length);

                if(setmask == baddr)
                {
                    //System.Diagnostics.Debug.WriteLine(addr + " is in subnet " + mask);
                }
                else
                {
                   // System.Diagnostics.Debug.WriteLine(addr + " NOT in subnet " + mask);
                    addrs.RemoveAt(i);
                }
            }
        }

        public async Task<string> tryConnect(string ip)
        {
            TCPSender tcps = new TCPSender(PORT);
            bool hasConnection = await tcps.Connect(ip,3000);
            if (!hasConnection)
                return null;
            await tcps.Send(WELCOME_MSG);
            string[] res = (await tcps.Receive()).Split('=');
            if (res.Length != 2 || !res[0].Equals("TAUIOT@devname"))
                return null;
            else
                return res[1];
        }

        private void lv_scalesnearby_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            
        }

        private void lv_scalesnearby_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
