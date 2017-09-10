using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;

namespace drpTests
{
    /**
     * Represents a DRP message according to the DRP protocol
     **/
   
    class DRP
    {
        private string protocol;
        private DRPDevType devType;
        private string userName;
        private long raspID;
        private DRPData data;
        private int token;
        private DRPMessageType messageType;
        private DateTime date;

        /* Setters & Getters */
        public string Protocol { get => protocol; set => protocol = value; }
        public DRPDevType DevType { get => devType; set => devType = value; }
        public string UserName { get => userName; set => userName = value; }
        public long RaspID { get => raspID; set => raspID = value; }
        public int Token { get => token; set => token = value; }
        public DRPMessageType MessageType { get => messageType; set => messageType = value; }
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

        public DRP(DRPDevType devType, string userName, long raspID, DRPMessageType messageType) : this(devType, userName, raspID, null, messageType)
        {

        }

        public DRP(DRPDevType devType, string userName, long raspID, DRPData data, DRPMessageType messageType)
        {
            protocol = "$DRP";
            this.devType = devType;
            this.userName = userName;
            this.raspID = raspID;
            this.data = data;
            token = 0;
            this.messageType = messageType;
            this.date = DateTime.Now;
        }


        public DRP(Newtonsoft.Json.Linq.JObject jMessage)
        {
            protocol = (string)jMessage["Protocol"];
            devType = parseDevType ((string)jMessage["DevType"]);
            userName = (string)jMessage["UserName"];
            raspID = (long)jMessage["RaspID"];
            data = DRPData.parseDRPData ((string)jMessage["Data"]);
            token = (int)jMessage["Token"];
            messageType = parseMessageType ((string)jMessage["MessageType"]);
            date = DateTime.Parse((string)jMessage["Date"]);
        }









        /**
         * Deserializing DRP message
         * @param drpMessage the string to deserialize
         * EXAMPLE:
         * {"$DRP":{"DevType":"RBPI","MACAddr":"123456789ABC","IPAddr":"C0A80101","Callback":"RES","Addressee":"123456789ABC","Date": "28/07/2017 19:02"}}
         **/

        public static DRP deserializeDRP(string drpMessage)
        {

            Newtonsoft.Json.Linq.JObject jMessage = Newtonsoft.Json.Linq.JObject.Parse(drpMessage);
            return new DRP(jMessage);
        }

        public static string serializeDRP(DRP drpMessage)
        {
            return drpMessage.ToString();
        }


        



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
        private static DRPMessageType parseMessageType(string type)
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

        public enum DRPDevType { RBPI, APP };
        public enum DRPMessageType { SCANNED, FINAL, DATA, EXCEPTION, ACKNOWLEDGE };

        public static void Main()
        {
            DRP d = new DRP(DRPDevType.APP, "PinkPanther", 12345, new DRPData(), DRPMessageType.FINAL);
            string s = serializeDRP(d);
            Console.WriteLine(s);
            DRP des = deserializeDRP(s);
            Console.WriteLine("ARE THEY EQUAL? {0}", d.Equals(des));

        }


    }

    
}


