using System;

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using PokerGameClasses;

namespace pGrServer
{   
    class Program
    {
        public static bool running;
        public static IDictionary<string, TcpClient> loggedClients;
        public static List<string> loggedTokens;
        public static Mutex loggedClientsAccess;
        public static TcpListener loginListener;
        static void Main()
        {
            ConsoleKeyInfo cki;
            Console.CursorVisible = false;
            var sb = new StringBuilder();
            var emptySpace = new StringBuilder();
            emptySpace.Append(' ', 10);
            DateTime startTime = DateTime.Now;
            

            //PokerLogicTests pokerTester = new PokerLogicTests();
            //pokerTester.RunExampleGame();

            //INITIALIZE
            Initialize();
            Thread loginThread = new Thread(ListenLogin);
            Thread autoLogoutThread = new Thread(AutoLogout);
            Thread requestsThread = new Thread(ListenRequests);

            //RUN
            loginThread.Start();
            autoLogoutThread.Start();
            requestsThread.Start();

            Console.Clear();
            while (running)
            {
                Console.SetCursorPosition(0, 0);
                sb.Clear();
                sb.AppendLine("--- Server status ---");
                sb.Append("Login active: " + (loginListener.Server.IsBound ? "YES" : "NO") + emptySpace + '\n');
                sb.Append("Logged clients: " + loggedClients.Count + emptySpace + '\n');
                DateTime now = DateTime.Now;
                sb.Append("Working time: " + now.Subtract(startTime).ToString() + '\n');
                Console.WriteLine(sb);
                if (Console.KeyAvailable)
                {
                    cki = Console.ReadKey();
                    if(cki.Key == ConsoleKey.Escape)
                    {
                        running = false;
                    }
                }
                Thread.Sleep(500);
                
            }

            //EXIT
            loginListener.Stop();
            loginThread.Join();
            autoLogoutThread.Join();
            requestsThread.Join();

            loggedClientsAccess.WaitOne();
            foreach (string token in loggedTokens)
            {
                loggedClients[token].Close();
            }
            loggedClients.Clear();
            loggedClientsAccess.ReleaseMutex();
            Environment.Exit(0);
        }
        public static void ListenRequests()
        {
            while (running)
            {

            }
            Console.WriteLine("Requests closed");
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

                    byte[] readBuffer = new byte[1024];
                    int numberOfBytesRead = 0;
                    StringBuilder lognpass = new StringBuilder();
                    numberOfBytesRead = clientStream.Read(readBuffer, 0, readBuffer.Length);
                    lognpass.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, numberOfBytesRead));

                    string[] loginAndPassword = lognpass.ToString().Split(new char[] { ' ' }, 2);

                    string username = loginAndPassword[0];
                    string password = loginAndPassword[1];

                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/loginuser");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = "{\"login\":\"" + username + "\",\"password\":\"" + password + "\"}";
                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        var dataFromDatabase = result.Split("\"");
                        var responseCode = Regex.Match(dataFromDatabase[2], @"\d+").Value;
                        if (responseCode == "412") //bad login
                        {
                            byte[] message = System.Text.Encoding.ASCII.GetBytes("##&&@@0001");
                            clientStream.Write(message, 0, message.Length);
                        }
                        else if (responseCode == "416") //bad password
                        {
                            byte[] message = System.Text.Encoding.ASCII.GetBytes("##&&@@0002");
                            clientStream.Write(message, 0, message.Length);
                        }
                        else if (responseCode == "210") // ok
                        {

                            string token = GenerateToken();

                            string toBeSearched = "xp\":";
                            var xpH = result.Substring(result.IndexOf(toBeSearched) + toBeSearched.Length);
                            var xp = Regex.Match(xpH, @"\d+").Value;

                            toBeSearched = "coins\":";
                            var coinsH = result.Substring(result.IndexOf(toBeSearched) + toBeSearched.Length);
                            var coins = Regex.Match(coinsH, @"\d+").Value;

                            toBeSearched = "login\":\"";
                            var loginH = result.Substring(result.IndexOf(toBeSearched) + toBeSearched.Length);
                            var data = loginH.Split('\"');
                            var login = data[0];
                            //message = System.Text.Encoding.ASCII.GetBytes(login + ' ');
                            //clientStream.Write(message, 0, message.Length);

                            toBeSearched = "nick\":\"";
                            var nickH = result.Substring(result.IndexOf(toBeSearched) + toBeSearched.Length);
                            data = nickH.Split('\"');
                            var nick = data[0];

                            byte[] message = System.Text.Encoding.ASCII.GetBytes(token + ' ' + xp + ' ' + coins + ' ' + nick);
                            clientStream.Write(message, 0, message.Length);

                            loggedClientsAccess.WaitOne();
                                loggedClients[token] = client;
                                loggedTokens.Add(token);
                            loggedClientsAccess.ReleaseMutex();
                        }
                        else //failed
                        {
                            byte[] message = System.Text.Encoding.ASCII.GetBytes("##&&@@0000");
                            clientStream.Write(message, 0, message.Length);
                        }
                    }
                    //clientStream.Dispose();
                }
            }
            catch(SocketException ex)
            {
                Console.WriteLine("login listening closed\n"); //Wyskoczy zawsze podczas zamykania
            }
            
        }
        public static void Initialize()
        {
            running = true;
            loggedClients = new Dictionary<string, TcpClient>();
            loggedClientsAccess = new Mutex();
            loginListener = new TcpListener(IPAddress.Any, (Int32)6937);
            loggedTokens = new List<string>();
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
                //Console.WriteLine('x');
            }
            Console.WriteLine("Auto Logout closed");
        }
    }
}
