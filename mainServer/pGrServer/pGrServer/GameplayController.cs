using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    public class GameplayController
    {
        public GameTable gameTable
        { get; set; }
        public ICardsDealer Dealer
        { get; set; }

        public int CurrentRound
        { get; set; }

        public int PositionOfPlayerWhoRaised
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
            Console.WriteLine("\nDo you want to start the game? (Just press enter, it will start either way XD)\n");
            Console.ReadKey();
            Console.Clear();

            this.gameTable.SortPlayersBySeats();
            this.gameTable.Settings.changeMinTokens(10);

            this.MakeBlindsFirstMoves();

            while (CurrentRound < 4)
            {
                this.MakeNextRound();
                if(CurrentRound < 4)
                    this.PositionOfPlayerWhoRaised = -1;
            }
        }

        private void MakeBlindsFirstMoves()
        {
            Player smallBlind = this.gameTable.Players[this.GetSmallBlindPosition()];
            Player bigBlind = this.gameTable.Players[this.GetBigBlindPosition()];

            Console.WriteLine("Player's '" + smallBlind.Nick + "' move (small blind):");
            bool moveDoneS = smallBlind.SmallBlindFirstMove();
            if (moveDoneS)
            {
                this.PositionOfPlayerWhoRaised = this.GetSmallBlindPosition();
                this.gameTable.CurrentBid = smallBlind.PlayersCurrentBet;
            }
            Console.WriteLine("Small blind set current bid to " + this.gameTable.CurrentBid + " tokens.\n");
            Console.ReadKey();

            Console.WriteLine("Player's '" + bigBlind.Nick + "' move (big blind):");
            bool moveDoneB = bigBlind.BigBlindFirstMove();
            if (moveDoneB)
            {
                this.PositionOfPlayerWhoRaised = this.GetBigBlindPosition();
                this.gameTable.CurrentBid = bigBlind.PlayersCurrentBet;
            }
            Console.WriteLine("Big blind set current bid to " + this.gameTable.CurrentBid + " tokens.\n");
            Console.ReadKey();
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
        public void MakeTurn(int startingPlayerNr, int roundParticipantsNr)
        {
            bool equalBets = false;
            while (!equalBets)
            {
                for (int i = 0; i < roundParticipantsNr; i++)
                {
                    int currentPlayer = (startingPlayerNr + i) % this.gameTable.Players.Count;
                    if (this.PositionOfPlayerWhoRaised == currentPlayer) // koiec tury, wróciliœmy do ostatniego gracza, który przebi³
                    {
                        equalBets = true;
                        break;
                    }

                    Player player = this.gameTable.Players[currentPlayer];

                    if (player.AllInMade || player.Folded) //Ci gracze ju¿ nie maj¹ ruchów
                        continue;

                    Console.Clear();
                    Console.WriteLine("------ Time for round nr " + this.CurrentRound + " -------\n\n");
                    Console.WriteLine(this.gameTable.TableGameState() + "\n");
                    Console.WriteLine(player.PlayerGameState() + "\n");
                    Console.WriteLine("Player's '" + player.Nick + "' move: ");
                    bool moveDone = player.MakeMove();
                    if (!moveDone)
                        continue;

                    //ostatni gracz, który przebi³ stawkê lub pierwszy, który czeka³ (jeœli nikt nie przebija³), jest kandydatem do sprawdzania
                    if (this.PositionOfPlayerWhoRaised == -1 || player.PlayersCurrentBet > this.gameTable.CurrentBid)
                    {
                        this.gameTable.CurrentBid = player.PlayersCurrentBet;
                        this.PositionOfPlayerWhoRaised = player.SeatNr;
                    }
                }
                if (this.CheckIfAllFolded())
                    break;
            }
        }

        public bool CheckIfAllFolded()
        {
            bool folded = true;
            foreach(Player p in this.gameTable.Players)
            {
                if (!p.Folded)
                    folded = false;
            }
            return folded;
        }
        public void MakeNextRound()
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

        public void PreFlopRound()
        {
            this.Dealer.DealCards(this.gameTable, 0);
            this.MakeTurn(this.GetPositionOfPlayerOffBy(this.GetBigBlindPosition(), 1), this.gameTable.Players.Count);
        }

        public void FlopRound()
        {
            this.Dealer.DealCards(gameTable, 1);
            this.MakeTurn(this.GetSmallBlindPosition(), this.gameTable.Players.Count);
        }

        public void TurnRound()
        {
            this.Dealer.DealCards(gameTable, 2);
            this.MakeTurn(this.GetSmallBlindPosition(), this.gameTable.Players.Count);
        }

        public void RiverRound()
        {
            this.Dealer.DealCards(gameTable, 3);
            this.MakeTurn(this.GetSmallBlindPosition(), this.gameTable.Players.Count);
        }

        public void ConcludeGame()
        {
            Player winner = determineWinner();
            if (winner != null)
            {
                winner.TokensCount = winner.TokensCount + this.gameTable.TokensInGame;
                winner.XP = winner.XP + 100; // ew. do zmiany
            }
            Console.WriteLine("\nAnd the winner is:\n\n" + winner + "\nCongrats!\n");
            this.ResetGame();
        }
        public Player determineWinner()
        {
            HandsComparer handsComparer = new HandsComparer();
            int biggestScore = 11;//gorzej niz najwyzsza karta
            Player winner = (this.gameTable.Players.Count > 0) ? this.gameTable.Players[0] : null;
            foreach (Player player in gameTable.Players)
            {
                if (player.Folded)
                    continue;
                // Karty gracza i 5 wspolnych lezacych na stole
                CardsCollection PlayerCards = player.PlayerHand + gameTable.shownHelpingCards;
                PlayerCards.SortDesc();
                Console.WriteLine(PlayerCards);
                int playerScore = handsComparer.valueOfCards(PlayerCards);
                if (playerScore <= biggestScore)
                {
                    winner = player;
                    biggestScore = playerScore;                    
                }
            }
            Dealer.Deck.SortDesc();
            Console.WriteLine(Dealer.Deck.ToString());

            return winner;
        }

        public void ResetGame()
        {
            this.Dealer.TakeBackCards(this.gameTable);
            this.Dealer.ChangePosition(this.gameTable);
            this.gameTable.ResetGameState();
        }
    }
}
