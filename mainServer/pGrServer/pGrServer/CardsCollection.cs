using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    public class CardsCollection
    {
        public List<Card> Cards
        { get; set; }

        public CardsCollection()
        {
            this.Cards = new List<Card>();
        }

        public CardsCollection(List<Card> cards)
        {
            this.Cards = cards;
        }

        public bool AddCard(Card card)
        {
            this.Cards.Add(card);
            return true;
        }
        public Card TakeOutCard(CardSign sign, CardValue val)
        {
            Card takenCard = this.Cards.Find(c => c.Sign == sign && c.Value == val);
            this.Cards.Remove(takenCard);
            return takenCard;
        }

        public Card TakeOutCard(int cardNr)
        {
            Card takenCard = this.Cards.ElementAt(cardNr);
            this.Cards.Remove(takenCard);
            return takenCard;
        }
        static public CardsCollection operator +(CardsCollection first, CardsCollection second)
        {
            CardsCollection cardsCollection = new CardsCollection();
            cardsCollection.Cards.AddRange(first.Cards);
            cardsCollection.Cards.AddRange(second.Cards);
            return cardsCollection;
        }
        public void SortDesc()
        {
            this.Cards.Sort((x,y)=>y.CompareTo(x));
        }

        public void SortAsc()
        {
            this.Cards.Sort();
        }
        public bool ShuffleCards()
        {
            System.Random random = new System.Random();
            this.Cards = this.Cards.OrderBy(c => random.Next()).ToList();
            return true;
        }

        public static CardsCollection CreateStandardDeck()
        {
            List<Card> deck = new List<Card>(52);

            int index = 0;
            foreach(CardValue cardVal in Enum.GetValues(typeof(CardValue)))
            {
                foreach(CardSign cardSign in Enum.GetValues(typeof(CardSign)))
                {
                    deck.Add(new Card(cardSign, cardVal));
                    index++;
                }
            }
            return new CardsCollection(deck);
        }

        override public string ToString()
        {
            return string.Join(", ", this.Cards);
        }
    }
}
