using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using pGrServer;

namespace PokerGameClasses
{
    public class GameplayController
    {
        private GameTable gameTable
        { get; set; }
        private ICardsDealer Dealer
        {  get;  set; }

        private int CurrentRound
        { get; set; }

        private int PositionOfPlayerWhoRaised
        { get; set; }
        public GameplayController(GameTable gameTable, ICardsDealer cardsDealer)
        {
            this.gameTable = gameTable;
            this.Dealer = cardsDealer;
            this.CurrentRound = 0;
            this.PositionOfPlayerWhoRaised = -1;
        }

        public void playTheGame()
        {
            //this.ResetGame();
            Console.WriteLine("Next hand is on");
            this.gameTable.isGameActive = true;
            this.gameTable.SortPlayersBySeats();

            this.SendIsGameOnStatus("Game started");

            this.MakeBlindsFirstMoves();

            while (CurrentRound < 4)
            {
                this.MakeNextRound();
                if(CurrentRound < 4)
                    this.PositionOfPlayerWhoRaised = -1;
            }

            this.SendIsGameOnStatus("Game ended");
        }

        public void SendIsGameOnStatus(string status)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(":G:");
            sb.Append("Status");
            sb.Append("|");
            sb.Append(status);
            foreach (Player p in this.gameTable.Players)
            {
                NetworkHelper.WriteNetworkStream(p.GameRequestsStream, sb.ToString());
                p.GameRequestsStream.Flush();
            }
        }

        private void MakeBlindsFirstMoves()
        {
            Player smallBlind = this.gameTable.Players[this.GetSmallBlindPosition()];
            Player bigBlind = this.gameTable.Players[this.GetBigBlindPosition()];

            StringBuilder sb = new StringBuilder();
            sb.Append(":G:");
            sb.Append("Info");
            sb.Append("|");
            sb.AppendLine("Player's '" + smallBlind.Nick + "' move (small blind):");

            bool moveDoneS = smallBlind.SmallBlindFirstMove();
            if (moveDoneS)
            {
                this.PositionOfPlayerWhoRaised = this.GetSmallBlindPosition();
                this.gameTable.CurrentBid = smallBlind.PlayersCurrentBet;
            }

            sb.AppendLine("Small blind set current bid to " + this.gameTable.CurrentBid + " tokens.\n");

            foreach(Player p in this.gameTable.Players)
            {
                NetworkHelper.WriteNetworkStream(p.GameRequestsStream, sb.ToString());
                p.GameRequestsStream.Flush();
            }

            sb.Clear();

            sb.Append(":G:");
            sb.Append("Info");
            sb.Append("|");
            sb.AppendLine("Player's '" + bigBlind.Nick + "' move (big blind):");
            
            bool moveDoneB = bigBlind.BigBlindFirstMove();
            if (moveDoneB)
            {
                this.PositionOfPlayerWhoRaised = this.GetBigBlindPosition();
                this.gameTable.CurrentBid = bigBlind.PlayersCurrentBet;
            }

            sb.AppendLine("Big blind set current bid to " + this.gameTable.CurrentBid + " tokens.\n");

            foreach (Player p in this.gameTable.Players)
            {
                NetworkHelper.WriteNetworkStream(p.GameRequestsStream, sb.ToString());
                p.GameRequestsStream.Flush();
            }
        }

        private int GetSmallBlindPosition()
        {
            return this.GetPositionOfPlayerOffBy(this.Dealer.Position, 1);
        }

        private int GetBigBlindPosition()
        {
            return this.GetPositionOfPlayerOffBy(this.Dealer.Position, 2);
        }

