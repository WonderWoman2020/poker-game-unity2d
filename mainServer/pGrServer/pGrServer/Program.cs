using System;

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using PokerGameClasses;

namespace pGrServer
{   
    class Program
    {
        public static bool running;
        public static IDictionary<string, TcpClient> loggedClients;
        public static Mutex loggedClientsAccess;
        public static TcpListener loginListener;
        static void Main()
        {

            PokerLogicTests pokerTester = new PokerLogicTests();
            pokerTester.RunExampleGame();

            Initialize();
            Thread loginThread = new Thread(ListenLogin);
            Thread autoLogoutThread = new Thread(AutoLogout);

            loginThread.Start();
            autoLogoutThread.Start();

            Console.ReadKey();
            running = false;
            loginListener.Stop();
            loginThread.Join();
            autoLogoutThread.Join();

            //TODO
            //zamknac wszystkie polaczenia
            //iteracja po tablicy asocjacyjnej
            //nie wiem jeszcze jak XD

            Environment.Exit(0);
        }
        public static void ListenLogin()
        {
            
            loginListener.Start();
            try
            {
                while (running)
                {
                    TcpClient client = loginListener.AcceptTcpClient();
                    NetworkStream clientStream = client.GetStream();
                    //TODO
                    //odebrać username i hasło
                    string username;
                    string password;

                    //TODO
                    //zapytanie do bazy czy taki uzytkownik istnieje

                    if (/*istnieje*/true)
                    {
                        string token = GenerateToken();

                        byte[] message = System.Text.Encoding.ASCII.GetBytes(token);
                        clientStream.Write(message, 0, message.Length);

                        loggedClientsAccess.WaitOne();
                            loggedClients[token] = client;
                        loggedClientsAccess.ReleaseMutex();

                    }
                    else
                    {
                        byte[] message = System.Text.Encoding.ASCII.GetBytes("0000000000");
                        clientStream.Write(message, 0, message.Length);

                        //TODO
                        //rejestrowac próby logowania na uzytkownika by zablokować próby odgadnięcia
                    }
                    clientStream.Dispose();
                }
            }
            catch(SocketException ex)
            {
                //To musi być złapane, bo inaczej wywala błąd unhandled exception
                //Po co taka logika, nie kumam, ale tu nic dziac sie nie musi XD
                Console.WriteLine(ex);

            }
            
        }
        public static void Initialize()
        {
            running = true;
            loggedClients = new Dictionary<string, TcpClient>();
            loggedClientsAccess = new Mutex();
            loginListener = new TcpListener(IPAddress.Any, (Int32)6937);
        }
        public static string GenerateToken()
        {
            int length = 10;
            const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder res = new StringBuilder();
            using(RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(CHARS[(int)(num % (uint)CHARS.Length)]);
                }
                return res.ToString();
            }
        }
        public static void AutoLogout()
        {
            //TODO
            //Co jakis czas np. minute przesledzic jakis dziennik aktywnosci
            //np wylogować osoby co nie miały jakichś akcji od 30min
            while (running)
            {
                Console.WriteLine('x');
            } 
        }
    }
}
