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

namespace hx711onUWP
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
            this.InitializeComponent();
            //comb = new Hx711(DOUT_PIN, SLK_PIN);
            scond = new AviaHX711(slk, dout);
            System.Diagnostics.Debug.WriteLine("HX711 Initilized");
        }
        AviaHX711 scond;
        //IHx711 comb;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            scond.PowerOn();

            float w = scond.GetGram();
            scond.PowerDown();
            System.Diagnostics.Debug.WriteLine(w);
            this.txt_weight.Text = (w*100).ToString() + " g";
            
        }

        private void Button_Calibrate_Click(object sender, RoutedEventArgs e)
        {
            scond.PowerOn();
            scond.AverageValue();
            scond.SetOffset(scond.AverageValue());
            scond.PowerDown();
            this.txt_weight.Text = "Calibrated";
        }
    }
}
