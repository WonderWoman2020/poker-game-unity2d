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

        public GameTable CreateYourTable(string name, GameTableSettings settings)
        {
            GameTable table = new GameTable(name, this);
            table.ChangeSettings(this, settings);
            this.Table = table;

            return table;
        }
    }
}
