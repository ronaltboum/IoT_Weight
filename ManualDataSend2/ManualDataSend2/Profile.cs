using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace ManualDataSend2
{
    class Profile
    {
        private float weight;
        private float fat;
        private string username;
        private List<uint> macsNearby;

        public Profile(string username, float weight, float fat, List<uint> macsNearby)
        {
            this.username = username;
            this.weight = weight;
            this.fat = fat;
            this.macsNearby = new List<uint>(macsNearby);
        }

        public float Weight
        {
            get { return weight; }
            set { weight = value; }
        }

        public float Fat
        {
            get { return fat; }
            set { fat = value; }
        }
        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public List<uint> MacsNearby
        {
            get { return new List<uint>(macsNearby); }
        }

        public void addMAC(uint mac)
        {
            macsNearby.Add(mac);
        }
        public void removeMAC(uint mac)
        {
            macsNearby.Remove(mac);
        }

        public override string ToString()
        {
            Dictionary<string, object> mainDict = new Dictionary<string, object>();
            Dictionary<string, object> macs = new Dictionary<string, object>();
            for(int i = 0;i< macsNearby.Count; i++)
            {
                macs.Add("mac" + i, macsNearby[i]);
            }
            mainDict.Add("username", username);
            mainDict.Add("macs", macs);
            mainDict.Add("weight", weight);
            mainDict.Add("fat", fat);

            return JsonConvert.SerializeObject(mainDict);
        }
    }
}
