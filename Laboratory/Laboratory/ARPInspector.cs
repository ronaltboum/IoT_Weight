using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using System.Net.NetworkInformation;
namespace Laboratory
{
    class ARPInspector
    {
        Dictionary<string, Dictionary<string, IPType>> iptable;
        public ARPInspector(string[] flags)
        {
           System.Net.Cookie
        }
        public ARPInspector() : this(new string[0]) { }


    }
    enum IPType
    {
        Static, Dynamic
    }
}
