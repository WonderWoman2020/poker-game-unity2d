﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PokerGameClasses
{
    class CardsCollection : MonoBehaviour
    {

        static public Sprite[] cardsSprites
        { get; set; }
        [SerializeField]
        private Sprite[] cardsSpriteSerialization;
        public List<Card> Cards
        { get; set; }


        public void Start()
        {
            
        }
        public CardsCollection()
        {
            DeserializeCards();
        }

        public CardsCollection(List<Card> cards)
        {
            this.Cards = cards;
            DeserializeCards();
        }
        public void DeserializeCards()
        {
            cardsSprites = cardsSpriteSerialization;
        }
        public void ShowCards()
        {

        }

        public bool AddCard(Card card)
        {
            this.Cards.Add(card);
            return true;
        }
        public static CardsCollection MergeTwoDecks(CardsCollection firstCardsCollection, CardsCollection secondCardsCollection)
        {
            CardsCollection cardsCollection = new CardsCollection();
            cardsCollection.Cards = firstCardsCollection.Cards;
            cardsCollection.Cards.AddRange(secondCardsCollection.Cards);
            return cardsCollection;
        }
        public static CardsCollection SortDesc(CardsCollection cards)
        {
            cards.Cards.Sort((x,y)=>y.Value-x.Value);
            return cards;
        }
        public Card TakeOutCard(CardSign sign, CardValue val)
        {
            Card takenCard = this.Cards.Find(c => c.Sign == sign && c.Value == val);
            this.Cards.Remove(takenCard);
            return takenCard;
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
                    deck.Add(new Card(cardSign, cardVal, cardsSprites[index]));
                    index++;
                }
            }
            return new CardsCollection(deck);
        }
    }
}
