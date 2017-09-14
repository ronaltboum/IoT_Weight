using System;z
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
namespace RPiRunner2
{
    class UserHardwareLinker : LinearHX
    {
        private TempProfile currUser;


        public UserHardwareLinker(GpioPin sck, GpioPin dout): base(dout,sck)
        {
            currUser = null;
        }

        public TempProfile currentServedUser()
        {
            return currUser;
        }

        public void StartUser(TempProfile user)
        {
            currUser = user;
        }

        public void FinishUser()
        {
            currUser = null;
        }

        public bool CheckOrigin(DRP drp)
        {
            return drp.SourceID == currentServedUser().Appid && drp.Token == currentServedUser().Token && drp.UserName.Equals(currentServedUser().Username);
        }
    }
    class TempProfile
    {
        private string username;
        private ulong token;
        private long appid;

        public TempProfile(string username, ulong token, long appid)
        {
            this.username = username;
            this.token = token;
            this.appid = appid;
        }

        public string Username { get => username; set => username = value; }
        public ulong Token { get => token; set => token = value; }
        public long Appid { get => appid; set => appid = value; }
    }
}