        private int GetPositionOfPlayerOffBy(int basePlayerPosition, int otherPlayerRelativePosition)
        {
            return (basePlayerPosition + otherPlayerRelativePosition) % this.gameTable.Players.Count;
        }
        // ze stolika
        private void MakeTurn(int startingPlayerNr, int roundParticipantsNr)
        {
            Console.WriteLine("It's round nr " + this.CurrentRound);
            bool equalBets = false;
            while (!equalBets)
            {
                for (int i = 0; i < roundParticipantsNr; i++)
                {
                    int currentPlayer = (startingPlayerNr + i) % this.gameTable.Players.Count;
                    Console.WriteLine("It's player's " + currentPlayer + " turn. (" + this.gameTable.Players[currentPlayer].Nick + ", seat: "+ this.gameTable.Players[currentPlayer].SeatNr+")");
                    Console.WriteLine("Position of player who raised last is: " + this.PositionOfPlayerWhoRaised);
                    if (this.PositionOfPlayerWhoRaised == currentPlayer) // koiec tury, wróciliœmy do ostatniego gracza, który przebi³
                    {
                        equalBets = true;
                        break;
                    }

                    Player player = this.gameTable.Players[currentPlayer];

                    if (player.AllInMade || player.Folded) //Ci gracze ju¿ nie maj¹ ruchów
                        continue;

                    //rozsy³anie stanu gry do wszystkich graczy przed ka¿dym ruchem
                    foreach (Player p in this.gameTable.Players)
                    {
                        Console.WriteLine("Trying to send message to player '" + p.Nick + "'");
                        NetworkHelper.WriteNetworkStream(p.GameRequestsStream, this.MessageGameState(player, p, false));
                        //Console.WriteLine("Message sent to " + p.Nick + " was:\n" + this.MessageGameState(player, p, false));
                    }

                    bool moveDone = player.MakeMove();
                    if (!moveDone)
                        continue;

                    //ostatni gracz, który przebi³ stawkê lub pierwszy, który czeka³ (jeœli nikt nie przebija³), jest kandydatem do sprawdzania
                    if (this.PositionOfPlayerWhoRaised == -1 || player.PlayersCurrentBet > this.gameTable.CurrentBid)
                    {
                        this.gameTable.CurrentBid = player.PlayersCurrentBet;
                        this.PositionOfPlayerWhoRaised = currentPlayer;//player.SeatNr; // numer miejsca przy stoliku jest tylko na potrzeby ³adnego wyœwietlania gry w Unity
                    }
                }
                if (this.CheckIfAllFolded())
                    break;
            }
        }

        private bool CheckIfAllFolded()
        {
            bool folded = true;
            foreach(Player p in this.gameTable.Players)
            {
                if (!p.Folded)
                    folded = false;
            }
            return folded;
        }
        private void MakeNextRound()
        {
            switch(this.CurrentRound)
            {
                case 0:
                    this.PreFlopRound();
                    break;
                case 1:
                    this.FlopRound();
                    break;
                case 2:
                    this.TurnRound();
                    break;
                case 3:
                    this.RiverRound();
                    break;
                default:
                    break;
            }
            this.CurrentRound++;
        }

        private void PreFlopRound()
        {
            this.Dealer.DealCards(this.gameTable, 0);
            this.MakeTurn(this.GetPositionOfPlayerOffBy(this.GetBigBlindPosition(), 1), this.gameTable.Players.Count);
        }

        private void FlopRound()
        {
            this.Dealer.DealCards(gameTable, 1);
            this.MakeTurn(this.GetSmallBlindPosition(), this.gameTable.Players.Count);
        }

        private void TurnRound()
        {
            this.Dealer.DealCards(gameTable, 2);
            this.MakeTurn(this.GetSmallBlindPosition(), this.gameTable.Players.Count);
        }

        private void RiverRound()
        {
            this.Dealer.DealCards(gameTable, 3);
            this.MakeTurn(this.GetSmallBlindPosition(), this.gameTable.Players.Count);
        }

