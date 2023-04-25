using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
   public enum CardSign
    {
        Club, //Trefl
        Diamond, //Karo
        Heart, //Kier
        Spade // Pik
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
 
    public class Card : IComparable<Card>
    {
        public CardSign Sign
        { get; set; }
        public CardValue Value
        { get; set; }
        public int Id
        { get; set; }
        public Card(CardSign sign, CardValue val, int id)
        {
            this.Sign = sign;
            this.Value = val;
            this.Id = id;
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
                case CardSign.Spade:
                    return "♠";
                case CardSign.Heart:
                    return "♥";
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
                //return ((int)this.Value).ToString();
                return Convert.ToString((int)this.Value);

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
            //return ((int)this.Value).ToString() + " " + ((int)this.Sign).ToString();
        }

        public int CompareTo(Card other)
        {
            if ((int)this.Value == (int)other.Value)
            {
                if ((int)this.Sign == (int)other.Sign)
                    return 0; //równe karty, w sumie niemożliwa sytuacja w grze

                return (int)this.Sign - (int)other.Sign; //jeśli karta ma wyższy znak, zwraca liczbę dodatnią
            }

            return (int)this.Value - (int)other.Value;
        }

        public static bool operator ==(Card first, Card second)
        {
            if (first.CompareTo(second) == 0)
                return true;

            return false;
        }

        public static bool operator !=(Card first, Card second)
        {
            if (first.CompareTo(second) != 0)
                return true;

            return false;
        }

        public static bool operator >(Card first, Card second)
        {
            if (first.CompareTo(second) > 0)
                return true;

            return false;
        }

        public static bool operator <(Card first, Card second)
        {
            if (first.CompareTo(second) < 0)
                return true;

            return false;
        }
    }
}
