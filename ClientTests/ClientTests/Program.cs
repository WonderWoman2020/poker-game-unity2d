using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //SZYBKI PORADNIK
            /*
             * W tym ulozeniu nalezy sie poprawnie zalogowac
             * tzn można testowac złe zalogowanie ale wtedy po kliknieciu klawisza by przejsc dalej juz nie bedzie dzialac
             * po zalogowaniu mozna kliknac dowolny przycisk by przejsc do etapu testowania funkcji
             * co sekunde wysyła chec pobrania aktualnych stołów
             * klikniecie spacji wysyła chęc zrobienia stołu o id nick+iloscTokenow+nr
             * przy zalogowaniu dwa razy na to samo konto logicznym jest, ze jak ktorys klient zrobił nick+tokeny+0
             * to kolejny bedzie chcial zrobic taki sam, dlatego nic sie nie stanie póki nr nie bedzie wiekszy
             * 
             * 
             * 
             * 
             * 
             */




            TcpClient server = new TcpClient();
            server.Connect("127.0.0.1", 6937);
            NetworkStream ns = server.GetStream();

            
            


            Console.WriteLine("Login: ");
            string username = Console.ReadLine();
            Console.WriteLine("Password: ");
            string password = Console.ReadLine();

            byte[] message = System.Text.Encoding.ASCII.GetBytes(username + ' ' + password);
            ns.Write(message, 0, message.Length);


            byte[] myReadBuffer = new byte[1024];
            int numberOfBytesRead = 0;
            StringBuilder myCompleteMessage = new StringBuilder();
            numberOfBytesRead = ns.Read(myReadBuffer, 0, myReadBuffer.Length);
            myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
            Console.WriteLine(myCompleteMessage);
            Console.WriteLine(myCompleteMessage.Length);
            Console.ReadKey();
            //JAk sie zle zaloguje to od tego miejsca bedzie juz zle ##########################################################
            //####################################################################################################################

            TcpClient serverGame = new TcpClient();
            serverGame.Connect("127.0.0.1", 6938);

            

            //byte[] readBuffer = new byte[256];
            //StringBuilder menuRequestStrings = new StringBuilder();
            //int bytesRead = ns.Read(readBuffer, 0, readBuffer.Length);
            //menuRequestStrings.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, bytesRead));

            string[] request = myCompleteMessage.ToString().Split(new char[] { ' ' });
            ns.Flush();
            string token = request[0];
            string login = request[3];
            string tokens = request[2];

            ConsoleKeyInfo cki;
            Console.CursorVisible = false;
            var sb = new StringBuilder();
            var emptySpace = new StringBuilder();
            emptySpace.Append(' ', 10);
            bool running = true;

            Console.Clear();
            int nr = int.Parse(tokens);
            while (running)
            {
                Console.SetCursorPosition(0, 0);
                sb.Clear();

                byte[] tosendtables = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "2");
                ns.Write(tosendtables, 0, tosendtables.Length);
                Thread.Sleep(1000);
                if (ns.DataAvailable)
                {
                    byte[] readBuf = new byte[4096];
                    StringBuilder menuRequestStr = new StringBuilder();
                    int nrbyt = ns.Read(readBuf, 0, readBuf.Length);
                    menuRequestStr.AppendFormat("{0}", Encoding.ASCII.GetString(readBuf, 0, nrbyt));
                    string[] tables = menuRequestStr.ToString().Split(new string(":T:")); //na poczatku tez dzieli i wykrywa 1 pusty string 

                    foreach (string table in tables)
                    {
                        sb.AppendLine(table);
                    }
                }
                

                Console.WriteLine(sb);
                if (Console.KeyAvailable)
                {
                    cki = Console.ReadKey();
                    if (cki.Key == ConsoleKey.Escape)
                    {
                        running = false;
                    }
                    if (cki.Key == ConsoleKey.Spacebar)
                    {
                        byte[] tosend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "0" + ' ' + login +nr.ToString() + ' ' + "1" + ' ' + "3" + ' ' + "10" + ' ' + "16" + ' ');
                        ns.Write(tosend, 0, tosend.Length);
                        nr++;

                    }
                }
                Console.WriteLine(emptySpace);
                

            }
        }
    }
}
