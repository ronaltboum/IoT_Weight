using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;

namespace ManualDataSend2
{
    /**
     * Represents a DRP message according to the DRP protocol
     **/
    [JsonObject(MemberSerialization.OptIn)]
    class DRP
    {
        private string protocol;
		private DRPDevType devType;
        private string userName;
        private long RBID;
        private DRPData data;
		private int token;
        private DRPMessageType drpMessageType;
        private DateTime date;

        /* Setters & Getters */
        public string Protocol {get => protocol; set=> protocol = value; }
		public DRPDevType devType { get => devType; set => devType = value; }
        public long MacAddr { get => macAddr; set => macAddr = value; }
        public uint IpAddr { get => ipAddr; set => ipAddr = value; }
        public long Addressee { get => addressee; set => addressee = value; }
        public MEPCallbackAction CallbackAction { get => callbackAction; set => callbackAction = value; }
        public DateTime Date { get => date; set => date = value; }

        /* Setters & Getters for JSON */
       /* [JsonProperty(PropertyName = "Protocol")]
        private string JProtocol { get => "$MEP"; }
        [JsonProperty(PropertyName = "DevType")]
        private string JDevType { get => stringer(devType); set => devType = parseDevType(value); }
        [JsonProperty(PropertyName = "MacAddr")]
        private string JMacAddr { get => macAddr.ToString("X"); set => macAddr = long.Parse(value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "IpAddr")]
        private string JIpAddr { get => ipAddr.ToString("X"); set => ipAddr = uint.Parse(value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "Addressee")]
        private string JAddressee { get => addressee.ToString("X"); set => addressee = uint.Parse(value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "CallbackAction")]
        private string JCallbackAction { get => stringer(callbackAction); set => callbackAction = parseCallbackAction(value); }
        [JsonProperty(PropertyName = "Date")]
        private string JDate { get => date.ToString(); } */

        /** constructors **/

        //Empty constructor is needed for deserialize JSON
        //private DRP() { }

        /*public DRP(MEPDevType devType, long macAddr, uint ipAddr, long addressee, MEPCallbackAction callbackAction)
        {
            this.devType = devType;
            this.IpAddr = ipAddr;
            this.MacAddr = macAddr;
            this.addressee = addressee;
            this.CallbackAction = callbackAction;
            this.Date = DateTime.Now;
        }
        private MEP(MEPDevType devType, long macAddr, uint ipAddr, long addressee, MEPCallbackAction callbackAction, DateTime date)
        {
            this.devType = devType;
            this.IpAddr = ipAddr;
            this.MacAddr = macAddr;
            this.addressee = addressee;
            this.CallbackAction = callbackAction;
            this.Date = date;
        }
        public MEP(MEPDevType devType, MEPCallbackAction callbackAction)
        {
            this.devType = devType;
            this.IpAddr = 0;
            this.MacAddr = 0;
            this.addressee = 0;
            this.CallbackAction = callbackAction;
            this.Date = DateTime.Now;*/
        }


        
		/**
         * Deserializing DRP message
         * @param mepMessage the string to deserialize
         * EXAMPLE:
         * {"$DRP":{"DevType":"RBPI","MACAddr":"123456789ABC","IPAddr":"C0A80101","Callback":"RES","Addressee":"123456789ABC","Date": "28/07/2017 19:02"}}
         **/
		 /*
        public static DRP deserializeDRP(string drpMessage)
        {
            drpMessage = drpMessage.Replace('?', '\"');
            JsonSerializerSettings jss = new JsonSerializerSettings();
            DRP drp = JsonConvert.DeserializeObject<DRP>(drpMessage, jss);
            return drp;
        }
		*/
		
        /**
         * convert enum type to string 
         */
        private static string stringer(DRPDevType type)
        {
            switch (type)
            {
                case DRPDevType.APP:
                    return "APP";
                case DRPDevType.RBPI:
                    return "RBPI";
                default:
                    return "";
            }
        }
        /**
         * convert enum type to string 
         */
        private static string stringer(DRPMessageType type)
        {
            switch (type)
            {
                case DRPMessageType.SCANNED:
                    return "SCN";
                
                case DRPMessageType.FINAL:
                    return "FIN";
				case DRPMessageType.DATA:
                    return "DTA";
				case DRPMessageType.EXCEPTION:
                    return "EXC";
				case DRPMessageType.ACKNOWLEDGE:
                    return "ACK";
                default:
                    return "";
            }
        }
        /**
         * convert string to enum type
         */
        private static DRPDevType parseDevType(string type)
        {
            switch (type)
            {
                case "APP":
                    return DRPDevType.APP;
                case "RBPI":
                    return DRPDevType.RBPI;
                default:
                    return DRPDevType.RBPI;
            }
        }
        /**
         * convert string to enum type
         */
        private static DRPMessageType parseCallbackAction(string type)
        {
            switch (type)
            {
                case "SCN":
                    return DRPMessageType.SCANNED;
                case "FIN":
                    return DRPMessageType.FINAL;
                case "DTA":
                    return DRPMessageType.DATA;
				case "EXC":
					return DRPMessageType.EXCEPTION;
				case "ACK":
					return DRPMessageType.ACKNOWLEDGE;
				default:
					return DRPMessageType.ACKNOWLEDGE;
            }
        }



        /**
        * Sign the current date and time onto the message
        **/
        public void timeStamp()
        {
            this.Date = DateTime.Now;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    enum DRPDevType { RBPI, APP }
    enum DRPMessageType { SCANNED, FINAL, DATA, EXCEPTION, ACKNOWLEDGE }
}
