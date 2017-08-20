using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ManualDataSend2
{
	class MEPClient{
		
		private uint myIp;
        private long myMac;
        private long raspberryMAC;
		
		//list of nearby Raspberry IPs
		private LinkedList<long> nearbyRasps;
		
		public uint MyIp { get => myIp; set => myIp = value; }
        public long MyMac { get => myMac; set => myMac = value; }
		public long RaspberryMAC { get => raspberryMAC; set => raspberryMAC = value; }
		
		private IMobileServiceTable<RaspberryTable> RaspberryTableRef;
		
		
		
		
		
		private LinkedList<long> getNearbyRasps{
			
		
       
            
            
        // Create a new instance field for this activity.
		}
		
		//sends a MEP to all nearby Raspberries 
		private async void sendMepsToNearbyRasps(){
			foreach (long rasp in nearbyRasps){
				MEP MEPmessage = new MEP (MEPDevType.APP, MyMac, MyIp, rasp, MEPCallbackAction.Response);
				sendMEP(MEPmessage);
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

		
		
		
		