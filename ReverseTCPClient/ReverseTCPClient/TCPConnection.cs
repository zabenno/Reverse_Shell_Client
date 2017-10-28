using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ReverseTCPClient
{
    class TCPConnection
    {
        private Socket cncServer = new Socket(SocketType.Stream, ProtocolType.Tcp);
        
        public TCPConnection()
        {
            
        }
    }
}
