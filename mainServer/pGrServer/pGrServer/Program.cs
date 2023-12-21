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
        public static List<string> tokennedPlayers;
        public static DateTime freeTokensLastClear;
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
                //if (loggedClients.Count > 2)
                //{
                //    sb.AppendLine();
                //    sb.AppendLine(loggedClients[loggedTokens[0]].Nick + " " + loggedClients[loggedTokens[0]].TokensCount.ToString());
                //    sb.AppendLine(loggedClients[loggedTokens[1]].Nick + " " + loggedClients[loggedTokens[1]].TokensCount.ToString());
                //    sb.AppendLine(loggedClients[loggedTokens[2]].Nick + " " + loggedClients[loggedTokens[2]].TokensCount.ToString());
                //    sb.AppendLine();
                //}
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
                                byte[] answer = null;
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
                                if (request.Length > 8)
                                {

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
                                            answer = System.Text.Encoding.ASCII.GetBytes("answer 0 0 "); // odpowiedź OK
                                        }
                                        else
                                        {
                                            answer = System.Text.Encoding.ASCII.GetBytes("answer 0 2 "); // juz jest taka nazwa
                                        }
                                    }
                                    else
                                    {
                                        if (!valid)
                                            answer = System.Text.Encoding.ASCII.GetBytes("answer 0 9 "); // błąd w walidacji
                                        else if (player.Table != null)
                                            answer = System.Text.Encoding.ASCII.GetBytes("answer 0 1 "); // gracz przy stole
                                    }
                                }
                                else
                                {
                                    answer = System.Text.Encoding.ASCII.GetBytes("answer 0 A "); // błąd pakietu
                                }

                                player.MenuRequestsStream.Write(answer, 0, answer.Length);
                                openTablesAccess.ReleaseMutex();
                            }
                            //Dolaczenie do stolu
                            else if (request[1] == "1")
                            {
                                openTablesAccess.WaitOne();

                                Console.WriteLine("Client " + player.Nick + " requested adding to table " + request[2]);
                                byte[] answer = null;

                                Player client = player;
                                if (request.Length > 3)
                                {
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
                                                answer = System.Text.Encoding.ASCII.GetBytes("answer 1 0 "); // odpowiedź OK
                                                foreach(var ppl in client.Table.Players)
                                                {
                                                    if(ppl != client)
                                                    {
                                                        byte[] info = System.Text.Encoding.ASCII.GetBytes("new player " + ppl.Nick + " " + ppl.TokensCount + " " + ppl.XP + " ");
                                                        ppl.MenuRequestsStream.Write(info, 0, info.Length);
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                client.Table = null;
                                                answer = System.Text.Encoding.ASCII.GetBytes("answer 1 3 "); // odpowiedź FAILED
                                            }
                                        }
                                        else
                                        {
                                            //Kiedy nie znaleziono
                                            answer = System.Text.Encoding.ASCII.GetBytes("answer 1 2 "); // odpowiedź FAILED
                                        }

                                    }
                                    else
                                    {
                                        //Kiedy juz jestesmy przy stoliku
                                        answer = System.Text.Encoding.ASCII.GetBytes("answer 1 1 "); // odpowiedź FAILED
                                    }
                                }
                                else
                                {
                                    answer = System.Text.Encoding.ASCII.GetBytes("answer 1 A "); // za krótki pakiet
                                }

                                player.MenuRequestsStream.Write(answer, 0, answer.Length);
                                openTablesAccess.ReleaseMutex();
                            }
                            //informacje o stołach
                            else if (request[1] == "2")
                            {
                                //Uwaga! Nie wysyła informacji o stolikach, kiedy się przy jakimś siedzi --> dlatego pojedynczy klient nigdy nie dostaje listy stolików
                                openTablesAccess.WaitOne();
                                StringBuilder completeMessage = new StringBuilder();
                                if (player.Table == null)
                                {
                                    if (openTables.Count > 0)
                                    {
                                        completeMessage.Append("answer 2 0 ");
                                        foreach (GameTable table in openTables)
                                        {
                                            completeMessage.Append(table.toMessage());
                                        }
                                    }
                                    else
                                    {
                                        completeMessage.Append("answer 2 1 ");
                                    }


                                }
                                else
                                {
                                    completeMessage.Append("answer 2 2 ");
                                }
                                byte[] message = System.Text.Encoding.ASCII.GetBytes(completeMessage.ToString());
                                player.MenuRequestsStream.Write(message, 0, message.Length);
                                openTablesAccess.ReleaseMutex();
                            }
                            //wylogowanie
                            else if (request[1] == "3")
                            {
                                byte[] answer = System.Text.Encoding.ASCII.GetBytes("answer 3 0 "); // odpowiedź OK
                                player.MenuRequestsStream.Write(answer, 0, answer.Length);
                                if(player.Table != null)
                                    RemoveFromTable(player);
                                LogOut(player, i);

                            }
                            //Odejscie od stołu
                            else if (request[1] == "4")
                            {
                                byte[] answer;
                                if (player.Table != null)
                                {
                                    if (!player.Table.isGameActive)
                                    {
                                        RemoveFromTable(player);
                                        answer = System.Text.Encoding.ASCII.GetBytes("answer 4 0 "); // odpowiedź OK
                                        foreach (var ppl in player.Table.Players)
                                        {
                                            if (ppl != player)
                                            {
                                                byte[] info = System.Text.Encoding.ASCII.GetBytes("rem player " + ppl.Nick + " ");
                                                ppl.MenuRequestsStream.Write(info, 0, info.Length);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        answer = System.Text.Encoding.ASCII.GetBytes("answer 4 1 "); // odpowiedź Failed
                                    }
                                }
                                else
                                {
                                    answer = System.Text.Encoding.ASCII.GetBytes("answer 4 1 "); // odpowiedź Failed
                                }
                                player.MenuRequestsStream.Write(answer, 0, answer.Length);

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

                                if (request.Length > 7)
                                {
                                    if (client.Table != null)
                                    {
                                        if (!client.Table.alreadyHasGameThread)
                                        {
                                            if (valid)
                                            {
                                                ChangeTableSettings(client.Table, client, request[2], request[3], request[4], request[5], request[6]);
                                                answear = System.Text.Encoding.ASCII.GetBytes("answer 5 0 "); // odpowiedź OK
                                            }
                                            else
                                            {
                                                answear = System.Text.Encoding.ASCII.GetBytes("answer 5 9 "); // odpowiedź Failed

                                            }
                                        }
                                        else
                                        {
                                            answear = System.Text.Encoding.ASCII.GetBytes("answer 5 2 ");
                                        }
                                    }
                                    else
                                    {
                                        answear = System.Text.Encoding.ASCII.GetBytes("answer 5 1 ");
                                    }
                                }
                                else
                                {
                                    answear = System.Text.Encoding.ASCII.GetBytes("answer 5 A ");
                                }
                                player.MenuRequestsStream.Write(answear, 0, answear.Length);
                                openTablesAccess.ReleaseMutex();
                            }
                            //Rozpocznij grę
                            else if (request[1] == "6")
                            {
                                byte[] answer = null;
                                Player client = loggedClients[token];
                                if (client.Table != null && client.Table.alreadyHasGameThread == false) // nie rozpoczynamy nowego wątky gry dla stolika, który już ma taki wątek
                                {
                                    if(client.Table.GetPlayerCount() > 1)
                                    {
                                        Thread gameThread = new Thread(() => Game(client.Table));
                                        allGameThreads.Add(gameThread);
                                        gameThread.Start();
                                        answer = System.Text.Encoding.ASCII.GetBytes("answer 6 0 "); // odpowiedź OK
                                    }
                                    else
                                    {
                                        answer = System.Text.Encoding.ASCII.GetBytes("answer 6 2 "); // odpowiedź za malo graczy
                                    }
                                }
                                else
                                {
                                    answer = System.Text.Encoding.ASCII.GetBytes("answer 6 1 "); // odpowiedź Failed
                                }

                                player.MenuRequestsStream.Write(answer, 0, answer.Length);
                            }
                            //Odbierz żetony
                            else if (request[1] == "7")
                            {
                                bool valid = true;
                                byte[] answer = null;
                                if (player.Table == null)
                                {
                                    DateTime now = DateTime.Now;
                                    TimeSpan difference = now - freeTokensLastClear;
                                    if (difference.TotalHours >= 12)
                                    {
                                        tokennedPlayers.Clear();
                                        freeTokensLastClear = now;
                                        if (now.Hour >= 12)
                                        {
                                            TimeSpan ts = new TimeSpan(12, 0, 0);
                                            freeTokensLastClear = freeTokensLastClear.Date + ts;
                                        }
                                        else
                                        {
                                            TimeSpan ts = new TimeSpan(0, 0, 0);
                                            freeTokensLastClear = freeTokensLastClear.Date + ts;
                                        }
                                        //Console.WriteLine("\n\n\n\n\nXDDDD" + freeTokensLastClear.ToString());

                                    }
                                    foreach (string ppl in tokennedPlayers)
                                    {
                                        if (ppl == player.Login)
                                        {
                                            valid = false;
                                        }
                                    }
                                    if (valid)
                                    {
                                        Player client = loggedClients[token];
                                        //TODO
                                        int freeTokens = 100;//Tutaj mozna tę wartość zrobić dynamicznie zaleznie od XP
                                        client.TokensCount += freeTokens;
                                        //NetworkHelper.WriteNetworkStream(client.MenuRequestsStream, "1" + ' ' + client.TokensCount.ToString());
                                        tokennedPlayers.Add(player.Login);
                                        client.MenuRequestsStream.Flush();
                                        UpdateUserCoinsOrXP(client.Login, "coins", player.TokensCount);
                                        answer = System.Text.Encoding.ASCII.GetBytes("answer 7 0 " + freeTokens.ToString() + " "); // odpowiedź OK
                                    }
                                    else
                                    {
                                        if (freeTokensLastClear.Hour == 12)
                                            answer = System.Text.Encoding.ASCII.GetBytes("answer 7 1 " + (24 - now.Hour).ToString() + " "); // odpowiedź Failed
                                        else
                                            answer = System.Text.Encoding.ASCII.GetBytes("answer 7 1 " + (12 - now.Hour).ToString() + " "); // odpowiedź Failed
                                    }
                                }
                                else
                                {
                                    answer = System.Text.Encoding.ASCII.GetBytes("answer 7 2 "); // odpowiedź Failed
                                }
                                player.MenuRequestsStream.Write(answer, 0, answer.Length);
                            }
                            //zmiana nicku
                            else if (request[1] == "8")
                            {
                                byte[] answer = null;
                                if (request.Length > 4)
                                {

                                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/updatenick");
                                    httpWebRequest.ContentType = "application/json";
                                    httpWebRequest.Method = "POST";
                                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                                    {
                                        string json = "{\"login\":\"" + player.Login + "\"," +
                                                        "\"nick\":\"" + request[2] + "\"," +
                                                        "\"password\":\"" + request[3] + "\"}";
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
                                            player.Nick = request[2];
                                            answer = System.Text.Encoding.ASCII.GetBytes("answer 8 0 ");
                                        }
                                        else if (responseCode == "405")
                                        {
                                            answer = System.Text.Encoding.ASCII.GetBytes("answer 8 1 ");
                                        }
                                        else if (responseCode == "401")
                                        {
                                            answer = System.Text.Encoding.ASCII.GetBytes("answer 8 2 ");
                                        }
                                        else
                                        {
                                            answer = System.Text.Encoding.ASCII.GetBytes("answer 8 3 ");
                                        }
                                    }

                                }
                                else
                                {
                                    answer = System.Text.Encoding.ASCII.GetBytes("answer 8 A ");
                                }
                                player.MenuRequestsStream.Write(answer, 0, answer.Length);
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
                                if (request.Length > 5)
                                {
                                    if (newPass == confPass)
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
                                }
                                else
                                {
                                    answear = System.Text.Encoding.ASCII.GetBytes("answer 9 A ");
                                }
                                player.MenuRequestsStream.Write(answear, 0, answear.Length);
                            }
                            //usun konto
                            else if (request[1] == "A")
                            {
                                byte[] answer = null;

                                if (request.Length > 3)
                                {
                                    if (player.Table == null)
                                    {
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
                                                answer = System.Text.Encoding.ASCII.GetBytes("answer A 0 ");
                                                player.MenuRequestsStream.Write(answer, 0, answer.Length);

                                                loggedClientsAccess.WaitOne();
                                                Socket s = loggedClients[token].MenuRequestsStream.Socket;
                                                bool pt1 = s.Poll(1000, SelectMode.SelectRead);
                                                bool pt2 = (s.Available == 0);
                                                if (pt1 && pt2)
                                                {
                                                    player.MenuRequestsStream.Write(answer, 0, answer.Length);
                                                    player.MenuRequestsTcp.Close();
                                                    player.MenuRequestsStream.Dispose();
                                                    player.GameRequestsTcp.Close();
                                                    player.GameRequestsStream.Dispose();
                                                    loggedClients.Remove(player.Token);
                                                    loggedTokens.RemoveAt(i);
                                                }
                                                loggedClientsAccess.ReleaseMutex();
                                            }
                                            else if (responseCode == "401")
                                            {
                                                // zle haslo
                                                answer = System.Text.Encoding.ASCII.GetBytes("answer A 1 ");
                                                player.MenuRequestsStream.Write(answer, 0, answer.Length);
                                            }
                                            else
                                            {
                                                //inny błąd aws
                                                answer = System.Text.Encoding.ASCII.GetBytes("answer A 2 ");
                                                player.MenuRequestsStream.Write(answer, 0, answer.Length);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // info, że gracz nie może teraz tego zrobić (najlepiej dosłownie info “Nie możesz teraz usunąć konta).
                                        //jest przy stole
                                        answer = System.Text.Encoding.ASCII.GetBytes("answer A 3 ");
                                        player.MenuRequestsStream.Write(answer, 0, answer.Length);
                                    }
                                }
                                else
                                {
                                    //za krótki pakiet
                                    answer = System.Text.Encoding.ASCII.GetBytes("answer A A ");
                                    player.MenuRequestsStream.Write(answer, 0, answer.Length);
                                }

                            }
                            else if (request[1] == "B")
                            {
                                int currXP = player.XP;
                                int currCoins = player.TokensCount;
                                byte[] answer = System.Text.Encoding.ASCII.GetBytes("answer B 0 " + currCoins + ' ' + currXP + ' '); // odpowiedź OK
                                player.MenuRequestsStream.Write(answer, 0, answer.Length);
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
                                string token = GenerateUniqueToken();

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
            tokennedPlayers = new List<string>();
            freeTokensLastClear = DateTime.Now;
            if (freeTokensLastClear.Hour > 12)
            {
                TimeSpan ts = new TimeSpan(12, 0, 0);
                freeTokensLastClear = freeTokensLastClear.Date + ts;
            }
            else
            {
                TimeSpan ts = new TimeSpan(0, 0, 0);
                freeTokensLastClear = freeTokensLastClear.Date + ts;
            }
        }
        public static string GenerateUniqueToken()
        {
            bool ok = false;
            string token = "";
            while (!ok)
            {
                ok = true;
                token = GenerateToken();
                if (token == "##&&@@0000" || token == "##&&@@0001" || token == "##&&@@0002" || token == "##&&@@0003")
                {
                    ok = false;
                }
                foreach (string loggedToken in loggedTokens)
                {
                    if (loggedToken == token)
                        ok = false;
                }
            }
            return token;
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
                        // nie usuwamy gracza ze stolika w trakcie trwania gry, musi zostać kopia gracza przy stoliku na potrzeby logiki gry do końca rozdania
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
            if (player.Table != null)
                RemoveFromTable(player);
            player.MenuRequestsTcp.Close();
            player.MenuRequestsStream.Dispose();
            player.GameRequestsTcp.Close();
            player.GameRequestsStream.Dispose();
            loggedClients.Remove(player.Token);
            loggedTokens.RemoveAt(i);
            UpdateBoth(player);
        }

        public static void RemoveFromTable(Player player)
        {
            // TODO dodać, żeby nie usuwało całkowicie gracza ze stolika w trakcie trwania gry (w GameTable sprawdzamy to przez bool isGameActive),
            // tylko zostawiało przy nim dummy-gracza - jego kopię, nie posiadającą NetworkStream'ów od menu i gry
            // (robić kopię gracza i podmieniać go w stoliku na kopię, a oryginał pozostaje zalogowany (jeśli tylko odszedł od stolika, a się nie wylogował),
            // oryginał musi też spasować, bo inaczej w Player.MakeMove() będzie oczekiwanie na jego ruch na kanale wiadomości gameStream).
            // To kopiowanie gracza jest na potrzeby logiki gry, która ma być niezależna od sieciowości i myśleć, że gracze nie mogą odejść od stolika w trakcie gry.
            // Nie powinno przeszkadzać w ponownym dołączeniu tego gracza do tego samego stolika - nie może dołączyć tylko ten sam obiekt gracza,
            // a gracz o takim samym nicku może (według metody GameTable.CheckIfPlayerSitsAtTheTable())
            openTablesAccess.WaitOne();
            if (player.Table != null)
            {
                GameTable tmp = player.Table;
                tmp.Remove(player.Nick);
                player.Table = null;
                if (tmp.GetPlayerCount() == 0)
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
            table.alreadyHasGameThread = true;
            GameplayController controller = new GameplayController(table, new TexasHoldemDealer());
            bool startNextGame = true; // co najmniej 1 rozdanie ma się wykonać, skoro ktoś wysłał zapytanie o włączenie gry
            while (true)
            {
                // zakończ wątek, jeśli nie ma już ludzkich graczy przy tym stoliku
                if (table.GetPlayerCount() < 1)
                {
                    table.alreadyHasGameThread = false;
                    return;
                }

                if (startNextGame)
                {
                    controller.playTheGame();
                    controller.ConcludeGame();
                }

                // TODO dodać usuwanie graczy, którzy byli nieaktywni podczas gry (nie wykonali ruchu przez dany czas, wyszli itd.)
                // TODO dodać też oznaczanie graczy jako niekatywnych podczas gry (limit czasowy na ruch w Plyer.MakeMove())

                // sprawdzaj, czy na kanale od któregoś z graczy pojawiło się zapytanie o włączenie następnego rozdania
                startNextGame = false;
                //for (int i = 0; i < table.Players.Count; i++)
                for (int i = table.Players.Count - 1; i >= 0; i--) // odwrotna iteracja, bo usuwam elementy z listy, po której iteruję w tej pętli 
                {
                    Player p = table.Players[i];
                    try
                    {
                        if (p.GameRequestsStream.DataAvailable)
                        {
                            string message = NetworkHelper.ReadNetworkStream(p.GameRequestsStream);
                            string[] splitted = message.Split(new string(" "));
                            int messageCode = Convert.ToInt32(splitted[0]);
                            if (messageCode == 100) // Kod oznaczający prośbę o następne rozdanie
                            {
                                if(table.GetPlayerCount() > 1)
                                {
                                    startNextGame = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Couldn't read from player's " + p.Nick + " game stream. Assuming network failure and removing player from table.");
                        Console.WriteLine(e);
                        RemoveFromTable(p); // usuwanie gracza, którego połączenie się zerwało
                    }
                }

                // Reset gry wołamy dopiero po odebraniu zapytania o kolejne rozdanie
                // (podczas czekania na kolejne rozdanie mogli dojść nowi gracze (lub opuścić grę), a poprawny reset gry zależy od poprawnej liczby graczy
                // dla określenia poprawnie kolejnej pozycji Dealer'a
                if (startNextGame)
                    controller.ResetGame();
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
                if (attribute.Length <= 9)
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