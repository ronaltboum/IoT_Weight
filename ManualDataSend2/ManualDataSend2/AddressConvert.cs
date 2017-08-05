using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
namespace ManualDataSend2
{

    /**
     * Convert MAC and IP Address to/from string
     */
    class AddressConvert
    {
        public static uint parseIPFromDecimal(string ipAsStr)
        {
            string[] segs = ipAsStr.Split('.');
            uint ip = 0;
            uint mul = 1;
            for(int i = segs.Length - 1; i >= 0; i--)
            {
                ip += uint.Parse(segs[i]) * mul;
                mul *= 256u;
            }
            return ip;
        }

        public static uint parseIPFromHex(string ipAsStr)
        {
            return uint.Parse(ipAsStr,NumberStyles.HexNumber);
        }

        public static long parseMacFromHex(string macAsStr)
        {
            return uint.Parse(macAsStr, NumberStyles.HexNumber);
        }

        public static long parseMACSeperatedByColon(string macAsStr)
        {
            string[] segs = macAsStr.Split(':');
            long mac = 0;
            long mul = 1;
            for (int i = segs.Length - 1; i >= 0; i--)
            {
                mac += long.Parse(segs[i],NumberStyles.HexNumber) * mul;
                mul *= 0x100;
            }
            return mac;
        }

        public static string IPtoHexString(uint ip)
        {
            return ip.ToString("X8");
        }
        public static string MAXtoStringWithoutColon(long mac)
        {
            return mac.ToString("X12");
        }

        public static string IPtoRepresentalString(uint ip)
        {
            string res = "";
            for(int i = 0; i < 4; i++)
            {
                res = (ip % 0x100).ToString() + "." + res; 
            }
            return res;
        }
        public static string MACtoRepresentalString(long mac)
        {
            string macAsStr = mac.ToString("X12");
            string res = "";
            for(int i = 0; i < 6; i++)
            {
                res += macAsStr[2*i] + macAsStr[2*i + 1] + ":";
            }
            return res;
        }
    }
}
