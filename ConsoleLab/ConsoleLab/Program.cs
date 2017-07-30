using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleLab
{
    class Program
    {
        static void Main(string[] args)
        {
            uint ip = 0x11111111;
            long mac = 0x123456789ABC;

            MEP mep = new MEP(MEPDevType.RBPI, mac, ip, MEPCallbackAction.Response);
            Console.WriteLine(mep.DevType);
            Console.WriteLine(mep.MacAddr);
            Console.WriteLine(mep.IpAddr);
            Console.WriteLine(mep.CallbackAction);
            Console.WriteLine(mep.Date);
            mep.timeStamp();
            Console.WriteLine(mep.Date);
            Console.WriteLine();
            string mepstr = mep.ToString();
            Console.WriteLine(mepstr);

            MEP mep2 = MEP.deserializeMEP(mepstr);
            Console.WriteLine(mep2.MacAddr);
            Console.WriteLine(mep2.IpAddr);

            Console.ReadLine();
        }
    }
}
