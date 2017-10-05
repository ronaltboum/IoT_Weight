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
using Android.Webkit;

namespace IoTWeight
{
    [Activity(Label = "What would you like to do ?")]
    public class LogInActivity : Activity
    {
        public static MobileServiceUser user;
        public static MobileServiceClient client;
        const string applicationURL = @"https://iotweight.azurewebsites.net";

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.LogInWelcome);

            client = ToDoActivity.CurrentActivity.CurrentClient;
            user = new MobileServiceUser(ToDoActivity.CurrentActivity.Currentuserid);


            // Create your application here
            Button getStatsButton = FindViewById<Button>(Resource.Id.GetStats);
            getStatsButton.SetBackgroundColor(Android.Graphics.Color.SteelBlue);
            Button startWeighButton = FindViewById<Button>(Resource.Id.StartWeigh);
            startWeighButton.SetBackgroundColor(Android.Graphics.Color.LightSkyBlue);
            Button BMIButton = FindViewById<Button>(Resource.Id.BMI);
            BMIButton.SetBackgroundColor(Android.Graphics.Color.LightBlue);
            Button logoutButton = FindViewById<Button>(Resource.Id.logout);
            getStatsButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(GetStatsChooseDisplay));
                StartActivity(intent);
            };

            startWeighButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(QRScan));
                StartActivity(intent);
            };

            BMIButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(CalculateBMI));
                StartActivity(intent);
            };

            logoutButton.Click += async (sender, e) =>
            {
                try
                {
                    if (user != null)
                    {
                        CookieManager.Instance.RemoveAllCookie();
                        await client.LogoutAsync();
                        CreateAndShowDialog(string.Format("You are now logged out - {0}", user.UserId), "Logged out!");
                    }
                    user = null;
                }
                catch (Exception ex)
                {
                    CreateAndShowDialog(ex.Message, "Logout failed");
                }



                var intent = new Intent(this, typeof(ToDoActivity));
                StartActivity(intent);
            };
        }
        void CreateAndShowDialog(string message, string title)
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.SetNeutralButton("OK", (sender, args) => {
            });
            builder.Create().Show();
        }
    }
}