        public void ConcludeGame()
        {
            List<Player> winners = determineWinner();
            //if (winners != null)
            //{
            //    int countOfWinners = winners.Count;
            //    int totalCoins = this.gameTable.TokensInGame;
            //    foreach (Player player in winners)
            //    {
            //        player.TokensCount += totalCoins / countOfWinners;
            //        player.XP += 100 / player.TokensCount;
            //    }
            //}
            Player winner = winners[0];
            if (winner != null)
            {
                winner.TokensCount = winner.TokensCount + this.gameTable.TokensInGame;
                winner.XP = winner.XP + 100; // ew. do zmiany
            }

            string winnerNick = null;
            if (winner != null)
                winnerNick = winner.Nick;

            StringBuilder sb = new StringBuilder();
            sb.Append(":G:");
            //sb.Append("Info");
            sb.Append("Winner");
            sb.Append("|");
            //sb.AppendLine("And the winner is:\n" + winnerNick + "\nCongrats!");
            sb.Append(winnerNick);

            //rozsy³anie do ka¿dego gracza informacji kto wygra³ i jeszcze raz stanu gry na koniec
            foreach (Player p in this.gameTable.Players)
            {
                NetworkHelper.WriteNetworkStream(p.GameRequestsStream, this.MessageGameState(null, p, true) + sb.ToString());
                p.GameRequestsStream.Flush();
                p.ResetPlayerGameState();
            }

            //this.ResetGame(); // Reset gry bêdzie odt¹d PRZED gr¹ (przy starcie gry), ¿eby u¿ywaæ zaktualizowanej miêdzy rozdaniami listy graczy
            this.gameTable.isGameActive = false;
        }
        public List<Player> determineWinner()
        {
            HandsComparer handsComparer = new HandsComparer();
            int biggestScore = 11;//gorzej niz najwyzsza karta
            List<Player> winners = (this.gameTable.Players.Count > 0) ? new List<Player> { this.gameTable.Players[0] } : null;
            foreach (Player player in gameTable.Players)
            {
                if (player.Folded)
                    continue;
                // Karty gracza i 5 wspolnych lezacych na stole
                CardsCollection PlayerCards = player.PlayerHand + gameTable.shownHelpingCards;
                PlayerCards.SortDesc();
                PlayerCards.SortDesc();
                Console.WriteLine(PlayerCards);
                int playerScore = handsComparer.valueOfCards(PlayerCards);
                if (playerScore < biggestScore)
                {
                    //wygrana
                    winners.Clear();
                    winners.Add(player);
                    biggestScore = playerScore;
                }
                else if (playerScore == biggestScore)
                {
                    CardsCollection ActualWinnerCards = winners[0].PlayerHand + gameTable.shownHelpingCards;
                
                    if (playerScore == 1)//royal poker nie wymaga sprawdzenia, tylko jeden jest
                    {
                        winners.Add(player);
                    }
                    else if (playerScore == 2)//poker sprawdzic wyzsza karte
                    {
                        PlayerCards.SortDesc();
                        ActualWinnerCards.SortDesc();
                        Card highestPokerCardActual = handsComparer.HighestCardOfPoker(ActualWinnerCards);
                        Card highestPokerCard = handsComparer.HighestCardOfPoker(PlayerCards);
                        if (highestPokerCard.Value > highestPokerCardActual.Value)
                        {
                            //wygrana
                            winners.Clear();
                            winners.Add(player);
                            biggestScore = playerScore;
                        }
                        else if (highestPokerCard.Value < highestPokerCardActual.Value)
                        {
                            //przegrana
                            continue;
                        }
                        else
                        {
                            //TODO remis
                            winners.Add(player);
                        }
                    }
                    else if (playerScore == 3)//kareta sprawdzic wyzsza karete a potem wyzsza karte
                    {
                        Card QuadsCardActual = handsComparer.CardOfQuads(ActualWinnerCards);
                        Card QuadsCard = handsComparer.CardOfQuads(PlayerCards);
                        if (QuadsCard.Value > QuadsCardActual.Value)
                        {
                            //wygrana
                            winners.Clear();
                            winners.Add(player);
                            biggestScore = playerScore;
                        }
                        else if (QuadsCard.Value == QuadsCardActual.Value)
                        {
                            //sprawdzam 5 karte kto ma wyzsza
                            CardsCollection copyofActualWinnerCards = new CardsCollection(new List<Card>(ActualWinnerCards.Cards));//ActualWinnerCards.Cards);
                            CardsCollection copyOfPlayerCards = new CardsCollection(new List<Card>(PlayerCards.Cards));
                            foreach (Card card in copyofActualWinnerCards.Cards.ToList())
                            {
                                if (card.Value == QuadsCard.Value)
                                {
                                    copyofActualWinnerCards.Cards.Remove(card);//TakeOutCard(card.Sign, card.Value);//!!!!
                                }
                            }
                            foreach (Card card in copyOfPlayerCards.Cards.ToList())
                            {
                                if (card.Value == QuadsCardActual.Value)
                                {
                                    copyOfPlayerCards.Cards.Remove(card);
                                }
                            }
                            copyofActualWinnerCards.SortDesc();
                            copyOfPlayerCards.SortDesc();
                            if (copyOfPlayerCards.Cards[0].Value > copyofActualWinnerCards.Cards[0].Value)
                            {
                                //wygrana
                                winners.Clear();
                                winners.Add(player);
                                biggestScore = playerScore;
                            }
                            else if (copyOfPlayerCards.Cards[0].Value < copyofActualWinnerCards.Cards[0].Value)
                            {
                                //przegrywa
                                continue;
                            }
                            else
                            {
                                //remis TODO
                                winners.Add(player);
                            }
                        }
                    }
                    else if (playerScore == 4)//full sprawdzic wieksza trojke, jak rowne to mniejsza
                    {
                        //tworze kopie kart gracza i najwyzszego wyniku
                        CardsCollection copyofActualWinnerCards = new CardsCollection(new List<Card>(ActualWinnerCards.Cards));
                        CardsCollection copyOfPlayerCards = new CardsCollection(new List<Card>(PlayerCards.Cards));
                        //sortuje
                        copyofActualWinnerCards.SortDesc();
                        copyOfPlayerCards.SortDesc();
                        //sprawdz trojke od gory, zapisuje jaka karta i usuwam z kolekcji
                        CardValue cardValueofThreePlayer = handsComparer.GiveCardOfTree(copyOfPlayerCards).Value;
                        CardValue cardValueofThreeActualWinner = handsComparer.GiveCardOfTree(copyofActualWinnerCards).Value;
                        if (cardValueofThreePlayer > cardValueofThreeActualWinner)
                        {
                            //wygrana
                            winners.Clear();
                            winners.Add(player);
                            biggestScore = playerScore;
                        }
                        else if (cardValueofThreePlayer < cardValueofThreeActualWinner)
                        {
                            //przegrywa
                            continue;
                        }
                        else
                        {
                            //usuwam 3
                            foreach (Card card in copyofActualWinnerCards.Cards.ToList())
                            {
                                if (card.Value == cardValueofThreePlayer)
                                {
                                    copyofActualWinnerCards.Cards.Remove(card);
                                }
                            }
                            foreach (Card card in copyOfPlayerCards.Cards.ToList())
                            {
                                if (card.Value == cardValueofThreeActualWinner)
                                {
                                    copyOfPlayerCards.Cards.Remove(card);
                                }
                            }
                            //sortuje
                            copyofActualWinnerCards.SortDesc();
                            copyOfPlayerCards.SortDesc();
                            //sprawdzam dwojki, zapisuje jaka karta
                            CardValue cardValueofTwoPlayer = handsComparer.GiveCardOfTwo(copyOfPlayerCards).Value;
                            CardValue cardValueofTwoActualWinner = handsComparer.GiveCardOfTwo(copyofActualWinnerCards).Value;
                            if (cardValueofTwoPlayer > cardValueofTwoActualWinner)
                            {
                                //wygrana
                                winners.Clear();
                                winners.Add(player);
                                biggestScore = playerScore;
                            }
                            else if (cardValueofTwoPlayer < cardValueofTwoActualWinner)
                            {
                                //przegrywa
                                continue;
                            }
                            else
                            {
                                //remis TODO
                                winners.Add(player);
                            }
                        }
                    }
                    else if (playerScore == 5)//kolor sprawdzic na jakiej karcie siedzi kolor 
                    {
                        //tworze kopie kart gracza i najwyzszego wyniku
                        CardsCollection copyofActualWinnerCards = new CardsCollection(new List<Card>(ActualWinnerCards.Cards));
                        CardsCollection copyOfPlayerCards = new CardsCollection(new List<Card>(PlayerCards.Cards));
                        int kier = 0, karo = 0, pik = 0, trefl = 0;
                        foreach (Card card in copyOfPlayerCards.Cards)
                        {
                            if (card.Sign == CardSign.Heart)
                                kier++;
                            else if (card.Sign == CardSign.Spade)
                                pik++;
                            else if (card.Sign == CardSign.Diamond)
                                karo++;
                            else if (card.Sign == CardSign.Club)
                                trefl++;
                        }
                        CardSign cardSignOfColour;
                        if (kier >= 5)
                            cardSignOfColour = CardSign.Heart;
                        else if (pik >= 5)
                            cardSignOfColour = CardSign.Spade;
                        else if (karo >= 5)
                            cardSignOfColour = CardSign.Diamond;
                        else
                            cardSignOfColour = CardSign.Club;

                        foreach (Card card in copyOfPlayerCards.Cards.ToList())
                            if (card.Sign != cardSignOfColour)
                                copyOfPlayerCards.Cards.Remove(card);

                        foreach (Card card in copyofActualWinnerCards.Cards.ToList())
                            if (card.Sign != cardSignOfColour)
                                copyofActualWinnerCards.Cards.Remove(card);

                        copyofActualWinnerCards.SortDesc();
                        copyOfPlayerCards.SortDesc();

                        for (int i = 0; i < 5; i++)
                        {
                            if (copyOfPlayerCards.Cards[i].Value > copyofActualWinnerCards.Cards[i].Value)
                            {
                                //wygrana
                                winners.Clear();
                                winners.Add(player);
                                biggestScore = playerScore;
                                break;
                            }
                            else if (copyOfPlayerCards.Cards[i].Value < copyofActualWinnerCards.Cards[i].Value)
                            {
                                //przegrana
                                break;
                            }
                            else if (i == 4 && copyOfPlayerCards.Cards[i].Value == copyofActualWinnerCards.Cards[i].Value)
                            {
                                //remis TODO
                                winners.Add(player);
                                break;
                            }
                        }
                    }
                    else if (playerScore == 6)//strit sprawdzic na jakiej karcie siedzi strit
                    {
                        ActualWinnerCards.SortDesc();
                        PlayerCards.SortDesc();
                        Card highestStraightCardActual = handsComparer.HighestCardOfStraigth(ActualWinnerCards);
                        Card highestStraightCardPlayer = handsComparer.HighestCardOfStraigth(PlayerCards);
                        if ((int)highestStraightCardPlayer.Value > (int)highestStraightCardActual.Value)
                        {
                            //wygrana
                            winners.Clear();
                            winners.Add(player);
                            biggestScore = playerScore;
                        }
                        else if ((int)highestStraightCardPlayer.Value < (int)highestStraightCardActual.Value)
                        {
                            //przegrana
                            continue;
                        }
                        else
                        {
                            //TODO remis
                            winners.Add(player);
                        }
                    }
                    else if (playerScore == 7)//trojka sprawdzic na jakiej karcie siedzi trojka, a potem najwyzsza karte
                    {
                        //tworze kopie kart gracza i najwyzszego wyniku
                        CardsCollection copyofActualWinnerCards = new CardsCollection(new List<Card>(ActualWinnerCards.Cards));
                        CardsCollection copyOfPlayerCards = new CardsCollection(new List<Card>(PlayerCards.Cards));
                        //sortuje
                        copyofActualWinnerCards.SortDesc();
                        copyOfPlayerCards.SortDesc();
                        //sprawdz trojke od gory, zapisuje jaka karta i usuwam z kolekcji
                        CardValue cardValueofThreePlayer = handsComparer.GiveCardOfTree(copyOfPlayerCards).Value;
                        CardValue cardValueofThreeActualWinner = handsComparer.GiveCardOfTree(copyofActualWinnerCards).Value;
                        if (cardValueofThreePlayer > cardValueofThreeActualWinner)
                        {
                            //wygrana
                            winners.Clear();
                            winners.Add(player);
                            biggestScore = playerScore;
                        }
                        else if (cardValueofThreePlayer < cardValueofThreeActualWinner)
                        {
                            //przegrywa
                            continue;
                        }
                        else
                        {
                            //trojki te same
                            foreach (Card card in copyOfPlayerCards.Cards.ToList())
                            {
                                if (card.Value == cardValueofThreePlayer)
                                {
                                    copyOfPlayerCards.Cards.Remove(card);
                                }
                            }
                            foreach (Card card in copyofActualWinnerCards.Cards.ToList())
                            {
                                if (card.Value == cardValueofThreeActualWinner)
                                {
                                    copyofActualWinnerCards.Cards.Remove(card);
                                }
                            }
                            copyofActualWinnerCards.SortDesc();
                            copyOfPlayerCards.SortDesc();
                            if (copyOfPlayerCards.Cards[0].Value > copyofActualWinnerCards.Cards[0].Value)
                            {
                                //wygrywa
                                winners.Clear();
                                winners.Add(player);
                                biggestScore = playerScore;
                            }
                            else if (copyOfPlayerCards.Cards[0].Value < copyofActualWinnerCards.Cards[0].Value)
                            {
                                //przegrywa
                                continue;
                            }
                            else
                            {
                                if (copyOfPlayerCards.Cards[1].Value > copyofActualWinnerCards.Cards[1].Value)
                                {
                                    //wygrywa
                                    winners.Clear();
                                    winners.Add(player);
                                    biggestScore = playerScore;
                                }
                                else if (copyOfPlayerCards.Cards[1].Value < copyofActualWinnerCards.Cards[1].Value)
                                {
                                    //przegrywa
                                    continue;
                                }
                                else
                                {
                                    //remis TODO
                                    winners.Add(player);
                                }
                            }
                        }
                    }
                    else if (playerScore == 8)//2 pary sprawdzic wyzsza pare, potem mniejsza, potem
                    {
                        //tworze kopie kart gracza i najwyzszego wyniku
                        CardsCollection copyofActualWinnerCards = new CardsCollection(new List<Card>(ActualWinnerCards.Cards));
                        CardsCollection copyOfPlayerCards = new CardsCollection(new List<Card>(PlayerCards.Cards));
                        //sortuje
                        copyofActualWinnerCards.SortDesc();
                        copyOfPlayerCards.SortDesc();
                        CardValue cardValueOfFirstTwoPlayer = handsComparer.GiveCardOfTwo(copyOfPlayerCards).Value;
                        CardValue cardValueOfFirstTwoActualWinner = handsComparer.GiveCardOfTwo(copyofActualWinnerCards).Value;
                        if (cardValueOfFirstTwoPlayer > cardValueOfFirstTwoActualWinner)
                        {
                            //wygrana
                            winners.Clear();
                            winners.Add(player);
                            biggestScore = playerScore;
                        }
                        else if (cardValueOfFirstTwoPlayer < cardValueOfFirstTwoActualWinner)
                        {
                            //przegrana
                            continue;
                        }
                        else
                        {
                            foreach (Card card in copyOfPlayerCards.Cards.ToList())
                            {
                                if (card.Value == cardValueOfFirstTwoPlayer)
                                    copyOfPlayerCards.Cards.Remove(card);
                            }
                            foreach (Card card in copyofActualWinnerCards.Cards.ToList())
                            {
                                if (card.Value == cardValueOfFirstTwoActualWinner)
                                    copyofActualWinnerCards.Cards.Remove(card);
                            }
                            copyofActualWinnerCards.SortDesc();
                            copyOfPlayerCards.SortDesc();
                            CardValue cardValueOfSecondTwoPlayer = handsComparer.GiveCardOfTwo(copyOfPlayerCards).Value;
                            CardValue cardValueOfSecondTwoActualWinner = handsComparer.GiveCardOfTwo(copyofActualWinnerCards).Value;
                            if (cardValueOfSecondTwoPlayer > cardValueOfSecondTwoActualWinner)
                            {
                                //wygrana
                                winners.Clear();
                                winners.Add(player);
                                biggestScore = playerScore;
                            }
                            else if (cardValueOfSecondTwoPlayer < cardValueOfSecondTwoActualWinner)
                            {
                                //przegrana
                                continue;
                            }
                            else
                            {//sprawdzam 5-ta karte
                                foreach (Card card in copyOfPlayerCards.Cards.ToList())
                                {
                                    if (card.Value == cardValueOfSecondTwoPlayer)
                                        copyOfPlayerCards.Cards.Remove(card);
                                }
                                foreach (Card card in copyofActualWinnerCards.Cards.ToList())
                                {
                                    if (card.Value == cardValueOfSecondTwoActualWinner)
                                        copyofActualWinnerCards.Cards.Remove(card);
                                }
                                copyofActualWinnerCards.SortDesc();
                                copyOfPlayerCards.SortDesc();
                                if (copyOfPlayerCards.Cards[0].Value > copyofActualWinnerCards.Cards[0].Value)
                                {
                                    //wygrana
                                    winners.Clear();
                                    winners.Add(player);
                                    biggestScore = playerScore;
                                }
                                else if (copyOfPlayerCards.Cards[0].Value < copyofActualWinnerCards.Cards[0].Value)
                                {
                                    //przegrana
                                    continue;
                                }
                                else
                                {
                                    //remis TODO
                                    winners.Add(player);
                                }
                            }
                        }
                    }
                    else if (playerScore == 9)//1 para sprawdzic na jakiej karcie siedzi 2, a potem najwyzsza karta
                    {
                        //tworze kopie kart gracza i najwyzszego wyniku
                        CardsCollection copyofActualWinnerCards = new CardsCollection(new List<Card>(ActualWinnerCards.Cards));
                        CardsCollection copyOfPlayerCards = new CardsCollection(new List<Card>(PlayerCards.Cards));
                        //sortuje
                        copyofActualWinnerCards.SortDesc();
                        copyOfPlayerCards.SortDesc();
                        CardValue cardValueOfTwoPlayer = handsComparer.GiveCardOfTwo(copyOfPlayerCards).Value;
                        CardValue cardValueOfTwoActualWinner = handsComparer.GiveCardOfTwo(copyofActualWinnerCards).Value;
                        if ((int)cardValueOfTwoPlayer > (int)cardValueOfTwoActualWinner)
                        {
                            //wygrana
                            winners.Clear();
                            winners.Add(player);
                            biggestScore = playerScore;
                        }
                        else if ((int)cardValueOfTwoPlayer < (int)cardValueOfTwoActualWinner)
                        {
                            //przegrana
                            continue;
                        }
                        else
                        {
                            foreach (Card card in copyOfPlayerCards.Cards.ToList())
                            {
                                if (card.Value == cardValueOfTwoPlayer)
                                    copyOfPlayerCards.Cards.Remove(card);
                            }
                            foreach (Card card in copyofActualWinnerCards.Cards.ToList())
                            {
                                if (card.Value == cardValueOfTwoActualWinner)
                                    copyofActualWinnerCards.Cards.Remove(card);
                            }
                            copyofActualWinnerCards.SortDesc();
                            copyOfPlayerCards.SortDesc();
                            for (int i = 0; i < 3; i++)
                            {
                                if (copyOfPlayerCards.Cards[i].Value > copyofActualWinnerCards.Cards[i].Value)
                                {
                                    //wygrana
                                    winners.Clear();
                                    winners.Add(player);
                                    biggestScore = playerScore;
                                    break;
                                }
                                else if (copyOfPlayerCards.Cards[i].Value < copyofActualWinnerCards.Cards[i].Value)
                                {
                                    //przegrana
                                    break;
                                }
                                else if (copyOfPlayerCards.Cards[i].Value == copyofActualWinnerCards.Cards[i].Value && i == 2)
                                {
                                    //remis
                                    winners.Add(player);
                                    break;
                                }
                            }

                        }
                    }
                    else if (playerScore == 10)//najwyzsza karta
                    {
                        ActualWinnerCards.SortDesc();
                        //po kolei sprawdzam tych 5 posortowanych karty
                        for (int i = 0; i < 5; i++)
                        {
                            if (PlayerCards.Cards[i].Value > ActualWinnerCards.Cards[i].Value)
                            {
                                //wygrana
                                winners.Clear();
                                winners.Add(player);
                                biggestScore = playerScore;
                                break;
                            }
                            else if (PlayerCards.Cards[i].Value < ActualWinnerCards.Cards[i].Value)
                            {
                                //przegrana
                                break;
                            }
                            else if (PlayerCards.Cards[i].Value == ActualWinnerCards.Cards[i].Value && i == 4)
                            {
                                //remis
                                winners.Add(player);
                                break;
                            }
                        }
                    }
                }
            }

            return winners;
        }

