using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    public enum PlayerType
    {
        Human,
        Bot
    }
    public class Player
    {
        public string Nick
        { get; set; }
        public PlayerType Type
        { get; set; }
        public string Rank
        { get; set; }
        public int XP
        { get; set; }
        public int TokensCount
        { get; set; }

        public GameTable Table
        { get; set; }
        public CardsCollection PlayerHand
        { get; set; }
        public int SeatNr
        { get; set; }
        public int PlayersCurrentBet
        { get; set; }
        public bool Folded
        { get; set; }
        public bool AllInMade
        { get; set; }

        public Player(string nick, PlayerType type)
        {
            this.ChangeNick(nick);
            this.Type = type;
            this.PlayerHand = new CardsCollection();
            this.XP = 0;
            this.TokensCount = 1000;
            this.Rank = "Newbie";
            this.Table = null;
            this.Folded = false;
            this.AllInMade = false;
            this.PlayersCurrentBet = 0;
            this.SeatNr = 0;
        }

        override public string ToString()
        {
            return this.Nick + " (" + Enum.GetName(typeof(PlayerType), this.Type) + ")\n"
                + this.Rank + "\n"
                + this.XP + " XP\n"
                + this.TokensCount + " Tokens\n"
                + "Current table: "+(this.Table == null ? "No table" : this.Table.Name) + "\n";
        }

        public bool MakeMove()
        {
            if (this.Table == null)
            {
                Console.WriteLine("You can't make a move - you're not sitting by any game table. You must join a game table first.");
                return false;
            }

            bool moveDone = false;
            //int playersTokensCountBeforeMove = this.TokensCount;
            while (!moveDone)
            {
                //potem zamienić na pobieranie inputu z przycisków
                Console.WriteLine("Input move number to be made: \n0 - Fold\n1 - Check\n2 - Raise\n3 - AllIn");
                int input = Convert.ToInt32(Console.ReadLine());
                switch (input)
                {
                    case 0:
                        moveDone = Fold();
                        break;
                    case 1:
                        moveDone = Check();
                        break;
                    case 2:
                        //tu też potem zamienić input
                        Console.WriteLine("Input how much you want to raise the bid");
                        int amount = Convert.ToInt32(Console.ReadLine());
                        moveDone = Raise(amount);
                        break;
                    case 3:
                        moveDone = AllIn();
                        break;
                    default:
                        moveDone = Fold();
                        break;
                }
            }

            //this.PlayersCurrentBet += playersTokensCountBeforeMove - this.TokensCount;

            return moveDone;
        }

        public bool SmallBlindFirstMove()
        {
            int smallBlindAmount = this.Table.Settings.MinPlayersTokenCount/2;
            return this.SpendSomeMoneyOnHazard(smallBlindAmount);
        }

        public bool BigBlindFirstMove()
        {
            int bigBlindAmount = this.Table.Settings.MinPlayersTokenCount;
            return this.SpendSomeMoneyOnHazard(bigBlindAmount);
        }
        private bool SpendSomeMoneyOnHazard(int amount)
        {
            if (this.TokensCount < amount)
            {
                Console.WriteLine("You have not enough tokens to make this move. Make other choice.");
                return false;
            }

            this.TokensCount = this.TokensCount - amount;
            this.PlayersCurrentBet = this.PlayersCurrentBet + amount;

            this.Table.TokensInGame = this.Table.TokensInGame + amount;
            if (this.PlayersCurrentBet > this.Table.CurrentBid)
                this.Table.CurrentBid = this.PlayersCurrentBet;
            //dodać to w kontrolerze i wskaźnik na gracza, który ostatni przebił

            return true;
        }
        public bool Fold()
        {
            Folded = true;
            return true;
        }
        public bool Check()
        {
            int amountNeededToMakeCheck = this.Table.CurrentBid - this.PlayersCurrentBet;
            return this.SpendSomeMoneyOnHazard(amountNeededToMakeCheck);
        }

        public bool Raise(int amount)
        {
            int amountNeededToMakeCheck = this.Table.CurrentBid - this.PlayersCurrentBet;
            return this.SpendSomeMoneyOnHazard(amount + amountNeededToMakeCheck);
        }

        // po zmianie, AllIn można wykonać zawsze, nawet jeśli mamy za mało tokenów na dobicie do stawki
        // (tak jak ustalaliśmy, zawsze można się ratować allIn'em)
        public bool AllIn()
        {
            bool moveMade = this.SpendSomeMoneyOnHazard(this.TokensCount);
            if (moveMade)
                this.AllInMade = true;
            return moveMade;
        }

        public void BuyTokens(int amount)
        {
            //TODO TokensShop
            this.TokensCount = this.TokensCount + amount;
        }

        public void BuyXP(int amount)
        {
            //TODO TokensShop
            this.XP = this.XP + amount;
        }

        public bool ChangeNick(string newNick)
        {
            if (newNick == null)
            {
                if (this.Nick == null)
                {
                    this.Nick = "Player";
                    return true;
                }

                return false;
            }

            this.Nick = newNick;
            return true;
        }

        public void ResetPlayerGameState()
        {
            this.PlayerHand = new CardsCollection();
            this.Folded = false;
            this.AllInMade = false;
            this.PlayersCurrentBet = 0;
        }

        public string PlayerGameState()
        {
            return "Player's '"+this.Nick+ "' game state:\n" 
                + "Hand: " + String.Join(", ", this.PlayerHand.Cards)
                + "\nTokens: " + this.TokensCount
                + "\nCurrent bet: "+this.PlayersCurrentBet
                + "\nXP: " + this.XP;
        }
    }
}
