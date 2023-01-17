using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    public class GameTable
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
            this.ChangeName(name);
            this.TokensInGame = 0;
            this.CurrentBid = 0;
            this.shownHelpingCards = new CardsCollection();
            this.Settings = new GameTableSettings();
            this.Players = new List<Player>();
            this.AddPlayer(owner);
            this.ChangeOwner(owner);
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
            {
                Console.WriteLine("You already sit at that table, dude, wake up.");
                return false;
            }

            if (this.Players.Count == this.Settings.MaxPlayersCountInGame)
            {
                Console.WriteLine("Too many players already at the table. You can't join this table now.");
                return false;
            }

            if (player != null)
            {
                if (this.Settings.MinPlayersXP > player.XP)
                {
                    Console.WriteLine("Not enought XP to join the game table");
                    return false;
                }
                if (this.Settings.MinPlayersTokenCount > player.TokensCount)
                {
                    Console.WriteLine("Not enought Tokens to join the game table");
                    return false;
                }
            }

            this.Players.Add(player);

            if (player != null)
                player.Table = this;

            if (this.Owner == null && player.Type == PlayerType.Human)
                this.ChangeOwner((HumanPlayer)player);

            return true;
        }

        public bool KickOutPlayer(string playerNick)
        {
            Player player = this.Players.Find(p => p.Nick == playerNick);
            this.Players.Remove(player);

            if (player != null)
                player.Table = null;

            if (player == this.Owner)
            {
                if (this.GetPlayerTypeCount(PlayerType.Human) > 0)
                {
                    HumanPlayer newOwner = (HumanPlayer)this.Players.Find(p => p.Type == PlayerType.Human);
                    this.ChangeOwner(newOwner);
                }
                else
                    this.ChangeOwner(null);
            }

            return true;
        }

        public bool ChangeSettings(Player player, GameTableSettings settings)
        {
            if (this.Owner == null)
                return false;

            if (player.Nick != this.Owner.Nick)
                return false;

            if(settings == null)
            {
                this.Settings = new GameTableSettings();
                return true;
            }

            this.Settings = settings;
            return true;
        }

        override public string ToString()
        {
            return "Name: " + this.Name + "\n"
                + "Owner: " + ((this.Owner == null) ? "No owner" : this.Owner.Nick) + "\n"
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
            if (newOwner != null)
                this.AddPlayer(newOwner);

            return true;
        }

        public void makeTurn() 
        {
            foreach(Player player in Players) 
            {
                if(!player.AllInMade && !player.folded)
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

        public void ResetGameState()
        {
            this.TokensInGame = 0;
            this.CurrentBid = 0;
            this.shownHelpingCards = new CardsCollection();
            this.Players.ForEach(p => p.PlayerHand = new CardsCollection());
            this.Players.ForEach(p => p.folded = false);
            this.Players.ForEach(p => p.AllInMade = false);
        }

    }
}
