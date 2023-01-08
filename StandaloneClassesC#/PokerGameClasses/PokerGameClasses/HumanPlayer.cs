using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    class HumanPlayer : Player
    {
        public HumanPlayer(string nick, PlayerType type)
            : base(nick, type)
        {

        }

        public GameTable CreateYourTable(string name)
        {
            //TODO set settings too
            GameTable table = new GameTable(name, this);
            this.Table = table;
            return table;
        }
    }
}
