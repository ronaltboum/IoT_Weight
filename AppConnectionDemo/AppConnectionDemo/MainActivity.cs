using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AppConnectionDemo
{
    [Activity(Label = "AppConnectionDemo", MainLauncher = true)]
    public class MainActivity : Activity
    {
        TCPSender tcps;

        EditText et;
        Button btn;
        TextView tv;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);


            et = FindViewById<EditText>(Resource.Id.editText1);
            btn = FindViewById<Button>(Resource.Id.button1);
            tv = FindViewById<TextView>(Resource.Id.textView1);

            btn.Click += Btn_Click;

            tcps = new TCPSender();

           
        }

        private async void Btn_Click(object sender, System.EventArgs e)
        {
            /*
             * TODO: the code for scanning the QR goes here.
             */

            string ip = findIpFromSerial("[QR scan result]");
            if (!tcps.Connect(ip))
            {
                System.Diagnostics.Debug.WriteLine("Connection failed.");
                return;
            }

            DRP result = await sendSCANNED(long.Parse("55665566"));
            if(result == null)
            {
                tv.Text = "Connection Timeout";
                return;
            }
            if (result.MessageType == DRPMessageType.DATA)
            {
                //TODO: Show the scaling result on the screen
                tv.Text = result.Data[0].ToString();
            }
            else if (result.MessageType == DRPMessageType.IN_USE)
            {
                //TODO: Show a message for the user that informs him the device is already in use by another user.
                tv.Text = "the scale is in use";
            }
            else if (result.MessageType == DRPMessageType.ILLEGAL || result.MessageType == DRPMessageType.HARDWARE_ERROR)
            {
                //TODO: The scaling could not been done due to error.
                tv.Text = "The scaling could not been done due to error.";
            }
            //TODO: send ACKs (we'll do it later)
        }

        private async Task<DRP> sendSCANNED(long serial, int timeout = 10000)
        {
            DRP msg = new DRP(DRPDevType.APP, "a_monkey", 343434, serial, new List<float>(), 0, DRPMessageType.SCANNED); //TODO: what is the username? what is the serial?
            tcps.Send(msg.ToString());
            Task<string> rec_str_task = tcps.Receive();
            if (rec_str_task.Wait(timeout))
            {
                string rec_str = rec_str_task.Result;
                DRP rec = DRP.deserializeDRP(rec_str); //TODO: do not assume for DRP, needs to be checked!
                return rec;
            }
            else
            {
                return null;
            }
        }

        private string findIpFromSerial(string serial)
        {
            //TODO: I've hardcoded my RPi ip here. In the final version we need to look for the ip in the DB
            return "192.168.1.104";
        }
    }
}

