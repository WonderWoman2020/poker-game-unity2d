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
        public CardsCollection PlayerHand
        { get; set; }
        public int XP
        { get; set; }
        public int TokensCount
        { get; set; }
        public string Rank
        { get; set; }
        public PlayerType Type
        { get; set; }
        public GameTable Table
        { get; set; }
        public bool folded;

        public bool AllInMade
        { get; set; }

        public int PlayersCurrentBet
        { get; set; }

        public int SeatNr
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
            this.folded = false;
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

        public bool makeMove()
        {
            if (this.Table == null)
            {
                Console.WriteLine("You can't make a move - you're not sitting by any game table. You must join a game table first.");
                return false;
            }

            bool moveDone = false;
            int playersCurrentTokensCount = this.TokensCount;
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

            this.PlayersCurrentBet += playersCurrentTokensCount - this.TokensCount;

            return moveDone;
        }

        public bool Fold()
        {
            folded = true;
            return true;
        }

        public bool Check()
        {
            if(this.TokensCount < (this.Table.CurrentBid - this.PlayersCurrentBet))
            {
                Console.WriteLine("You have not enough tokens to make this move. Make other choice.");
                return false;
            }

            
            this.Table.TokensInGame = this.Table.TokensInGame + (this.Table.CurrentBid - this.PlayersCurrentBet);
            this.TokensCount = this.TokensCount - (this.Table.CurrentBid - this.PlayersCurrentBet);
            return true;
        }

        public bool Raise(int amount)
        {
            if(this.TokensCount < amount + (this.Table.CurrentBid - this.PlayersCurrentBet))
            {
                Console.WriteLine("You have not enough tokens to make this move. Make other choice or decrease raise value.");
                return false;
            }

            this.Table.TokensInGame = this.Table.TokensInGame + (this.Table.CurrentBid - this.PlayersCurrentBet) + amount;
            this.TokensCount = this.TokensCount - ((this.Table.CurrentBid - this.PlayersCurrentBet) + amount);
            this.Table.CurrentBid = this.Table.CurrentBid + amount;
            return true;
        }

        public bool AllIn()
        {
            if(this.TokensCount < (this.Table.CurrentBid - this.PlayersCurrentBet))
            {
                Console.WriteLine("Amount smaller than current bid in the game. Input bigger value or make different move.");
                return false;
            }

            this.Table.TokensInGame = this.Table.TokensInGame + this.TokensCount;
            if (this.TokensCount > this.Table.CurrentBid)
                this.Table.CurrentBid = this.TokensCount;

            this.TokensCount = 0;
            this.AllInMade = true;

            return true;
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
            this.folded = false;
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
