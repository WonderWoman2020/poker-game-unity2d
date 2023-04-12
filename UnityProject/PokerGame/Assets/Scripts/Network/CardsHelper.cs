using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientSideCardsHelper;

namespace ClientSideCardsHelper
{
    public class CardsHelper
    {
        public static CardsCollection StringToCardsCollection(string cards)
        {
            if (cards == "")
                return null;

            CardsCollection cardsCollection = new CardsCollection();

            string[] splittedCards = cards.Split(new string(","));
            foreach(string card in splittedCards)
            {
                string[] cardData = card.Split(new string(" "));
                CardValue value = (CardValue)Convert.ToInt32(cardData[0]);
                CardSign sign = (CardSign)Convert.ToInt32(cardData[1]);
                Card receivedCard = new Card(sign, value);
                cardsCollection.AddCard(receivedCard);
            }

            return cardsCollection;
        }
    }
}
