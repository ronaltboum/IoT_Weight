﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
namespace ConsoleLab
{
    class MEP
    {
        private MEPDevType devType;
        private uint ipAddr;
        private long macAddr;
        private MEPCallbackAction callbackAction;
        private DateTime date;

        /* Setters & Getters */
        public MEPDevType DevType { get => devType; set => devType = value; }
        public uint IpAddr { get => ipAddr; set => ipAddr = value; }
        public long MacAddr { get => macAddr; set => macAddr = value; }
        public MEPCallbackAction CallbackAction { get => callbackAction; set => callbackAction = value; }
        public DateTime Date { get => date; set => date = value; }

        /**
         * Deserializing MEP message
         * @param mepMessage the string to deserialize
         **/
        public static MEP deserializeMEP(string mepMessage)
        {
            Dictionary<string,Dictionary<string,string>> mepMes = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(mepMessage);
            Dictionary<string, string> data = mepMes["$MEP"];
            MEP mep = new MEP(parseDevType(data["DevType"]), long.Parse(data["MACAddr"],NumberStyles.HexNumber), uint.Parse(data["IPAddr"],NumberStyles.HexNumber), parseCallbackAction(data["Callback"]), DateTime.Parse(data["Date"]));
            return mep;
        }

        /**
         * Sign the current date and time onto the message
         **/
        public void timeStamp()
        {
            this.Date = DateTime.Now;
        }

        /** constructors **/
        public MEP(MEPDevType devType, long macAddr, uint ipAddr, MEPCallbackAction callbackAction)
        {
            this.DevType = devType;
            this.IpAddr = ipAddr;
            this.MacAddr = macAddr;
            this.CallbackAction = callbackAction;
            this.Date = DateTime.Now;
        }
        private MEP(MEPDevType devType, long macAddr, uint ipAddr, MEPCallbackAction callbackAction, DateTime date)
        {
            this.DevType = devType;
            this.IpAddr = ipAddr;
            this.MacAddr = macAddr;
            this.CallbackAction = callbackAction;
            this.Date = date;
        }
        public MEP(MEPDevType devType, MEPCallbackAction callbackAction)
        {
            this.DevType = devType;
            this.IpAddr = 0;
            this.MacAddr = 0;
            this.CallbackAction = callbackAction;
            this.Date = DateTime.Now;
        }

        public override string ToString()
        {
            Dictionary<string, Dictionary<string, string>> wrapper = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> data = new Dictionary<string, string>();

            wrapper.Add("$MEP", data);
            data.Add("DevType", stringer(DevType));
            data.Add("MACAddr", MacAddr.ToString("X12"));
            data.Add("IPAddr", IpAddr.ToString("X8"));
            data.Add("Callback", stringer(CallbackAction));
            data.Add("Date", Date.ToString());

            return JsonConvert.SerializeObject(wrapper);
        }

        /**
        * convert enum type to string 
        */
        private static string stringer(MEPDevType type)
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
        /**
         * convert enum type to string 
         */
        private static string stringer(MEPCallbackAction type)
        {
            switch (type)
            {
                case MEPCallbackAction.Response:
                    return "RES";
                case MEPCallbackAction.Acknowledge:
                    return "ACK";
                case MEPCallbackAction.NoOperation:
                    return "NOP";
                default:
                    return "";
            }
        }
        /**
         * convert string to enum type
         */
        private static MEPDevType parseDevType(string type)
        {
            switch (type)
            {
                case "APP":
                    return MEPDevType.APP;
                case "RBPI":
                    return MEPDevType.RBPI;
                default:
                    return MEPDevType.RBPI;
            }
        }
        /**
         * convert string to enum type
         */
        private static MEPCallbackAction parseCallbackAction(string type)
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
    }
    enum MEPDevType { RBPI, APP }
    enum MEPCallbackAction { Response, Acknowledge, NoOperation }
}
