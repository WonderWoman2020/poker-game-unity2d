﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PokerGameClasses
{
    enum CardSign
    {
        Heart, //Serce
        Spade, // Pik
        Diamond, //Kier
        Club //Trefl
    }

    enum CardValue
    {
        Two = 2,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack, //Jopek
        Queen, //Krolowa
        King, //Krol
        Ace //As
    }

    enum CardColor
    {
        Black,
        Red
    }
 

    class Card : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;
        public Sprite cardSprite;
        public CardSign Sign
        { get; set; }
        public CardValue Value
        { get; set; }
        public string Name
        { get; set; }

        public void Start()
        {
            cardSprite = CardsCollection.cardsSprites[0];
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = cardSprite;
        }
        public Card(CardSign sign, CardValue val, Sprite img)
        {
            this.Sign = sign;
            this.Value = val;
            this.cardSprite = img;
            this.Name = this.CreateCardName();
        }

        public CardColor GetCardColor()
        {
            if (this.Sign == CardSign.Diamond || this.Sign == CardSign.Heart)
                return CardColor.Red;
            else
                return CardColor.Black;
        }

        private string CreateCardName()
        {
            return this.CardValueToString() + " " + this.CardSignToString();
        }

        public string CardValueToString()
        {
            if ((int)this.Value <= 10)
                return this.Value.ToString();

            switch (this.Value)
            {
                case CardValue.Jack:
                    return "Jack";
                case CardValue.Queen:
                    return "Queen";
                case CardValue.King:
                    return "King";
                case CardValue.Ace:
                    return "Ace";
                default:
                    return "Undef. Value";
            }
        }

        public string CardSignToString()
        {
            switch(this.Sign)
            {
                case CardSign.Heart:
                    return "Heart";
                case CardSign.Spade:
                    return "Spade";
                case CardSign.Diamond:
                    return "Diamond";
                case CardSign.Club:
                    return "Club";
                default:
                    return "Undef. Sign";
            }
        }

        public string CardColorToString()
        {
            if (this.GetCardColor() == CardColor.Black)
                return "Black";
            else
                return "Red";
        }
    }
}
