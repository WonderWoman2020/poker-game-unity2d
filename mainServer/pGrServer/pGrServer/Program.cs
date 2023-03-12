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
            Initialize();
            Thread loginThread = new Thread(ListenLogin);
            Thread autoLogoutThread = new Thread(AutoLogout);

            loginThread.Start();
            autoLogoutThread.Start();

            Console.ReadKey();
            //zamykanie
            running = false;
            loginListener.Stop();
            loginThread.Join();
            autoLogoutThread.Join();

            //TODO
            //zamknac wszystkie polaczenia
            //iteracja po tablicy asocjacyjnej

            Environment.Exit(0);
        }
        public static void ListenLogin()
        {
            Int32 port = 6937;
            
            loginListener.Start();
            try
            {
                while (running)
                {
                    TcpClient client = loginListener.AcceptTcpClient();

                    //TODO
                    //odebrać username i hasło
                    string username;
                    string password;

                    //TODO
                    //zapytanie do bazy czy taki uzytkownik istnieje

                    if (/*istnieje*/true)
                    {
                        string token = GenerateToken();

                        //TODO
                        //Nalezy ten token wysłać użytkownikowi

                        loggedClientsAccess.WaitOne();
                        loggedClients[token] = client;
                        loggedClientsAccess.ReleaseMutex();

                    }
                    else
                    {
                        //TODO
                        //Odpowiedz, ze sie nie udalo
                        //chyba tyle

                        //Ewentualnie, rejestrowac próby logowania na uzytkownika by zablokować próby odgadnięcia
                    }
                }
            }
            catch(SocketException ex)
            {

            }
            
        }
        public static void Initialize()
        {
            running = true;
            loggedClients = new Dictionary<string, TcpClient>();
            loggedClientsAccess = new Mutex();
            loginListener = new TcpListener(IPAddress.Any, 6937);
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
            while (running)
            {
                Console.WriteLine('x');
            } 
        }
    }
}
