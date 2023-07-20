using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    public class HumanPlayer : Player
    {
        public HumanPlayer(string nick, PlayerType type)
            : base(nick, type)
        {

        }

        public GameTable CreateYourTable(string name, GameTableSettings settings)
        {
            if (this.Table != null)
                this.Table.Remove(this.Nick);

            GameTable table = new GameTable(name, this);
            if (settings != null)
            {
                if (settings.MinPlayersXP > this.XP)
                {
                    //Console.WriteLine("Not enough XP for this table setting - XP of your table will be lowered");
                    settings.MinPlayersXP = this.XP;
                }

                if (settings.BigBlind > this.TokensCount)
                {
                    //Console.WriteLine("Not enough tokens for this table setting - tokens count of your table will be lowered");
                    settings.changeBigBlind(this.TokensCount);
                }
                if (settings.MinTokens > this.TokensCount)
                {
                    //Console.WriteLine("Not enough tokens for this table setting - tokens count of your table will be lowered");
                    settings.changeMinTokens(this.TokensCount);
                }

            }
            table.ChangeSettings(this, settings);

            return table;
        }
    }
}
