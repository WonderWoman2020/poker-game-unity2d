using System;

namespace PokerGameClasses
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Card sampleCard = new Card(CardSign.Heart, CardValue.Ten);
            Console.WriteLine(sampleCard.Name);
            Console.WriteLine(sampleCard.CardColorToString());
        }
    }
}
