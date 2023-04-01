using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using PokerGameClasses;

namespace pGrServer
{
    class Client
    {
        public string Token { get; set; }
        public TcpClient MenuRequestsTcp { get; set; }
        public NetworkStream MenuRequestsStream { get; set; }
        public TcpClient GameRequestsTcp { get; set; }
        public NetworkStream GameRequestsStream { get; set; }
        public string Nick { get; }
        public int Xp { get; set; }
        public int Coins { get; set; }
        public string Login { get; }
        public GameTable GameTable { get; set; }
        public Player Player { get; set; }
        public Client(string nick, int xp, int coins, string login)
        {
            this.Nick = nick;
            this.Xp = xp;
            this.Coins = coins;
            this.Login = login;
        }
    }
}
