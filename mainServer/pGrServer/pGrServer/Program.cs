using System;
using PokerGameClasses;

namespace pGrServer
{
    class Program
    {
        static void Main(string[] args)
        {
            PokerLogicTests pokerTester = new PokerLogicTests();
            pokerTester.RunExampleGame();
        }
    }
}
