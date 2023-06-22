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

        public static List<Thread> allGameThreads;

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

            allGameThreads = new List<Thread>();

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

                Thread.Sleep(500);
                // Usuń wątki gry, które się już skończyły
                allGameThreads.RemoveAll(t => !t.IsAlive);
                //Console.WriteLine("Active threads: " + allGameThreads.Count);

            }

            //EXIT
            loginListener.Stop();
            gameListener.Stop();
            loginThread.Join();
            autoLogoutThread.Join();
            requestsThread.Join();

            foreach (Thread gameThread in allGameThreads)
                gameThread.Join();

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
                for (int i = loggedTokens.Count - 1; i >= 0; i--)
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
                        if (token == request[0])
                        {
                            //Utworzenie stołu
                            if (request[1] == "0")
                            {
                                //  2      3    4      5        6           7
                                //nazwa, tryb, bots, min xp, min stawka, big blind
                                byte[] answear = null;
                                bool valid = true;

                                if (request[2].Length > 32)
                                    valid = false;
                                else if (!ValidateStringIfPositiveInt(request[4]))
                                    valid = false;
                                else if (!ValidateStringIfPositiveInt(request[5]))
                                    valid = false;
                                else if (!ValidateStringIfPositiveInt(request[6]))
                                    valid = false;
                                else if (!ValidateStringIfPositiveInt(request[7]))
                                    valid = false;


                                openTablesAccess.WaitOne();
                                //Tylko dla clienta ktory nie jest przy stole
                                if (player.Table == null && valid)
                                {
                                    string name = request[2];
                                    bool found = false;
                                    //Przeszukanie czy podana nazwa nie jest juz zajeta przez inny stół (Name to ID stołu)
                                    foreach (GameTable table in openTables)
                                        if (table.Name == name)
                                        {
                                            found = true;
                                        }
                                            

                                    if (!found)
                                    {
                                        //Utworzenie stołu
                                        GameTable gameTable = new GameTable(name, (HumanPlayer)player);
                                        //Ustawienie przy jakim stole jest Client
                                        loggedClients[token].Table = gameTable;
                                        ChangeTableSettings(gameTable, player, request[3], request[4], request[5], request[6], request[7]);
                                        openTables.Add(gameTable);
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer 0 0 "); // odpowiedź OK
                                    }
                                    else
                                    {
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer 0 2 "); // juz jest taka nazwa
                                    }
                                }
                                else
                                {
                                    if(!valid)
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer 0 9 "); // błąd w walidacji
                                    else
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer 0 1 "); // gracz przy stole
                                }
                                player.MenuRequestsStream.Write(answear, 0, answear.Length);
                                openTablesAccess.ReleaseMutex();
                            }
                            //Dolaczenie do stolu
                            else if (request[1] == "1")
                            {
                                openTablesAccess.WaitOne();

                                Console.WriteLine("Client " + player.Nick + " requested adding to table " + request[2]);
                                byte[] answear = null;

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
                                            answear = System.Text.Encoding.ASCII.GetBytes("answer 1 0 "); // odpowiedź OK
                                           
                                        }
                                        else
                                        {
                                            client.Table = null;
                                            answear = System.Text.Encoding.ASCII.GetBytes("answer 1 3 "); // odpowiedź FAILED
                                        }
                                    }
                                    else
                                    {
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer 1 2 "); // odpowiedź FAILED
                                    }

                                }
                                else
                                {
                                    answear = System.Text.Encoding.ASCII.GetBytes("answer 1 1 "); // odpowiedź FAILED
                                }
                                player.MenuRequestsStream.Write(answear, 0, answear.Length);
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
                                byte[] answear = System.Text.Encoding.ASCII.GetBytes("answer 3 0 "); // odpowiedź OK
                                player.MenuRequestsStream.Write(answear, 0, answear.Length);
                            }
                            //Odejscie od stołu
                            else if (request[1] == "4")
                            {
                                RemoveFromTable(player);
                                byte[] answear = System.Text.Encoding.ASCII.GetBytes("answer 4 0 "); // odpowiedź OK
                                player.MenuRequestsStream.Write(answear, 0, answear.Length);
                            }
                            //Zmiana ustawień
                            else if (request[1] == "5")
                            {
                                byte[] answear = null;
                                bool valid = true;
                                openTablesAccess.WaitOne();
                                Player client = loggedClients[token];

                                if (!ValidateStringIfPositiveInt(request[3]))
                                    valid = false;
                                if (!ValidateStringIfPositiveInt(request[4]))
                                    valid = false;
                                if (!ValidateStringIfPositiveInt(request[5]))
                                    valid = false;
                                if (!ValidateStringIfPositiveInt(request[6]))
                                    valid = false;

                                if (client.Table != null && valid)
                                {
                                    ChangeTableSettings(client.Table, client, request[2], request[3], request[4], request[5], request[6]);
                                    answear = System.Text.Encoding.ASCII.GetBytes("answer 5 0 "); // odpowiedź OK
                                }
                                else
                                {
                                    if (!valid)
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer 5 9 "); // odpowiedź Failed
                                    else
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer 5 1 "); // odpowiedź Failed
                                }
                                player.MenuRequestsStream.Write(answear, 0, answear.Length);
                                openTablesAccess.ReleaseMutex();
                            }
                            //Rozpocznij grę
                            else if (request[1] == "6")
                            {
                                byte[] answear = null;
                                Player client = loggedClients[token];
                                if (client.Table != null)
                                {
                                    Thread gameThread = new Thread(() => Game(client.Table));
                                    allGameThreads.Add(gameThread);
                                    gameThread.Start();
                                    answear = System.Text.Encoding.ASCII.GetBytes("answer 6 0 "); // odpowiedź OK
                                }
                                else
                                {
                                    answear = System.Text.Encoding.ASCII.GetBytes("answer 6 1 "); // odpowiedź Failed
                                }

                                player.MenuRequestsStream.Write(answear, 0, answear.Length);
                            }
                            //Zmień liczbę żetonów gracza
                            else if(request[1] == "7")
                            {
                                bool valid = true;
                                byte[] answear = null;
                                if (!ValidateStringIfPositiveInt(request[2]))
                                    valid = false;

                                if (valid)
                                {
                                    Player client = loggedClients[token];
                                    int coins = Convert.ToInt32(request[2]);
                                    client.TokensCount += coins;
                                    NetworkHelper.WriteNetworkStream(client.MenuRequestsStream, "1" + ' ' + client.TokensCount.ToString());
                                    client.MenuRequestsStream.Flush();
                                    UpdateUserCoinsOrXP(client.Login, "coins", player.TokensCount);
                                    answear = System.Text.Encoding.ASCII.GetBytes("answer 7 0 "); // odpowiedź OK
                                }
                                else
                                {
                                    answear = System.Text.Encoding.ASCII.GetBytes("answer 7 9 "); // odpowiedź Failed
                                }
                                player.MenuRequestsStream.Write(answear, 0, answear.Length);
                            }
                            //zmiana nicku
                            else if(request[1] == "8")
                            {
                                byte[] answear = null;
                                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/updatenick");
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "POST";
                                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                                {
                                    string json = "{\"login\":\"" + player.Login + "\"," +
                                                    "\"nick\":\"" + request[2] + "\"}";
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
                                    if (responseCode == "200")
                                    {
                                        player.Login = request[2];
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer 8 0 ");
                                    }
                                    else if (responseCode == "405")
                                    {
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer 8 1 ");
                                    }
                                    else
                                    {
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer 8 2 ");
                                    }
                                }
                                player.MenuRequestsStream.Write(answear, 0, answear.Length);
                            }
                            //zmiana hasla
                            else if (request[1] == "9")
                            {
                                byte[] answear = null;
                                
                                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/updatepasswd");
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "POST";
                                var currPass = request[2];
                                var newPass = request[3];
                                var confPass = request[4];
                                if(newPass == confPass)
                                {
                                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                                    {
                                        string json = "{\"login\":\"" + player.Login + "\"," +
                                                      "\"currentPassword\":\"" + currPass + "\"," +
                                                      "\"newPassword\":\"" + newPass + "\"}";
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
                                        if (responseCode == "200")
                                        {
                                            //poprawnie zmienione
                                            answear = System.Text.Encoding.ASCII.GetBytes("answer 9 0 ");
                                        }
                                        else if (responseCode == "401")
                                        {
                                            //wyslij info uzytkownikowi o zlym hasle
                                            answear = System.Text.Encoding.ASCII.GetBytes("answer 9 2 ");
                                        }
                                        else
                                        {
                                            //wyslij info uzytkownikowi o bledzie innym niz wymienione (np server error)
                                            answear = System.Text.Encoding.ASCII.GetBytes("answer 9 3 ");
                                        }
                                    }
                                }
                                else
                                {
                                    answear = System.Text.Encoding.ASCII.GetBytes("answer 9 1 ");
                                }
                                player.MenuRequestsStream.Write(answear, 0, answear.Length);
                            }
                            //usun konto
                            else if (request[1] == "10")
                            {
                                byte[] answear = null;
                                

                                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/deleteaccount");
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "POST";
                                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                                {
                                    string json = "{\"login\":\"" + player.Login + "\"," +
                                                  "\"password\":\"" + request[2] + "\"}";
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
                                    if (responseCode == "200")
                                    {
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer A 0 ");

                                        loggedClientsAccess.WaitOne();
                                        Socket s = loggedClients[token].MenuRequestsStream.Socket;
                                        bool pt1 = s.Poll(1000, SelectMode.SelectRead);
                                        bool pt2 = (s.Available == 0);
                                        if (pt1 && pt2)
                                        {
                                            player.MenuRequestsStream.Write(answear, 0, answear.Length);
                                            player.MenuRequestsTcp.Close();
                                            player.MenuRequestsStream.Dispose();
                                            player.GameRequestsTcp.Close();
                                            player.GameRequestsStream.Dispose();
                                            loggedClients.Remove(player.Token);
                                            loggedTokens.RemoveAt(i);
                                        }
                                        loggedClientsAccess.ReleaseMutex();
                                        Thread.Sleep(10000);
                                    }
                                    else if (responseCode == "401")
                                    {
                                        // zle haslo
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer A 1 ");
                                        player.MenuRequestsStream.Write(answear, 0, answear.Length);
                                    }
                                    else
                                    {
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer A 2 ");
                                        player.MenuRequestsStream.Write(answear, 0, answear.Length);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine(request);
                                byte[] answear = System.Text.Encoding.ASCII.GetBytes("answer 100 1 "); // odpowiedź Failed
                                player.MenuRequestsStream.Write(answear, 0, answear.Length);
                            }
                               
                        }
                    }
                }
                loggedClientsAccess.ReleaseMutex();

                
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
                    //wczesniej (akceptuje port) (otrzymuje login i haslo) (odpowiadam ok lub nie ok) (akceptuje drugi port)
                    //teraz  (akceptuje port) (wysylam klucz publiczny) (otrzymuje zaszyfrowany login i haslo i deszyfruje) (odpowiadam ok lub nie) 
                    //(akceptuje drugi port)

                    //(akceptuje port)
                    TcpClient client = loginListener.AcceptTcpClient();
                    NetworkStream clientStream = client.GetStream();

                    //(wysylam klucz publiczny)
                    var rsa = new RSACryptoServiceProvider(1024);
                    rsa.PersistKeyInCsp = false;
                    var publicKeyXml = rsa.ToXmlString(false);
                    byte[] publicKeyBytes = System.Text.Encoding.ASCII.GetBytes(publicKeyXml);
                    clientStream.Write(publicKeyBytes, 0, publicKeyBytes.Length);

                    //(otrzymuje zaszyfrowany login i haslo i deszyfruje)
                    byte[] readBuffer = new byte[128];
                    int numberOfBytesRead = 0;
                    
                    numberOfBytesRead = clientStream.Read(readBuffer, 0, readBuffer.Length);
                    var decryptedLognPass = rsa.Decrypt(readBuffer, false);

                    StringBuilder lognpass = new StringBuilder();
                    lognpass.AppendFormat("{0}", Encoding.ASCII.GetString(decryptedLognPass, 0, decryptedLognPass.Length));

                    clientStream.Flush();
                    string[] loginAndPassword = lognpass.ToString().Split(new char[] { ' ' }, 2);

                    string username = loginAndPassword[0];
                    string password = loginAndPassword[1];

                    //(odpowiadam ok lub nie) 
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
                            foreach (string tok in loggedTokens)
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
                            if (!is_logged)
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

                                //(akceptuje drugi port)
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
                    rsa.Dispose();
                }
            }
            catch (SocketException ex)
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
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
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
                    if (pt1 && pt2)
                    {
                        //RemoveFromTable(loggedClients[token]);
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
            UpdateBoth(player);
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
        public static void Game(GameTable table)
        {
            GameplayController controller = new GameplayController(table, new TexasHoldemDealer());
            // TODO poprawić, żeby gra była włączana np. co 10 sekund i nie n razy, tylko dopóki wszyscy gracze nie opuszczą stolika
            for (int i = 0; i < table.Players.Count; i++)
            {
                controller.playTheGame();
                controller.ConcludeGame();
                Thread.Sleep(10000);
            }
        }
        public static void ChangeTableSettings(GameTable table, Player player, string mode, string nrOfBots, string minXp, string min_tokens, string big_blind)
        {
            GameTableSettings gameTableSettings = new GameTableSettings();
            if (mode == "0")
                gameTableSettings.changeMode(GameMode.No_Bots);
            else if (mode == "1")
                gameTableSettings.changeMode(GameMode.You_And_Bots);
            else if (mode == "2")
                gameTableSettings.changeMode(GameMode.Mixed);

            // (note: BotsCount != BotsNumberOnStart)
            // bots count is dynamic and changes depending on how many bots sits at the table,
            // and bots number on start is just an initial setting
            gameTableSettings.changeBotsNumber(int.Parse(nrOfBots));

            gameTableSettings.changeMinXP(int.Parse(minXp));
            gameTableSettings.changeMinTokens(int.Parse(min_tokens));
            gameTableSettings.changeBigBlind(int.Parse(big_blind));
            //Console.WriteLine(gameTableSettings);

            table.ChangeSettings((HumanPlayer)player, gameTableSettings);
        }

        public static bool ValidateStringIfPositiveInt(string attribute)
        {
            Regex regex = new Regex(@"^\d+$");
            if (regex.IsMatch(attribute))
            {
                if(attribute.Length <= 9)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
}
