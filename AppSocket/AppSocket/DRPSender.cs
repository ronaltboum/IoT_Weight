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

namespace AppSocket
{
    class DRPSender: TCPSender
    {
        public DRPSender(int port) : base(port) { }
        public DRPSender(): base() { }

        public void SendDRP(object msg)
        {
            /*
             * Add code here so that this method will convert a DRP message into a string so it could be sent away.
             * You also need to change 'msg' type be DRP object
             */
            base.Send(msg as string);
        }
        public object ReceiveDRP()
        {
            string str = base.Receive();
            /*
            * write a code to convert the string str to a DRP message and return it
            * You also need to change the method's return-type to be DRP object
            */
            return (object)str;
        }
    }
}