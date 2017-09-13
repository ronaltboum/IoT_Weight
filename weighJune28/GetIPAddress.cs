using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices;
using Java.Util;
using Java.Net;

namespace weighJune28
{
    [Activity(Label = "GetIPAddress")]
    public class GetIPAddress : Activity
    {
        private IMobileServiceTable<RaspberryTable> raspberryTableRef;
        uint ipAddress = 0;
        float currentWeigh = 0;   //to be returned from Raspberry

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            // This activity receives from QRActivity a string which is the Raspberry's QRCode,  and returns this Raspberry's IP Address
            SetContentView(Resource.Layout.DisplayWeigh);

            string qrCode = Intent.GetStringExtra("qrcode") ?? "QR Code not available";
            //TODO:  DELETE LATER:
            qrCode = "Testing 1";
    
            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            raspberryTableRef = client.GetTable<RaspberryTable>();
            try
            {
                //some inserts for debugging:
                //var record1 = new RaspberryTable
                //{
                //    QRCode = "Testing 1",
                //    IPNumber = 350,
                //};
                //await raspberryTableRef.InsertAsync(record1);

                var ipAddressList = await raspberryTableRef.Where(item => (item.QRCode == qrCode) ).ToListAsync();
                //TODO:  handle this case
                if (ipAddressList.Count == 0)
                {
                    CreateAndShowDialog("Sorry:", "No Raspberries with the scanned QR Code were found in the database ");
                }
                
                else 
                {
                    var address = ipAddressList[0];
                    ipAddress = address.IPNumber;
                    // TODO:   start function that sends protocol messages to Raspberry
                    string message = "Ip address of qr code " + qrCode + ": " + ipAddress;
                    //CreateAndShowDialog( message, "Debugg: ");
                    ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.ProgressCircle);
                    progress.Visibility = ViewStates.Gone;
                    FindViewById<TextView>(Resource.Id.Text1).Text = "Your Current Weight:";
                    //TODO :  DELETE LATER:
                    currentWeigh = 97.4f;
                    FindViewById<TextView>(Resource.Id.currentWeigh).Text = Convert.ToString(currentWeigh);

                }
                //TODO:   handle case where there are more than one ip address for the given QR Code.  
                //Decide whether the Raspberry (at the installation process), or the 
                //application will delete the old ip addresses
                //(in case the Raspberry was moved to a place with a different ip address)

            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }



        }


        private void CreateAndShowDialog(Exception exception, String title)
        {
            CreateAndShowDialog(exception.Message, title);
        }

        private void CreateAndShowDialog(string message, string title)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }
    }
}