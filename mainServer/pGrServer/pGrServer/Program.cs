using System;
using PokerGameClasses;

namespace pGrServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Card card = new Card(CardSign.Heart, CardValue.Ace);
            Console.WriteLine(card.Name);
        }
    }
}
