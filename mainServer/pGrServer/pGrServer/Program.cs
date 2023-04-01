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

        public static IDictionary<string, Client> loggedClients;
        public static List<string> loggedTokens;
        public static List<GameTable> openTables;

        public static Mutex openTablesAccess;
        public static Mutex loggedClientsAccess;

        public static TcpListener loginListener;
        public static TcpListener gameListener;

        
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
                sb.Append("Open tables: " + openTables.Count + emptySpace + '\n');
                DateTime now = DateTime.Now;
                sb.Append("Working time: " + now.Subtract(startTime).ToString() + '\n');
                Console.WriteLine(sb);
                if (Console.KeyAvailable)
                {
                    cki = Console.ReadKey();
                    if (cki.Key == ConsoleKey.Escape)
                    {
                        running = false;
                    }
                }
                Console.WriteLine(emptySpace);
                Thread.Sleep(500);

            }

            //EXIT
            loginListener.Stop();
            gameListener.Stop();
            loginThread.Join();
            autoLogoutThread.Join();
            requestsThread.Join();

            loggedClientsAccess.WaitOne();
            foreach (string token in loggedTokens)
            {
                loggedClients[token].MenuRequestsTcp.Close();
                loggedClients[token].MenuRequestsStream.Dispose();
                loggedClients[token].GameRequestsTcp.Close();
                loggedClients[token].GameRequestsStream.Dispose();
                loggedClients.Remove(token);
            }
            loggedClients.Clear();
            loggedClientsAccess.ReleaseMutex();

            Environment.Exit(0);
        }
        public static void ListenRequests()
        {
            while (running)
            {
                loggedClientsAccess.WaitOne();
                for(int i=loggedTokens.Count-1; i >= 0; i--)
                {
                    string token = loggedTokens[i];
                    if (loggedClients[token].MenuRequestsStream.DataAvailable)
                    {

                        byte[] readBuffer = new byte[256];
                        StringBuilder menuRequestStrings = new StringBuilder();
                        int bytesRead = loggedClients[token].MenuRequestsStream.Read(readBuffer, 0, readBuffer.Length);
                        menuRequestStrings.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
                        //Console.WriteLine(menuRequestStrings.ToString());
                        string[] request = menuRequestStrings.ToString().Split(new char[] { ' ' });
                        loggedClients[token].MenuRequestsStream.Flush();
                        //'token' '1 literowy kod' 'argumenty'
                        if(token == request[0])
                        {
                            if (request[1] == "0") //Utworzenie stołu
                            {
                                //nazwa, tryb, bots, min xp, min stawka
                                string name      = request[2];
                                string mode      = request[3];
                                string nrOfBots  = request[4];
                                string minXp     = request[5];
                                string big_blind = request[6];

                                bool found = false;
                                openTablesAccess.WaitOne();
                                if(loggedClients[token].GameTable == null)
                                {
                                    foreach(GameTable table in openTables)
                                        if (table.Name == name)
                                            found = true;

                                    if (!found)
                                    {
                                        loggedClients[token].CreateNewPlayer();
                                        GameTable gameTable = new GameTable(name, (HumanPlayer)loggedClients[token].Player);
                                        loggedClients[token].GameTable = gameTable;
                                        GameTableSettings gameTableSettings = new GameTableSettings();
                                        if (mode == "0")
                                            gameTableSettings.changeMode(GameMode.Mixed);
                                        else if (mode == "1")
                                            gameTableSettings.changeMode(GameMode.No_Bots);
                                        else if (mode == "2")
                                            gameTableSettings.changeMode(GameMode.You_And_Bots);

                                        gameTableSettings.changeBotsNumber(int.Parse(nrOfBots));
                                        gameTableSettings.changeMinXP(int.Parse(minXp));
                                        gameTableSettings.changeMinTokens(int.Parse(big_blind));

                                        gameTable.ChangeSettings(loggedClients[token].Player, gameTableSettings);

                                        openTables.Add(gameTable);
                                    }
                                }
                                openTablesAccess.ReleaseMutex();

                            }
                            else if (request[1] == "1") //Dolaczenie do stolu
                            {
                                if (loggedClients[token].GameTable == null)
                                {

                                }
                            }
                            else if (request[1] == "2")//informacje o stołach
                            {
                                StringBuilder completeMessage = new StringBuilder();
                                openTablesAccess.WaitOne();
                                foreach (GameTable table in openTables)
                                {
                                    completeMessage.Append(table.toMessage());
                                }
                                byte[] message = System.Text.Encoding.ASCII.GetBytes(completeMessage.ToString());
                                loggedClients[token].MenuRequestsStream.Write(message, 0, message.Length);
                                openTablesAccess.ReleaseMutex();
                            }
                            else if (request[1] == "4") //wylogowanie
                            {
                                //############################################################################
                                //TODO
                                //Wszelkie problemy ze stołami/grami itd 
                                if(loggedClients[token].GameTable != null)
                                {

                                }

                                loggedClients[token].MenuRequestsTcp.Close();
                                loggedClients[token].MenuRequestsStream.Dispose();
                                loggedClients[token].GameRequestsTcp.Close();
                                loggedClients[token].GameRequestsStream.Dispose();
                                loggedClients.Remove(token);
                                loggedTokens.RemoveAt(i);
                            }
                        }
                    }
                }
                loggedClientsAccess.ReleaseMutex();
                Thread.Sleep(250);
            }
            Console.WriteLine("Menu requests listening stopped");
        }
        public static void ListenLogin()
        {
            
            loginListener.Start();
            gameListener.Start();
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

                    clientStream.Flush();
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
                            var xpI = int.Parse(xp);

                            toBeSearched = "coins\":";
                            var coinsH = result.Substring(result.IndexOf(toBeSearched) + toBeSearched.Length);
                            var coins = Regex.Match(coinsH, @"\d+").Value;
                            var coinsI = int.Parse(coins);

                            toBeSearched = "login\":\"";
                            var loginH = result.Substring(result.IndexOf(toBeSearched) + toBeSearched.Length);
                            var data = loginH.Split('\"');
                            var login = data[0];

                            toBeSearched = "nick\":\"";
                            var nickH = result.Substring(result.IndexOf(toBeSearched) + toBeSearched.Length);
                            data = nickH.Split('\"');
                            var nick = data[0];

                            byte[] message = System.Text.Encoding.ASCII.GetBytes(token + ' ' + xp + ' ' + coins + ' ' + nick);
                            clientStream.Write(message, 0, message.Length);

                            
                            TcpClient gameClient = gameListener.AcceptTcpClient();

                            loggedClientsAccess.WaitOne();
                                loggedClients[token] = new Client(nick, xpI, coinsI, login);
                                loggedClients[token].MenuRequestsTcp = client;
                                loggedClients[token].MenuRequestsStream = client.GetStream();
                                loggedClients[token].GameRequestsTcp = gameClient;
                                loggedClients[token].GameRequestsStream = gameClient.GetStream();
                                loggedTokens.Add(token);
                            loggedClientsAccess.ReleaseMutex();
                        }
                        else //failed
                        {
                            byte[] message = System.Text.Encoding.ASCII.GetBytes("##&&@@0000");
                            clientStream.Write(message, 0, message.Length);
                        }
                    }
                }
            }
            catch(SocketException ex)
            {
                Console.WriteLine("login listening stopped"); //Wyskoczy zawsze podczas zamykania
            }
            
        }
        public static void Initialize()
        {
            running = true;
            loggedClients = new Dictionary<string, Client>();
            loggedClientsAccess = new Mutex();
            openTablesAccess = new Mutex();
            loginListener = new TcpListener(IPAddress.Any, (Int32)6937);
            gameListener = new TcpListener(IPAddress.Any, (Int32)6938);
            loggedTokens = new List<string>();
            openTables = new List<GameTable>();
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
                loggedClientsAccess.WaitOne();
                for (int i = loggedTokens.Count - 1; i >= 0; i--)
                {
                    string token = loggedTokens[i];
                    Socket s = loggedClients[token].MenuRequestsStream.Socket;
                    bool pt1 = s.Poll(1000, SelectMode.SelectRead);
                    bool pt2 = (s.Available == 0);
                    if(pt1 && pt2)
                    {
                        loggedClients[token].MenuRequestsTcp.Close();
                        loggedClients[token].MenuRequestsStream.Dispose();
                        loggedClients[token].GameRequestsTcp.Close();
                        loggedClients[token].GameRequestsStream.Dispose();
                        loggedClients.Remove(token);
                        loggedTokens.RemoveAt(i);
                    }
  
                }
                loggedClientsAccess.ReleaseMutex();
                Thread.Sleep(10000);
            }
            Console.WriteLine("Auto Logout stopped");
        }
    }
}
