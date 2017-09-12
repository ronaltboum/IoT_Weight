using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPiRunner2;
namespace DRPTester
{
    class Program
    {
        static void Main(string[] args)
        {
            List<float> lst = new List<float>() { 0, 5, 44.77f };
            List<float> lst2 = new List<float>() { 0 };
            List<float> lst3 = new List<float>() { 0, 0 };
            List<float> lst4 = new List<float>() { };
            List<float> lst5 = new List<float>() { 45000f };

            DRP[] drpin = new DRP[5];
            drpin[0] = new DRP(DRPDevType.RBPI, "Bar", 0xAAABBB, 1, lst, 0, DRPMessageType.DATA);
            drpin[1] = new DRP(DRPDevType.APP, "Ramy", 0x1234567890, 2, lst2, 1, DRPMessageType.EXCEPTION);
            drpin[2] = new DRP(DRPDevType.RBPI, "Ron", 0x0, -1, lst3, 2, DRPMessageType.ACKNOWLEDGE);
            drpin[3] = new DRP(DRPDevType.RBPI, "We", 0xFFFFF, -10, lst4, 9000, DRPMessageType.FINAL);
            drpin[4] = new DRP(DRPDevType.RBPI, "You", 2, 71, lst5, 10000, DRPMessageType.SCANNED);


            bool allgood = true;
            foreach(DRP drp in drpin)
            {
                if (!test(drp))
                {
                    allgood = false;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Test Failed :-(");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
            if (allgood)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("All tests passed!");
            }
            Console.Read();
        }


        static bool test(DRP drpin)
        {
            DRP drpout;

            string str = drpin.ToString();
            Console.WriteLine(str);

            drpout = DRP.deserializeDRP(str);

            if (drpout.Equals(drpin))
            {
                return true;
            }
            else
            {
                Console.WriteLine(drpout + "\nNO MATCH!");
                return false;
            }
        }
    }
}
