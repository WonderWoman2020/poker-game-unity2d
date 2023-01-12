using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    enum PlayerType
    {
        Human,
        Bot
    }
    class Player
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

        public Player(string nick, PlayerType type)
        {
            this.Nick = nick;
            this.Type = type;
            this.PlayerHand = new CardsCollection();
            this.XP = 0;
            this.TokensCount = 1000;
            this.Rank = "Newbie";
            this.Table = null;
            this.folded = false;
        }

        override public string ToString()
        {
            return this.Nick + " (" + Enum.GetName(typeof(PlayerType), this.Type) + ")\n"
                + this.Rank + "\n"
                + this.XP + " XP\n"
                + this.TokensCount + " Tokens\n"
                + "Current table: "+this.Table + "\n";
        }

        public int getInput() 
        {
            //tutaj trzeba cos wykombinowac
            return 0;
        }

        public bool makeMove()
        {
            int input = getInput();

            bool moveDone = false;
            while (!moveDone)
            {
                switch (input)
                {
                    case 0:
                        moveDone = Fold();
                        break;
                    case 1:
                        moveDone = Check();
                        break;
                    case 2:
                        int amount = 0; //get input
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

            return moveDone;
        }

        public bool Fold()
        {
            folded = true;
            return true;
        }

        public bool Check()
        {
            if(this.TokensCount < this.Table.CurrentBid)
            {
                Console.WriteLine("You have not enough tokens to make this move. Make other choice.");
                return false;
            }

            
            this.Table.TokensInGame = this.Table.TokensInGame + this.Table.CurrentBid;
            this.TokensCount = this.TokensCount - this.Table.CurrentBid;

            return true;
        }

        public bool Raise(int amount)
        {
            if(amount < this.Table.CurrentBid || amount > this.TokensCount)
            {
                Console.WriteLine("You have not enough tokens to make this move. Make other choice or decrease raise value.");
                return false;
            }

            this.Table.TokensInGame = this.Table.TokensInGame + amount;
            this.TokensCount = this.TokensCount - amount;
            return true;
        }

        public bool AllIn()
        {
            if(this.TokensCount < this.Table.CurrentBid)
            {
                Console.WriteLine("Amount smaller than current bid in the game. Input bigger value or make different move.");
                return false;
            }

            this.Table.TokensInGame = this.Table.TokensInGame + this.TokensCount;
            this.TokensCount = 0;

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
            this.Nick = newNick;
            return true;
        }

        public bool JoinGameTable(GameTable table)
        {
            table.AddPlayer(this);
            this.Table = table;
            return true;
        }

        public bool LeaveGameTable()
        {
            this.Table.KickOutPlayer(this.Nick);
            this.Table = null;
            return true;
        }
    }
}
