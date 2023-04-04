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

        public static IDictionary<string, Player> loggedClients;
        public static List<string> loggedTokens;
        public static List<GameTable> openTables;

        public static Mutex openTablesAccess;
        public static Mutex loggedClientsAccess;

        public static TcpListener loginListener;
        public static TcpListener gameListener;

        public static Thread oneGameThread;
        public static GameTable tableToStartGame;

        
        static void Main()
        {
            //Zmienne potrzebne do wyswietlania stanu w //RUN
            ConsoleKeyInfo cki;
            Console.CursorVisible = false;
            var sb = new StringBuilder();
            var emptySpace = new StringBuilder();
            emptySpace.Append(' ', 10);
            DateTime startTime = DateTime.Now;
            

            //INITIALIZE
            Initialize();
            Thread loginThread = new Thread(ListenLogin);
            Thread autoLogoutThread = new Thread(AutoLogout);
            Thread requestsThread = new Thread(ListenRequests);

            oneGameThread = null;
            tableToStartGame = null;

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
                loggedClientsAccess.WaitOne();
                sb.Append("Logged clients: " + loggedClients.Count + emptySpace + '\n');
                loggedClientsAccess.ReleaseMutex();
                openTablesAccess.WaitOne();
                sb.Append("Open tables: " + openTables.Count + emptySpace + '\n');
                openTablesAccess.ReleaseMutex();
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

            if(oneGameThread != null)
                oneGameThread.Join();

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
                //Przejdz po wszystkich zalogowanych tokenach (od tyłu na potrzeby usuwania)
                for(int i=loggedTokens.Count-1; i >= 0; i--)
                {
                    string token = loggedTokens[i];
                    Player player = loggedClients[token];
                    //Jesli jest cos do przeczytania od danego clienta
                    if (player.MenuRequestsStream.DataAvailable)
                    {
                        //Przeczytaj max 256 bajtów -> zamień na ascii -> słowa podzielone przez spacje -> usun WSZYSTKO co w streamie nadmiarowe
                        byte[] readBuffer = new byte[256];
                        StringBuilder menuRequestStrings = new StringBuilder();
                        int bytesRead = player.MenuRequestsStream.Read(readBuffer, 0, readBuffer.Length);
                        menuRequestStrings.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
                        string[] request = menuRequestStrings.ToString().Split(new char[] { ' ' });
                        player.MenuRequestsStream.Flush();

                        //'token' '1 literowy kod' 'argumenty'
                        //Test czy token wysłany odpowiada tokenowi clienta (zabezpieczenie)
                        if(token == request[0])
                        {
                            //Utworzenie stołu
                            if (request[1] == "0") 
                            {
                                //  2      3    4      5        6
                                //nazwa, tryb, bots, min xp, min stawka
                                string name = request[2];

                                bool found = false;
                                openTablesAccess.WaitOne();
                                //Tylko dla clienta ktory nie jest przy stole
                                if(player.Table == null)
                                {
                                    //Przeszukanie czy podana nazwa nie jest juz zajeta przez inny stół (Name to ID stołu)
                                    foreach(GameTable table in openTables)
                                        if (table.Name == name)
                                            found = true;

                                    if (!found)
                                    {
                                        //Utworzenie stołu
                                        GameTable gameTable = new GameTable(name, (HumanPlayer)player);
                                        //Ustawienie przy jakim stole jest Client
                                        loggedClients[token].Table = gameTable;
                                        ChangeTableSettings(gameTable, player, request[3], request[4], request[5], request[6]);
                                        openTables.Add(gameTable);
                                    }
                                }
                                openTablesAccess.ReleaseMutex();
                            }
                            //Dolaczenie do stolu
                            else if (request[1] == "1") 
                            {
                                openTablesAccess.WaitOne();
                                Player client = player;
                                if (client.Table == null) // Dołączymy do nowego stołu tylko, jeśli przy żadnym nie siedzimy
                                {
                                    string name = request[2];
                                    bool found = false;
                                    GameTable table = null;
                                    //Szukanie nazwy stołu do którego chce sie dolaczyc
                                    foreach (GameTable tab in openTables)
                                        if (tab.Name == name)
                                        {
                                            found = true;
                                            table = tab;
                                        }      
                                    if (found)
                                    {
                                        //jesli udalo sie dodac gracza do stołu
                                        bool git = table.AddPlayer(client);
                                        if (git)
                                        {
                                            client.Table = table;
                                        }
                                        else
                                        {
                                            client.Table = null;
                                        }
                                    }
                                    
                                }
                                openTablesAccess.ReleaseMutex();
                            }
                            //informacje o stołach
                            else if (request[1] == "2")
                            {
                                //Uwaga! Nie wysyła informacji o stolikach, kiedy się przy jakimś siedzi --> dlatego pojedynczy klient nigdy nie dostaje listy stolików
                                if (player.Table == null) 
                                {
                                    StringBuilder completeMessage = new StringBuilder();
                                    openTablesAccess.WaitOne();
                                    foreach (GameTable table in openTables)
                                    {
                                        completeMessage.Append(table.toMessage());
                                    }
                                    byte[] message = System.Text.Encoding.ASCII.GetBytes(completeMessage.ToString());
                                    player.MenuRequestsStream.Write(message, 0, message.Length);
                                    openTablesAccess.ReleaseMutex();
                                }
                            }
                            //wylogowanie
                            else if (request[1] == "3") 
                            {
                                RemoveFromTable(player);
                                LogOut(player, i);
                            }
                            //Odejscie od stołu
                            else if (request[1] == "4") 
                            {
                                RemoveFromTable(player);
                            }
                            //Zmiana ustawień
                            else if (request[1] == "5") 
                            {
                                openTablesAccess.WaitOne();
                                Player client = loggedClients[token];

                                if(client.Table != null)
                                {
                                    ChangeTableSettings(client.Table, client, request[2], request[3], request[4], request[5]);
                                }
                                openTablesAccess.ReleaseMutex();
                            }
                            //Rozpocznij grę
                            else if (request[1] == "6")
                            {
                                //openTablesAccess.WaitOne();
                                Player client = loggedClients[token];
                                if (client.Table != null)
                                {
                                    tableToStartGame = client.Table;
                                    oneGameThread = new Thread(Game);
                                    oneGameThread.Start();
                                    //NetworkHelper.WriteNetworkStream(client.GameRequestsStream, "Game started on server");
                                    //client.GameRequestsStream.Flush();
                                }
                                //openTablesAccess.ReleaseMutex();
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
                            bool is_logged = false;
                            loggedClientsAccess.WaitOne();
                            foreach(string tok in loggedTokens)
                            {
                                if (username == loggedClients[tok].Login)
                                {
                                    is_logged = true;
                                    byte[] message = System.Text.Encoding.ASCII.GetBytes("##&&@@0003");
                                    clientStream.Write(message, 0, message.Length);
                                    break;
                                }   
                            }
                            loggedClientsAccess.ReleaseMutex();
                            if(!is_logged)
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
                                Console.WriteLine("Accepted game client");

                                loggedClientsAccess.WaitOne();

                                loggedClients[token] = new HumanPlayer(nick, PlayerType.Human);
                                loggedClients[token].UpdateData(xpI, coinsI, login);
                                loggedClients[token].Token = token;
                                loggedClients[token].MenuRequestsTcp = client;
                                loggedClients[token].MenuRequestsStream = client.GetStream();
                                loggedClients[token].GameRequestsTcp = gameClient;
                                loggedClients[token].GameRequestsStream = gameClient.GetStream();
                                loggedTokens.Add(token);
                                loggedClientsAccess.ReleaseMutex();
                                Console.WriteLine("Added client to list");
                            }
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
            loggedClients = new Dictionary<string, Player>();
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
                    //Ogolnie to te warunki znalezione w necie, dokladnie czemu tak sie dzieje to nie pamietam
                    Socket s = loggedClients[token].MenuRequestsStream.Socket;
                    bool pt1 = s.Poll(1000, SelectMode.SelectRead);
                    bool pt2 = (s.Available == 0);
                    if(pt1 && pt2)
                    {
                        RemoveFromTable(loggedClients[token]);
                        LogOut(loggedClients[token], i);
                    }
                }
                loggedClientsAccess.ReleaseMutex();
                Thread.Sleep(10000);
            }
            Console.WriteLine("Auto Logout stopped");
        }
        public static void LogOut(Player player, int i)
        {
            player.MenuRequestsTcp.Close();
            player.MenuRequestsStream.Dispose();
            player.GameRequestsTcp.Close();
            player.GameRequestsStream.Dispose();
            loggedClients.Remove(player.Token);
            loggedTokens.RemoveAt(i);
            //@@@@@@@@@@@@ Działa i szkoda odpalac przy testowaniu obecnie
            //UpdateBoth(player);
        }
        public static void RemoveFromTable(Player player)
        {
            openTablesAccess.WaitOne();
            if (player.Table != null)
            {
                GameTable tmp = player.Table;
                tmp.Remove(player.Nick);
                player.Table = null;
                if (tmp.GetPlayerTypeCount(PlayerType.Human) == 0)
                {
                    openTables.Remove(tmp);
                }
            }
            openTablesAccess.ReleaseMutex();
        }
        public static void UpdateBoth(Player player)
        {
            UpdateUserCoinsOrXP(player.Login, "coins", player.TokensCount);
            UpdateUserCoinsOrXP(player.Login, "xp", player.XP);
        }
        public static void UpdateUserCoinsOrXP(string playerLogin, string item, int value)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/updatevalues");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"login\":\"" + playerLogin + "\"," +
                                "\"toUpdate\":\"" + item + "\"," +
                                "\"updatedValue\":\"" + value + "\"}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                var dataFromDatabase = result.Split("\"");
            }
        }
        public static void Game()
        {
            GameTable table = tableToStartGame;
            //NetworkHelper.WriteNetworkStream(table.Players[0].GameRequestsStream, "We're on our best way to play the game!");
            //table.Players[0].GameRequestsStream.Flush();            
            GameplayController controller = new GameplayController(table, new TexasHoldemDealer());
            tableToStartGame = null;
            controller.playTheGame();
            controller.ConcludeGame();
        }
        public static void ChangeTableSettings(GameTable table,Player player, string mode, string nrOfBots, string minXp, string big_blind)
        {
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

            table.ChangeSettings((HumanPlayer)player, gameTableSettings);
        }
    }
}
