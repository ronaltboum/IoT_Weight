using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiSocket
{
    public class DRPListener : TCPListener
    {
        public DRPListener(int port): base(port){ }
        public DRPListener() : base() { }

        /*
         * Handling incoming data from the client.
         * @param data the data that has received.
         */
        public override void OnDataReceived(string data)
        {
            /* 
             * complete this method.
             * NOTE: to send data back to client use:  this.Send(string message); 
             */
        }

        /*
         * Activates whenever there was an unexpected error in the connection.
         * @param exception A description of the error.
         */
        public override void OnError(string exception)
        {
            /* 
             * complete this method.
             * NOTE: to send data back to client use:  this.Send(string message); 
             */
        }
    }
}
