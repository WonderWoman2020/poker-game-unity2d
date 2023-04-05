using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using pGrServer;
using PokerGameClasses;

namespace ClientTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //SZYBKI PORADNIK
            /*
             * 
             * 
             * juz nie
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
            string[] request = myCompleteMessage.ToString().Split(new char[] { ' ' });
            string token = request[0];
            Console.WriteLine(token);
            ns.Flush();
            bool error = false;

                    
            if (token == "##&&@@0000")
            {
                error = true;
                Console.WriteLine("Server Error");
            }        
            else if (token == "##&&@@0001")
            {
                error = true;
                Console.WriteLine("bad login");
            }
            else if (token == "##&&@@0002")
            {
                error = true;
                Console.WriteLine("bad password");
            }        
            else if (token == "##&&@@0003")
            {
                error = true;
                Console.WriteLine("already logged");
            }
            Console.WriteLine("Press any key to open game connection client on port 6938 and finish logging in procedure...");
            Console.ReadKey();   
            if(!error)
            {

                TcpClient serverGame = new TcpClient();
                serverGame.Connect("127.0.0.1", 6938);

                NetworkStream gameStream = serverGame.GetStream();

                Console.WriteLine("Game connection accepted by server");

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

                    byte[] tosendtables = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "2"); //Poproś o informacje o stolikach
                    ns.Write(tosendtables, 0, tosendtables.Length);
                    //ns.Flush();
                    Thread.Sleep(1000);
                    if (ns.DataAvailable)
                    {
                        byte[] readBuf = new byte[4096];
                        StringBuilder menuRequestStr = new StringBuilder();
                        int nrbyt = ns.Read(readBuf, 0, readBuf.Length);
                        menuRequestStr.AppendFormat("{0}", Encoding.ASCII.GetString(readBuf, 0, nrbyt));
                        string[] tables = menuRequestStr.ToString().Split(new string(":T:")); //na poczatku tez dzieli i wykrywa 1 pusty string 
                        //dlatego tutaj i=1
                        for (int i = 1; i < tables.Length; i++)
                        {
                            string[] mess = tables[i].Split(' ');
                            sb.AppendLine("---Table---");
                            sb.AppendLine("Name       : " + mess[0]);
                            sb.AppendLine("Owner      : " + mess[1]);
                            sb.AppendLine("Human count: " + mess[2]);
                            sb.AppendLine("Bot count  : " + mess[3]);
                            sb.AppendLine("min XP     : " + mess[4]);
                            sb.AppendLine("min tokens : " + mess[5]);
                        }
                    }

                    Console.WriteLine(sb);

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    //odbieranie zapytań z gry od serwera, wysyłanie odpowiedzi od gracza (tak, na razie jest odwrotnie niż powinno być w przyszłości XD)
                    if (gameStream.DataAvailable)
                    {
                        string gameRequest = NetworkHelper.ReadNetworkStream(gameStream);
                        gameStream.Flush();
                        Console.Clear();
                        //Console.WriteLine(gameRequest);

                        string[] splittedRequests = gameRequest.Split(new string(":G:"));

                        foreach(string singleRequest in splittedRequests)
                        {
                            string[] splitted = singleRequest.Split(new string("|"));
                            if (splitted[0] == "Info")
                                Console.WriteLine(splitted[1]);
                            //pobierz ruch od gracza - ruchem jest albo wybranie liczby od 0 do 3
                            //oznaczające kolejno Fold, Check, Raise i AllIn
                            //albo po wykonaniu ruchu Raise, nadejdzie drugi Move request z prośbą o podanie wartości o jaką przebijamy
                            else if (splitted[0] == "Move request")
                            {
                                Console.WriteLine(splitted[0]);
                                Console.WriteLine(splitted[1]);
                                int input = Convert.ToInt32(Console.ReadLine());
                                NetworkHelper.WriteNetworkStream(gameStream, input.ToString());
                            }
                            //która runda się toczy
                            else if(splitted[0] == "Round")
                            {
                                int round = Convert.ToInt32(splitted[1]);
                                Console.WriteLine("------ Time for round nr " + round + " -------\n\n");
                            }
                            // 0 - pusty string, 1 - Name, 2 - wartość Name, 3 - Cards, 4 - wartość Cards (karty)
                            // 5 - Tokens Count, 6 - wartość Tokens Count, 7 - Current Bid, 8 - wartość Current Bid
                            else if(splitted[0] == "Table state")
                            {
                                //Console.WriteLine(splitted[1]);
                                string[] tableState = splitted[1].Split(new string(":"));
                                string name = tableState[2];
                                string cards = tableState[4];
                                CardsCollection cardsCollection = CardsHelper.StringToCardsCollection(cards);
                                int tokensInGame = Convert.ToInt32(tableState[6]);
                                int currentBid = Convert.ToInt32(tableState[8]);
                                Console.WriteLine("Table's '" + name+ "' game state:" + "\nCards: " + cardsCollection + "\nTokens in game: " + tokensInGame + "\nCurrent Bid: " + currentBid+"\n");
                            }
                            //podobnie jak w 'Table state'
                            else if(splitted[0] == "Player state")
                            {
                                //Console.WriteLine(splitted[1]);
                                string[] playerState = splitted[1].Split(new string(":"));
                                string nick = playerState[2];
                                string hand = playerState[4];
                                CardsCollection cardsCollection = CardsHelper.StringToCardsCollection(hand);
                                int tokensCount = Convert.ToInt32(playerState[6]);
                                int currentBet = Convert.ToInt32(playerState[8]);
                                int xp = Convert.ToInt32(playerState[10]);
                                Console.WriteLine("Player's '" + nick + "' game state:" + "\nHand: " + cardsCollection + "\nTokens: " + tokensCount + "\nCurrent Bet: " + currentBet + "\nXP: "+xp+"\n");
                            }
                            //którego gracza teraz kolej
                            else if(splitted[0] == "Which player turn")
                            {
                                string nickOfThePlayer = splitted[1];
                                Console.WriteLine("Player's '" + nickOfThePlayer + "' move: ");
                            }
                            //inne jeszcze nie zdefiniowane wiadomości, o których zapomniałam XD jeśli takie jeszcze są
                            else
                                if(splitted[0] != "")
                                    Console.WriteLine("Undefined message from server: " + singleRequest);
                        }
                        
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (Console.KeyAvailable)
                    {
                        cki = Console.ReadKey();
                        if (cki.Key == ConsoleKey.Escape)
                        {
                            running = false;
                        }
                        if (cki.Key == ConsoleKey.A) //Dodaj nowy stolik
                        {
                            byte[] tosend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "0" + ' ' + login + nr.ToString() + ' ' + "1" + ' ' + "3" + ' ' + "0" + ' ' + "16" + ' ');
                            ns.Write(tosend, 0, tosend.Length);
                            nr++;

                        }
                        if (cki.Key == ConsoleKey.J) //Dołącz do stolika
                        {
                            Console.WriteLine("Enter table name");
                            string tableName = Console.ReadLine();
                            byte[] tosend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "1" + ' ' + tableName + ' ');
                            ns.Write(tosend, 0, tosend.Length);
                        }
                        if (cki.Key == ConsoleKey.O) //Odejdź od stolika
                        {

                            byte[] tosend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "4" + ' ');
                            ns.Write(tosend, 0, tosend.Length);

                        }
                        if (cki.Key == ConsoleKey.C) //Zmień ustawienia stolika
                        {
                            byte[] tosend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "5" + ' ' + "1" + ' ' + "3" + ' ' + "20" + ' ' + "55" + ' ');
                            ns.Write(tosend, 0, tosend.Length);
                        }
                        if (cki.Key == ConsoleKey.P) //Uruchom grę
                        {
                            byte[] tosend = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "6" + ' ');
                            ns.Write(tosend, 0, tosend.Length);
                        }
                        if (cki.Key == ConsoleKey.R) //Wyczyść konsolę
                        {
                            Console.Clear();
                        }
                    }
                    Console.WriteLine(emptySpace);
                }
                byte[] tose = System.Text.Encoding.ASCII.GetBytes(token + ' ' + "3"); //Wyloguj się
                ns.Write(tose, 0, tose.Length);
                Thread.Sleep(1000);
                ns.Flush();
                serverGame.Close();
                server.Close();
                gameStream.Dispose();
            }
            
        }
    }
}
