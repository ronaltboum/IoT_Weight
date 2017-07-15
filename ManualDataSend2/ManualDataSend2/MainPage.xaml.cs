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
using Newtonsoft.Json;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ManualDataSend2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.txt_received.Text = "Receiving...";
            string message = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
            this.txt_received.Text = message;
        }


        bool logged = false;
        private async void btn_login_Click(object sender, RoutedEventArgs e)
        {
            if (!logged)
            {
                //currently sends the data to cloud, but accepts any user anyway
                Dictionary<string, string> userData = new Dictionary<string, string>();
                userData.Add("usernname", txt_username.Text);
                userData.Add("password", txt_password.Text);
                string data = JsonConvert.SerializeObject(userData);

                await AzureIoTHub.SendDeviceToCloudMessageAsync(data);
                txt_received.Text = data;

                /* the server should check if the username and password are valid */
                this.txt_received.Text = "Receiving...";
                string message = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();

                if (message.Equals("RST"))
                {
                    txt_received.Text = "Authentication failed.";
                    return;
                }
                else if (!message.Equals("ACK"))
                {
                    txt_received.Text = "Error: Cloud sent unrecognized answer \"" + message + "\"";
                    return;
                }
                /* ============================================================= */
                lbl_loginMsg.Text = "You are now logged in, " + txt_username.Text + "!";
                txt_username.IsReadOnly = true;
                txt_password.IsReadOnly = true;
                btn_login.Content = "Log out";
                logged = true;
            }
            else
            {
                txt_username.IsReadOnly = false;
                txt_password.IsReadOnly = false;
                btn_login.Content = "Login";
                logged = false;
            }

        }

        private async void btn_upload_Click(object sender, RoutedEventArgs e)
        {
            if (!logged)
            {
                txt_received.Text = "You must be logged on.";
                return;
            }
            Dictionary<string, string> userData = new Dictionary<string, string>();
            userData.Add("username", txt_username.Text);
            userData.Add("weight", txt_enterWeight.Text);
            userData.Add("fat", txt_enterFat.Text);

            string data = JsonConvert.SerializeObject(userData);
            await AzureIoTHub.SendDeviceToCloudMessageAsync(data);
            txt_received.Text = data;
        }

        private async void btn_example_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> userData = new Dictionary<string, string>();
            userData.Add("username", txt_username.Text);
            userData.Add("weigh", txt_enterWeight.Text);
            userData.Add("createdAt",DateTime.Now.ToString());

            string data = JsonConvert.SerializeObject(userData);
            await AzureIoTHub.SendDeviceToCloudMessageAsync(data);
            txt_received.Text = data;
        }
    }
}
