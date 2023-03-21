using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    public class TexasHoldemDealer : ICardsDealer
    {
        public CardsCollection Deck 
        { get; set; }

        public TexasHoldemDealer()
        {
            this.Deck = null;
        }
        public void CreateDeck()
        {
            List<Card> deck = new List<Card>(52);

            int index = 0;
            foreach (CardValue cardVal in Enum.GetValues(typeof(CardValue)))
            {
                foreach (CardSign cardSign in Enum.GetValues(typeof(CardSign)))
                {
                    deck.Add(new Card(cardSign, cardVal));
                    index++;
                }
            }
            this.Deck = new CardsCollection(deck);
        }

        public void DealCards(GameTable gameTable, int roundNr)
        {
            throw new NotImplementedException();
        }

        public void ShuffleCards()
        {
            System.Random random = new System.Random();
            this.Deck.Cards = this.Deck.Cards.OrderBy(c => random.Next()).ToList();
        }
    }
}
