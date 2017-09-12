using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;

namespace RPiRunner2
{
    /**
     * Represents a DRP message according to the DRP protocol
     **/
    [JsonObject(MemberSerialization.OptIn)]
    class DRP
    {
        public const string PROTOCOL = "$DRP";
        private DRPDevType devType;
        private string userName;
        private long raspID;
        private long appID;
        private IList<float> data;
        private ulong token;
        private DRPMessageType messageType;
        private DateTime date;

        /* Setters & Getters */
        public DRPDevType DevType { get => devType; set => devType = value; }
        public string UserName { get => userName; set => userName = value; }
        public long RaspID { get => raspID; set => raspID = value; }
        public ulong Token { get => token; set => token = value; }
        public DRPMessageType MessageType { get => messageType; set => messageType = value; }
        public DateTime Date { get => date; set => date = value; }
        public long AppID { get => appID; set => appID = value; }
        public IList<float> Data { get => data; }

        /* Setters & Getters for JSON */
        [JsonProperty(PropertyName = "Protocol")]
        private string JProtocol { get => PROTOCOL; }
        [JsonProperty(PropertyName = "DevType")]
        private string JDevType { get =>stringer( devType); set => devType =parseDevType( value); }
        [JsonProperty(PropertyName = "RaspID")]
        private string JRaspID { get => raspID.ToString("X"); set => raspID = long.Parse( value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "AppID")]
        private string JAppID { get => appID.ToString("X"); set => appID = long.Parse(value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "Username")]
        private string JUserName { get => userName; set => userName = value; }
        [JsonProperty(PropertyName = "data")]
        private string JData { get => JsonConvert.SerializeObject(data); set => data = JsonConvert.DeserializeObject<IList<float>>(value); }
        [JsonProperty(PropertyName = "Token")]
        private string JToken { get => token.ToString("X"); set => token = ulong.Parse(value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "MsgType")]
        private string JMessageType { get => stringer( messageType); set => messageType = parseCallbackAction( value); }
        [JsonProperty(PropertyName = "Date")]
        private string JDate { get => date.ToString(); set => date = DateTime.Parse(value); }
        


        /** constructors **/

        //Empty constructor is needed for deserialize JSON
        private DRP() { }

        public DRP(DRPDevType devType, string userName, long raspID, long appID, IList<float> data, ulong token, DRPMessageType messageType, DateTime date)
        {
            this.devType = devType;
            this.userName = userName;
            this.raspID = raspID;
            this.appID = appID;
            this.data = data;
            this.token = token;
            this.messageType = messageType;
            this.date = date;
        }
        public DRP(DRPDevType devType, string userName, long raspID, long appID, IList<float> data, ulong token, DRPMessageType messageType)
        {
            this.devType = devType;
            this.userName = userName;
            this.raspID = raspID;
            this.appID = appID;
            this.data = data;
            this.token = token;
            this.messageType = messageType;
            this.date = DateTime.Now;
        }

        /**
         * Deserializing DRP message
         * @param mepMessage the string to deserialize
         * EXAMPLE:
         * {"$DRP":{"DevType":"RBPI","MACAddr":"123456789ABC","IPAddr":"C0A80101","Callback":"RES","Addressee":"123456789ABC","Date": "28/07/2017 19:02"}}
         **/
       
       public static DRP deserializeDRP(string drpMessage)
       {
           drpMessage = drpMessage.Replace('?', '\"');
           DRP drp = JsonConvert.DeserializeObject<DRP>(drpMessage);
           return drp;
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
        public override bool Equals(object obj)
        {
            if (!(obj is DRP))
                return false;
            DRP compto = obj as DRP;
            if (this.devType != compto.devType)
                return false;
            if (this.appID != compto.appID)
                return false;
            if (this.raspID != compto.raspID)
                return false;
            if (!JsonConvert.SerializeObject(data).Equals(JsonConvert.SerializeObject(compto.data)))
                return false;
            if (this.messageType != compto.messageType)
                return false;
            return true;
        }
    }
    enum DRPDevType { RBPI, APP }
    enum DRPMessageType { SCANNED, FINAL, DATA, EXCEPTION, ACKNOWLEDGE }
}
