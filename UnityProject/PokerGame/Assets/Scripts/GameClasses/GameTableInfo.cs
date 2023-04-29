using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    public class GameTableInfo
    {
        public string Name;
        public string Owner;
        public string HumanCount;
        public string BotCount;
        public string minXp;
        public string minChips;


        public GameTableInfo(string name, string owner, string humanCount, string botCount, string minXp, string minChips)
        {
            this.Name = name;
            this.Owner = owner;
            this.HumanCount = humanCount;
            this.BotCount = botCount;
            this.minXp = minXp;
            this.minChips = minChips;
        }

        // TODO metoda Unpack GameTableInfo
    }
}
