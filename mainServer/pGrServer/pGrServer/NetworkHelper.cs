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
        // TODO odczytywanie - dodać odczytywanie w pętli, gdyby wiadomość okazała się być dłuższa niż 1024 znaki
        public static string ReadNetworkStream(NetworkStream stream)
        {
            try
            {
                byte[] readBuffer = new byte[1024];
                StringBuilder sb = new StringBuilder();
                int bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                sb.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, bytesRead));

                Console.WriteLine("Will return string");

                return sb.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Reached ReadNetworkStream");
                return null;
            }
        }

        public static void WriteNetworkStream(NetworkStream stream, string data)
        {
            try
            {
                byte[] message = Encoding.ASCII.GetBytes(data);
                stream.Write(message, 0, message.Length);
                stream.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Reached WriteNetworkStream");
            }
        }
    }
}