        public void ResetGame()
        {
            this.Dealer.TakeBackCards(this.gameTable);
            this.Dealer.ChangePosition(this.gameTable);
            this.gameTable.ResetGameState();
            this.gameTable.isGameActive = false;
            this.PositionOfPlayerWhoRaised = -1;
            this.CurrentRound = 0;
        }

        private string MessageGameState(Player currentPlayer, Player fromWhichPerspective, bool allPlayersCards)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(":G:");
            sb.Append("Round");
            sb.Append("|");
            sb.Append(this.CurrentRound);

            sb.Append(":G:");
            sb.Append("Table state");
            sb.Append("|");
            sb.Append(this.gameTable.MessageGameState());

            foreach (Player p in this.gameTable.Players)
            {
                sb.Append(":G:");
                sb.Append("Player state");
                sb.Append("|");
                if(p.Nick == fromWhichPerspective.Nick) // Uwaga! Za³o¿enie, ¿e nick gracza jest indywidualny
                    sb.Append(p.MessageGameState(true));
                else if(allPlayersCards)
                    sb.Append(p.MessageGameState(true));
                else
                    sb.Append(p.MessageGameState(false));
            }

            if (currentPlayer != null)
            {
                sb.Append(":G:");
                sb.Append("Which player turn");
                sb.Append("|");
                sb.Append(currentPlayer.Nick);
            }

            return sb.ToString();
        }
    }
}
