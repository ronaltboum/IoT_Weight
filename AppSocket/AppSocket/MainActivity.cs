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
        Button btn;
        TextView tv;
        TCPSender tcps;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            tcps = new TCPSender(9000);
            btn = FindViewById<Button>(Resource.Id.button1);
            tv = FindViewById<TextView>(Resource.Id.textView1);

            btn.Click += Btn_Click;

        }

        private void Btn_Click(object sender, EventArgs e)
        {
            tcps.Connect("192.168.1.104");
            tcps.Send("dracarys!");
            string msg = tcps.Receive();
            tv.Text = msg;
        }
    }
}