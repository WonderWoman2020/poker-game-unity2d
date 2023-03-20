using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
   public enum CardSign
    {
        Heart, //Serce
        Spade, // Pik
        Diamond, //Kier
        Club //Trefl
    }

    public enum CardValue
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

   public enum CardColor
    {
        Black,
        Red
    }
 

    public class Card
    {
        public CardSign Sign
        { get; set; }
        public CardValue Value
        { get; set; }
        
        public Card(CardSign sign, CardValue val)
        {
            this.Sign = sign;
            this.Value = val;
        }

        public CardColor GetCardColor()
        {
            if (this.Sign == CardSign.Diamond || this.Sign == CardSign.Heart)
                return CardColor.Red;
            else
                return CardColor.Black;
        }

        public string GetShortSign()
        {
            switch (this.Sign)
            {
                case CardSign.Heart:
                    return "♥";
                case CardSign.Spade:
                    return "♠";
                case CardSign.Diamond:
                    return "♦";
                case CardSign.Club:
                    return "♣";
                default:
                    return "X";                
            }
        }

        public string GetShortValue()
        {
            if (this.Value >= CardValue.Two && this.Value <= CardValue.Ten)
                return ((int)this.Value).ToString();

            if (this.Value >= CardValue.Jack && this.Value <= CardValue.Ace)
            {
                string val = this.Value.ToString();
                return val[0].ToString();
            }

            return "Y";
        }

        public string GetShortName()
        {
            return this.GetShortValue() + this.GetShortSign();
        }

        public string GetName()
        {
            return this.Value.ToString() + " " + this.Sign.ToString();
        }

        override public string ToString()
        {
            return this.GetShortName();
        }
    }
}
