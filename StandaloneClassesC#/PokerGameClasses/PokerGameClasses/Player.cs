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
        //public GameTable Table;

        public Player(string nick, PlayerType type)
        {
            this.Nick = nick;
            this.Type = type;
            this.PlayerHand = new CardsCollection();
            this.XP = 0;
            this.TokensCount = 1000;
            this.Rank = "Newbie";
            //this.Table = null;
        }

        override public string ToString()
        {
            return this.Nick + " (" + Enum.GetName(typeof(PlayerType), this.Type) + ")\n"
                +this.Rank+"\n"
                + this.XP + " XP\n"
                + this.TokensCount + " Tokens\n";
        }

        public void Fold()
        {
            //TODO
        }

        public void Check()
        {
            //TODO
        }

        public bool Raise(int amount)
        {
            this.TokensCount = this.TokensCount - amount;
            return true;
        }

        public void AllIn()
        {
            //TODO
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

        public bool JoinGameTable(string gameTableName)
        {
            //TODO GameTable
            return true;
        }

        public bool LeaveGameTable()
        {
            //TODO GameTable
            return true;
        }
    }
}
