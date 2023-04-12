using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using pGrServer;

namespace PokerGameClasses
{
    public enum PlayerType
    {
        Human,
        Bot
    }
    public class Player
    {
        public string Token { get; set; }
        public TcpClient MenuRequestsTcp { get; set; }
        public NetworkStream MenuRequestsStream { get; set; }
        public TcpClient GameRequestsTcp { get; set; }
        public NetworkStream GameRequestsStream { get; set; }
        public string Login { get; set; }
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
        public void UpdateData(int xp, int coins, string login)
        {
            this.XP = xp;
            this.TokensCount = coins;
            this.Login = login;
        }
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
            while (!moveDone)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(":G:");
                sb.Append("Move request");
                sb.Append("|");
                sb.AppendLine("Input move number to be made: \n0 - Fold\n1 - Check\n" +
                    "2 - Raise (if you choose this option, write also amount of coins to raise, separated by 1 space)\n" +
                    "3 - AllIn");                

                NetworkHelper.WriteNetworkStream(this.GameRequestsStream, sb.ToString());
                this.GameRequestsStream.Flush();
                string moveResponse = NetworkHelper.ReadNetworkStream(this.GameRequestsStream);
                string[] splitted = moveResponse.Split(new string(" "));

                switch (Convert.ToInt32(splitted[0]))
                {
                    case 0:
                        moveDone = Fold();
                        break;
                    case 1:
                        moveDone = Check();
                        break;
                    case 2:
                        sb.Clear();
                        sb.Append(":G:");
                        sb.Append("Move request");
                        sb.Append("|");
                        sb.AppendLine("Input how much you want to raise the bid:");

                        //NetworkHelper.WriteNetworkStream(this.GameRequestsStream, sb.ToString());
                        //this.GameRequestsStream.Flush();
                        //moveResponse = NetworkHelper.ReadNetworkStream(this.GameRequestsStream);

                        int amount = Convert.ToInt32(splitted[1]);
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
                StringBuilder sb = new StringBuilder();
                sb.Append(":G:");
                sb.Append("Info");
                sb.Append("|");
                sb.AppendLine("You have not enough tokens to make this move. Make other choice.");
                NetworkHelper.WriteNetworkStream(this.GameRequestsStream, sb.ToString());
                this.GameRequestsStream.Flush();
                return false;
            }
            this.TokensCount = this.TokensCount - amount;
            this.PlayersCurrentBet = this.PlayersCurrentBet + amount;

            this.Table.TokensInGame = this.Table.TokensInGame + amount;

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

        public string MessageGameState()
        {
            return ":Nick:" + this.Nick
                + ":Hand:" + this.PlayerHand.ToString()
                + ":Tokens:" + this.TokensCount
                + ":Current bet:" + this.PlayersCurrentBet
                + ":XP: " + this.XP;
        }

    }
}
