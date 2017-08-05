using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
namespace ManualDataSend2
{
    class CloudMessagesListener
    {
        private Queue<string> unhandledMessages;
        private bool isRunning;
        private MEPServer mepServer;

        public const int RESPONSE_TIMEOUT = 5000; //If the addressee does not answer within 5 second, then it assumed to be dead.

        public Queue<string> UnhandledMessages { get => new Queue<string>(unhandledMessages); }
        public bool IsRunning { get => isRunning;  }
        public MEPServer MepServer { get => mepServer; }

        public CloudMessagesListener(long myMac, uint myIp)
        {
            unhandledMessages = new Queue<string>();
            isRunning = false;
            mepServer = new MEPServer(myMac, myIp);
        }

        public void start()
        {
            isRunning = true;
            Debug.WriteLine("Server is running");
            while (IsRunning)
            {
                Task<string> msg_task = AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
                if (!msg_task.Wait(RESPONSE_TIMEOUT))
                {
                    Debug.WriteLine("No massage was received in the last few seconds.");
                    continue;
                }
                string msg = msg_task.Result;
                if(msg.ToUpper().IndexOf("$MEP") >= 0)
                {
                    Debug.WriteLine("MEP message received:");
                    Debug.WriteLine(msg);
                    mepServer.handleMEPMessage(MEP.deserializeMEP(msg));
                }
                else
                {
                    Debug.WriteLine("Unknown message received:" +
                        "");
                    Debug.WriteLine(msg);
                    unhandledMessages.Enqueue(msg);
                }
            }
        }
        public void stop()
        {
            isRunning = false;
        }
    }
}
