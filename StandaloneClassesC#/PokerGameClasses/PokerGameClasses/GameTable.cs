using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    class GameTable
    {
        public string Name
        { get; set; }
        public HumanPlayer Owner
        { get; set; }
        public List<Player> Players
        { get; set; }
        public CardsCollection shownHelpingCards; //odsloniete karty pomocnicze, brac z tego pola do GUI

        public int TokensInGame
        { get; set; }
        public int CurrentBid
        { get; set; }

        public GameTableSettings Settings
        { get; set; }

        public GameTable(string name, HumanPlayer owner)
        {
            this.Name = name;
            this.Owner = owner;
            this.TokensInGame = 0;
            this.CurrentBid = 0;
        }

        public int GetPlayerTypeCount(PlayerType type)
        {
            int count = 0;
            foreach(Player player in Players)
            {
                if (player.Type == type)
                    count++;
            }
            return count;
        }

        public bool AddPlayer(Player player)
        {
            if (this.Players.Contains(player))
                return false;

            this.Players.Add(player);
            return true;
        }

        public bool KickOutPlayer(string playerNick)
        {
            Player player = this.Players.Find(p => p.Nick == playerNick);
            this.Players.Remove(player);
            return true;
        }

        public void ShowSettings()
        {
            //TODO
        }

        public bool ChangeSettings(Player player, GameTableSettings settings)
        {
            if (player.Nick != this.Owner.Nick)
                return false;

            this.Settings = settings;
            return true;
        }

        override public string ToString()
        {
            //TODO settings about XP and Tokens in ToString included
            return "Name: " + this.Name + "\n"
                + "Owner: " + this.Owner + "\n"
                + "Human count: " + this.GetPlayerTypeCount(PlayerType.Human) + "\n"
                + "Bots count: " + this.GetPlayerTypeCount(PlayerType.Bot) + "\n"
                + "Min XP: " + this.Settings.MinPlayersXP + "\n"
                + "Min Chips: " + this.Settings.MinPlayersTokenCount + "\n";
        }

        public bool ChangeName(string newName)
        {
            this.Name = newName;
            return true;
        }

        public bool ChangeOwner(HumanPlayer newOwner)
        {
            this.Owner = newOwner;
            return true;
        }

        public void makeTurn() 
        {
            foreach(Player player in Players) 
            {
                player.makeMove();
            }
        }

        public bool checkIfEveryoneFolded()
        {
            bool everyoneFolded = true;
            foreach (Player player in Players) 
            {
                if(player.folded == false) 
                {
                    everyoneFolded = false;
                    break;
                }
            }
            return everyoneFolded;
        }

    }
}
