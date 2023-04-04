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
            if (this.Cards == null)
                this.Cards = new List<Card>();

            this.Cards.Add(card);
            return true;
        }
        public Card TakeOutCard(CardSign sign, CardValue val)
        {
            if (this.Cards == null)
                return null;

            Card takenCard = this.Cards.Find(c => c.Sign == sign && c.Value == val);
            this.Cards.Remove(takenCard);
            return takenCard;
        }

        public Card TakeOutCard(int cardNr)
        {
            if (this.Cards == null)
                return null;

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
            if (this.Cards == null)
                return;

            this.Cards.Sort((x,y)=>y.CompareTo(x));
        }

        public void SortAsc()
        {
            if (this.Cards == null)
                return;

            this.Cards.Sort();
        }

        override public string ToString()
        {
            return string.Join(", ", this.Cards);
            //return string.Join(",", this.Cards);
        }
    }
}
