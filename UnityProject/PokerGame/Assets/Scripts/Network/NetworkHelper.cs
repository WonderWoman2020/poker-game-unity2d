using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace pGrServer
{
    public class NetworkHelper
    {
        public static string ReadNetworkStream(NetworkStream stream)
        {
            byte[] readBuffer = new byte[1024];
            StringBuilder sb = new StringBuilder();
            int bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
            sb.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
            return sb.ToString();
        }

        public static void WriteNetworkStream(NetworkStream stream, string data)
        {
            byte[] message = Encoding.ASCII.GetBytes(data);
            stream.Write(message, 0, message.Length);
        }
    }
}
