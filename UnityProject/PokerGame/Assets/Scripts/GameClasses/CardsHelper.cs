using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PokerGameClasses
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

                int index = CardsHelper.GetCardSpriteID(sign, value);
                Card receivedCard = new Card(sign, value, index);
                cardsCollection.AddCard(receivedCard);
            }

            return cardsCollection;
        }

        public static int GetCardSpriteID(CardSign sign, CardValue value)
        {
            //pozyskiwanie ID karty
            int v = Convert.ToInt32(value) - 2;
            List<CardSign> signsInOrder = new List<CardSign> { CardSign.Heart, CardSign.Spade, CardSign.Diamond, CardSign.Club };
            int s = signsInOrder.IndexOf(sign);
            // Jeśli zły znak, zwróć ID sprite'a tyłu karty
            if (s == -1)
                return 52;

            int index = s * 13 + v;
            return index;
        }
    }
}
