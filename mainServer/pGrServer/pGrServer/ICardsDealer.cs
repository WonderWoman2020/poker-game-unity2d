using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    public interface ICardsDealer
    {
        public CardsCollection Deck
        { set; get; }
        public void CreateDeck();
        public void ShuffleCards();
        public void DealCards(GameTable gameTable, int roundNr);
    }
}
