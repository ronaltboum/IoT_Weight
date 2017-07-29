using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManualDataSend2
{
    class MEPMessageHandler
    {
        public static async void sendMEP(MEP message)
        {
            await AzureIoTHub.SendDeviceToCloudMessageAsync(message.ToString());
        }
        public static async Task<MEP> receiveMEP()
        {
            string res = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
            return MEP.parseMEP(res);
        }
    }
}
