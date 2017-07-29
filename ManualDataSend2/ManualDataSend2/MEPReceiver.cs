using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ManualDataSend2
{
    class MEP
    {
        private MEPDevType devType;
        private uint ipAddr;
        private long macAddr;
        private MEPCallbackAction callbackAction;
        private DateTime date;

        public MEPDevType DevType { get; set; }
        public MEPDevType IpAddr { get; set; }
        public MEPDevType MacAddr { get; set; }
        public MEPDevType CallbackAction { get; set; }
        public MEPDevType Date { get; set; }

        public static MEP parseMEP(string mepMessage)
        {
            MEP mepMes = JsonConvert.DeserializeObject(mepMessage) as MEP;
            return mepMes;
        }

        public MEP(MEPDevType devType, uint ipAddr, long macAddr, MEPCallbackAction callbackAction)
        {
            this.devType = devType;
            this.ipAddr = ipAddr;
            this.macAddr = macAddr;
            this.callbackAction = callbackAction;
            this.date = new DateTime();
        }
        public MEP(MEPDevType devType, MEPCallbackAction callbackAction)
        {
            this.devType = devType;
            this.ipAddr = 0;
            this.macAddr = 0;
            this.callbackAction = callbackAction;
            this.date = new DateTime();
        }

        private string stringer(MEPDevType type)
        {
            switch (type)
            {
                case MEPDevType.APP:
                    return "APP";
                case MEPDevType.RBPI:
                    return "RBPI";
                default:
                    return "";
            }
        }
        private MEPDevType parseDevType(string type)
        {
            switch (type)
            {
                case "APP":
                    return MEPDevType.APP ;
                case "RBPI":
                    return MEPDevType.RBPI;
                default:
                    return MEPDevType.RBPI;
            }
        }
        private MEPCallbackAction parseCallbackAction(string type)
        {
            switch (type)
            {
                case "RES":
                    return MEPCallbackAction.Response;
                case "ACK":
                    return MEPCallbackAction.Acknowledge;
                case "NOP":
                    return MEPCallbackAction.NoOperation;
                default:
                    return MEPCallbackAction.NoOperation;
            }
        }

        public override string ToString()
        {
            Dictionary<string, Dictionary<string, string>> wrapper = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> data = new Dictionary<string, string>();

            wrapper.Add("$MEP", data);
            data.Add("DevType", stringer(devType));
            data.Add("MACAddr", macAddr.ToString("X12"));
            data.Add("IPAddr", ipAddr.ToString("X8"));
            data.Add("Callback", stringer(callbackAction));
            data.Add("Date", date.ToString());

            return JsonConvert.SerializeObject(wrapper);
        }
    }
    enum MEPDevType { RBPI, APP }
    enum MEPCallbackAction { Response, Acknowledge, NoOperation }
}
