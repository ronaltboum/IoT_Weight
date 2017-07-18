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
using System.Diagnostics;
using Windows.Devices.Gpio;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Laboratory
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            //this.InitializeComponent();
            Debug.WriteLine("==Laboratory==");

            GpioController gpio = GpioController.GetDefault();
            GpioPin clock = gpio.OpenPin(4);
            
            clock.SetDriveMode(GpioPinDriveMode.Output);

            while (true)
            {
                clock.Write(GpioPinValue.High);
                clock.Write(GpioPinValue.Low);
                System.Threading.Tasks.Task.Delay(100);
            }

        }
    }
}
