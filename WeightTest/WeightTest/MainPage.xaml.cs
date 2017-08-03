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
using System.Diagnostics;
using ManualDataSend2;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WeightTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const byte DOUT_PIN = 24;
        const byte SLK_PIN = 23;

        private GpioPin dout;
        private GpioPin slk;
        private GpioController gpio;

        public MainPage()
        {
            gpio = GpioController.GetDefault();
            dout = gpio.OpenPin(DOUT_PIN);
            slk = gpio.OpenPin(SLK_PIN);
            hx711 = new HX711(slk, dout);
            this.InitializeComponent();
        }

        HX711 hx711;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            float knownWeight = float.Parse(txt_knownWeight.Text);

            hx711.calibrate(knownWeight);

            Debug.WriteLine("calibrated");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            float g;
            //hx711.PowerOn();
            for (int i = 0; i < 25; i++)
            {
                g = hx711.PoweOnAndGetGram();
                Debug.WriteLine(g);
                System.Threading.Tasks.Task.Delay(100);
            }
            //hx711.PowerDown();
            Debug.WriteLine("===");
        }
    }
}
