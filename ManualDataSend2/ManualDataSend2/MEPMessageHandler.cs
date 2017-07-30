using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManualDataSend2
{
    /**
     * Handling sending and receiving MEP Messages
     */
    class MEPMessageHandler
    {
        /**
         * Send MEP message
         */
        public static async void sendMEP(MEP message)
        {
            message.timeStamp();
            await AzureIoTHub.SendDeviceToCloudMessageAsync(message.ToString());
        }
        /**
         * Receive MEP Message
         * NOTE: This method does not send response!
         */
        public static async Task<MEP> receiveMEP()
        {
            string res = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
            return MEP.deserializeMEP(res);
        }
        /**
         * Send MEP message and wait for response.
         * NOTE: This method does not send response!
         * If the message's callback action is NOP, 'null' is returned.
         */
        public static async Task<MEP> sendReceive(MEP message)
        {
            sendMEP(message);
            if (message.CallbackAction == MEPCallbackAction.NoOperation)
                return null;
            return await receiveMEP();
        }
        /**
        * Listen to incoming messages and response them.
        */
        public static async Task<MEP> listen(long mac, uint ip)
        {
            MEP income = await receiveMEP();
            MEP response = new MEP(MEPDevType.RBPI, mac, ip, MEPCallbackAction.NoOperation);

            MEPCallbackAction callback = income.CallbackAction;

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
                    break;

            }

            return income;
        }
    }
}
