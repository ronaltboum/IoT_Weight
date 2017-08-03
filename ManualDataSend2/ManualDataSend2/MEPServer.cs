using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
namespace ManualDataSend2
{
    /**
     * This class listens to incoming MEP messages, analyze and answer them.
     */
    class MEPServer
    {
        private uint myIp;
        private long myMac;

        private Queue<MEP> history; //history of received messages
        private Dictionary<long, uint> usersNearby; //MAC and IP of nearby users

        private bool running;

        public uint MyIp { get => myIp; set => myIp = value; }
        public long MyMac { get => myMac; set => myMac = value; }
        internal Queue<MEP> History { get => new Queue<MEP>(history); }
        public Dictionary<long, uint> UsersNearby { get => new Dictionary<long, uint>(usersNearby); }

        public const int RESPONSE_TIMEOUT = 5000; //If the addressee does not answer within 5 second, then it assumed to be dead.
        public const long BROADCAST_ADDR = 0; // address for broadcasting
        public MEPServer(long myMac, uint myIp)
        {
            this.MyMac = myMac;
            this.MyIp = myIp;

            history = new Queue<MEP>();
            usersNearby = new Dictionary<long, uint>();

            running = false;
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

        


        /**
         * Remove a device with a given MAC address from the list (i.e. the device was probably disconnected)
         */
        private void declareDead(long MAC)
        {
            usersNearby.Remove(MAC);
        }

        /**
         * stop receiving MEP messages
         */
        public void stop()
        {
            running = false;
        }

        /**
         * Refreshing the 'UsersNearby' table.
         * Removing all entries from the table and then send MEP message for all devices nearby
         */
        public async void sayHello()
        {
            usersNearby.Clear();
            MEP boardcast = new MEP(MEPDevType.RBPI, myMac, myIp, BROADCAST_ADDR, MEPCallbackAction.Response);
            await AzureIoTHub.SendDeviceToCloudMessageAsync(boardcast.ToString());
            //incoming response will be taken care by the server (Start() func)
        }

        /**
         * starting the MEP Server
         * This method will run in the backround and wait for incoming messages.
         * When a message is received, registers the MAC and IP addersses and sends a response.
         * This method will run untill Stop() method is called.
         */
        public void start()
        {
            MEP response;
            MEPCallbackAction callback;
            Task<MEP> income_task;
            MEP income;

            bool gotmsg;

            running = true;

            while (running)
            {
                income_task = receiveMEP();
                gotmsg = income_task.Wait(RESPONSE_TIMEOUT * 2);
                if (!gotmsg) //every 10 seconds, if no messages was received, refresh.
                {
                    Debug.WriteLine("No massage was received in the last 10 seconds");
                    continue;
                }
                income = income_task.Result;
                Debug.WriteLine("Got message: " + income.ToString());

                response = new MEP(MEPDevType.RBPI, MyMac, MyIp, income.MacAddr, MEPCallbackAction.NoOperation);
                callback = income.CallbackAction;

                //register the MAC and IP that has received
                usersNearby.Add(income.MacAddr, income.IpAddr);
                history.Enqueue(income);

                //answering the messages (if needed)
                if (income.Addressee == myMac || (income.Addressee == 0 && income.MacAddr != myMac))
                {
                    switch (callback)
                    {
                        case MEPCallbackAction.Response:
                            response.CallbackAction = MEPCallbackAction.Acknowledge;
                            sendMEP(response);
                            break;
                        case MEPCallbackAction.Acknowledge:
                            response.CallbackAction = MEPCallbackAction.NoOperation;
                            response.IpAddr = 0;
                            response.MacAddr = 0;
                            sendMEP(response);
                            break;
                        case MEPCallbackAction.NoOperation:
                            income = null;
                            break;
                    }
                }
            }
        }

        /*
        private async Task<MEP> sendMEPAndGetResponse(MEP message)
        {
            message.timeStamp();
            await AzureIoTHub.SendDeviceToCloudMessageAsync(message.ToString());
            if (message.CallbackAction != MEPCallbackAction.NoOperation)
            {
                MEP res = await receiveMEP();
                return res;
            }
            else
                return null;
        }*/

        /*
        public void _start()
        {
            MEP response;
            MEPCallbackAction callback;
            running = true;

            Task<MEP> income_task;
            MEP income;

            bool gotmsg;
            while (running)
            {
                //get incoming message
                income_task = receiveMEP();
                gotmsg = income_task.Wait(RESPONSE_TIMEOUT * 2);
                if (!gotmsg) //every 10 seconds, if no messages was received, refresh.
                {
                    Debug.WriteLine("No massage was received in the last 10 seconds");
                    continue;
                }
                income = income_task.Result;
                Debug.WriteLine("Got message: " + income.ToString());
                do //while there are no more messages waiting
                {
                    response = new MEP(MEPDevType.RBPI, MyMac, MyIp, MEPCallbackAction.NoOperation);
                    callback = income.CallbackAction;

                    //register the MAC and IP that has received
                    usersNearby.Add(income.MacAddr, income.IpAddr);
                    history.Enqueue(income);

                    //answering the messages (if needed)
                    switch (callback)
                    {
                        case MEPCallbackAction.Response:
                            response.CallbackAction = MEPCallbackAction.Acknowledge;
                            income_task = sendMEPAndGetResponse(response);
                            gotmsg = income_task.Wait(RESPONSE_TIMEOUT);
                            if (!gotmsg) //if got no response, assuming the devices was disconnected.
                            {
                                Debug.WriteLine("No response from device with MAC " + income.MacAddr.ToString("X12"));
                                declareDead(income.MacAddr);
                                continue;
                            }
                            income = income_task.Result;
                            break;
                        case MEPCallbackAction.Acknowledge:
                            response.CallbackAction = MEPCallbackAction.NoOperation;
                            response.IpAddr = 0;
                            response.MacAddr = 0;
                            gotmsg = income_task.Wait(RESPONSE_TIMEOUT);
                            if (!gotmsg) //if got no response, assuming the devices was disconnected.
                            {
                                Debug.WriteLine("No response from device with MAC " + income.MacAddr.ToString("X12"));
                                declareDead(income.MacAddr);
                                continue;
                            }
                            income = income_task.Result;
                            break;
                        case MEPCallbackAction.NoOperation:
                            income = null;
                            break;
                    }
                } while (income != null);
            }*/
    }
}
}
