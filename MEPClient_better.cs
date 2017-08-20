using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.WindowsAzure.MobileServices;
using weighJune28;

namespace ManualDataSend2
{
    class MEPClient
    {

        private uint myIp;
        private long myMac;
        private long raspberryMAC;

        //list of nearby Raspberry IPs
        private List<long> nearbyRasps;
        private Queue<string> unhandledMessages;


        public MEPClient(uint myIp, long myMac, List<long> nearbyRasps)
        {
            this.myIp = myIp;
            this.myMac = myMac;
            this.nearbyRasps = new List<long>(nearbyRasps);
            this.unhandledMessages = new Queue<string>();
        }

        public uint MyIp { get => myIp; set => myIp = value; }
        public long MyMac { get => myMac; set => myMac = value; }
        public long RaspberryMAC { get => raspberryMAC; set => raspberryMAC = value; }
        public List<long> NearbyRasps { get => new List<long>(nearbyRasps); }


        //sends a MEP to all nearby Raspberries 
        private void sendMepsToNearbyRasps()
        {
            foreach (long rasp in nearbyRasps)
            {
                MEP MEPmessage = new MEP(MEPDevType.APP, MyMac, MyIp, rasp, MEPCallbackAction.Response);
                sendMEP(MEPmessage);
            }

        }

        private const int TIMEOUT = 5000;

        public void run()
        {
            sendMepsToNearbyRasps();
            listen();
        }


        private bool isValidMep(string msg)
        {
            return msg.IndexOf("$MEP") >= 0;
        }

        private void listen()
        {
            while (true)
            {
                Task<string> msg_task = receiveMessage();
                if (!msg_task.Wait(TIMEOUT))
                {
                    Debug.WriteLine("No more messages received");
                    waitingForMessages = false;
                    break;
                }
                string msg = msg_task.Result;
                if (!isValidMep(msg))
                {
                    unhandledMessages.Enqueue(msg);
                    continue;
                }

                MEP mep = mep.deserializeMEP(msg);
                raspberryMAC = mep.MacAddr;

                if (msg.CallbackAction == MEPCallbackAction.Respnse)
                {
                    MEP response = new MEP(MEPDevType.APP, myMac, myIp, raspberryMAC, MEPCallbackAction.NoOperation);
                    sendMEP(response);
                }
                else if (msg.CallbackAction == MEPCallbackAction.Acknowledge)
                {
                    MEP response = new MEP(MEPDevType.APP, 0, 0, 0, MEPCallbackAction.NoOperation);
                    sendMEP(response);
                }
            }
        }

        /**
         * Receive MEP messages from the cloud
         */
        private async Task<MEP> receiveMEP()
        {
            string res = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
            return MEP.deserializeMEP(res);
        }


        /**
         * Send MEP Message to the cloud
         */
        private async void sendMEP(MEP message)
        {
            message.timeStamp();
            await AzureIoTHub.SendDeviceToCloudMessageAsync(message.ToString());
        }
    }

}