using Android.App;
using Android.Widget;
using Android.OS;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.Text;

namespace AppSocket
{
    [Activity(Label = "AppSocket", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            TCPSender tcps = new TCPSender(9000);

            tcps.Connect("192.168.1.106");
            tcps.Send("ohayo!");
            string msg = tcps.Receive();

            System.Diagnostics.Debug.WriteLine(msg);
        }
    }
}