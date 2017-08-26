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
using System.Threading.Tasks;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WeightTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const byte DOUT_PIN = 26;
        const byte SLK_PIN = 19;

        private GpioPin dout;
        private GpioPin slk;
        private GpioController gpio;
        private HX711_bogde hx711b;

        public MainPage()
        {
            gpio = GpioController.GetDefault();
            slk = gpio.OpenPin(SLK_PIN);
            dout = gpio.OpenPin(DOUT_PIN);
            hx711b = new HX711_bogde(dout, slk);

            this.InitializeComponent();
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*
             * How to Calibrate Your Scale:
             * 1. Call set_scale() with no parameter.
             * 2. Call tare() with no parameter.
             * 3. Place a known weight on the scale and call get_units(10).
             * 4. Divide the result in step 3 to your known weight. You should get about the parameter you need to pass to set_scale.
             * 5. Adjust the parameter in step 4 until you get an accurate reading.
            */
            txt_results.Text = "";

            float knownWeight = float.Parse(txt_knownWeight.Text);

            appendLine("==Calibration==", txt_results);

            hx711b.set_scale();
            hx711b.tare();
            float g = hx711b.get_units(10);
            g = g / knownWeight;
            appendLine("", txt_results);
            appendLine("Your weight scale is", txt_results);
            appendLine(g.ToString(), txt_results);
            txt_scale.Text = g.ToString();
        }

        private void writeText(string text, TextBox textbox)
        {
            textbox.Text = text;
        }


        /*
        private void Button_Click_1Async(object sender, RoutedEventArgs e)
        {

            Button_Click_1(sender, e);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            gpio = GpioController.GetDefault();
            dout = gpio.OpenPin(DOUT_PIN);
            slk = gpio.OpenPin(SLK_PIN);

            //dout.Write(GpioPinValue.High);
            //dout.Write(GpioPinValue.High);
            //while (true)
            //{

            //}

            hx711 = new HX711(slk, dout);

            float g;
            hx711.PowerOn();
            txt_results.Text = "";
            for (int i = 0; i < 25; i++)
            {
                g = hx711.GetGram() - 16842.14f;
                Debug.WriteLine(g);
                txt_results.Text += g.ToString() + "\n";
            }
            hx711.PowerDown();
            Debug.WriteLine("===");
        }
        */
            
        private void appendLine(string text, TextBox tb)
        {
            tb.Text += text + "\n";
            Debug.WriteLine(text);
        }

        private void Button_weigh2_Click(object sender, RoutedEventArgs e)
        {
            txt_results.Text = "";

            appendLine("==Weighting Results==", txt_results);

            // print a raw reading from the ADC
            appendLine("Raw Data:", txt_results);
            appendLine(hx711b.read().ToString(), txt_results);
            appendLine(" ", txt_results);

            // print the average of 20 readings from the ADC
            appendLine("Avg of 20 readings:", txt_results);
            appendLine(hx711b.read().ToString(), txt_results);
            appendLine(" ", txt_results);

            // print the average of 5 readings from the ADC minus the tare weight (not set yet)
            appendLine("Data after scaling:", txt_results);
            appendLine(hx711b.get_units(5).ToString(), txt_results);
            appendLine(" ", txt_results);

            hx711b.set_scale(float.Parse(txt_scale.Text));
            hx711b.tare();

            appendLine("==Calibrated==", txt_results);
            appendLine("Calculating data again.", txt_results);

            // print a raw reading from the ADC
            appendLine("*Raw Data:", txt_results);
            appendLine(hx711b.read().ToString(), txt_results);
            appendLine(" ", txt_results);

            // print the average of 20 readings from the ADC
            appendLine("*Avg of 20 readings:", txt_results);
            appendLine(hx711b.read().ToString(), txt_results);
            appendLine(" ", txt_results);

            // print the average of 5 readings from the ADC minus the tare weight (not set yet)
            appendLine("*Data after scaling:", txt_results);
            appendLine(hx711b.get_units(5).ToString(), txt_results);
            appendLine(" ", txt_results);

        }
    }
}
