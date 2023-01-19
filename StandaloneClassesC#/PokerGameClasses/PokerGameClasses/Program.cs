using System;
using System.Collections.Generic;
using System.Linq;
namespace PokerGameClasses
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Test klas Card i CardsCollection\n");
            Card sampleCard = new Card(CardSign.Heart, CardValue.Ten);
            Console.WriteLine(sampleCard.Name);
            Console.WriteLine(sampleCard.CardColorToString());

            Console.WriteLine();

            CardsCollection sampleDeck = CardsCollection.CreateStandardDeck();
            sampleDeck.Cards.ForEach(c => Console.WriteLine(c.Name));
            Console.WriteLine();
            Card takenCard = sampleDeck.TakeOutCard(CardSign.Heart, CardValue.Ace);
            sampleDeck.Cards.ForEach(c => Console.WriteLine(c.Name));
            Console.WriteLine();
            Console.WriteLine(takenCard.Name);
            Console.WriteLine(sampleDeck.TakeOutCard(CardSign.Heart, CardValue.Ace));
            sampleDeck.Cards.ForEach(c => Console.WriteLine(c.Name));
            Console.WriteLine();
            sampleDeck.ShuffleCards();
            sampleDeck.Cards.ForEach(c => Console.WriteLine(c.Name));

            Console.WriteLine();

            Player samplePlayer = new Player("Gamer1", PlayerType.Human);
            Console.WriteLine(samplePlayer);
            //samplePlayer.Raise(10);
            samplePlayer.BuyXP(100);
            samplePlayer.ChangeNick("Gamer#1");
            Console.WriteLine(samplePlayer);

            Console.WriteLine("\nTest łączenia i sortowania");
            Console.WriteLine("............///////////////////////////.");
            sampleDeck.Cards.ForEach(c => Console.WriteLine(c.Name));
            Console.WriteLine("........");
            CardsCollection.SortDesc(sampleDeck).Cards.ForEach(c => Console.WriteLine(c.Name));
            Console.WriteLine("............///////////////////////////.");
            sampleDeck = CardsCollection.MergeTwoDecks(sampleDeck,sampleDeck);
            CardsCollection.SortDesc(sampleDeck).Cards.ForEach(c => Console.WriteLine(c.Name));

            Console.WriteLine("\nTest wyryfikacji układów");
            CardsCollection mainDeck = CardsCollection.CreateStandardDeck();
            GameplayController controller = new GameplayController();
            mainDeck.ShuffleCards();
            Card[] testingCards = new Card[7];
            for (int i = 0; i < 7; i++)
            {
                testingCards[i] = mainDeck.Cards[i];
            }
            List<Card> toCheck = testingCards.ToList();
            CardsCollection handleCards = new CardsCollection(toCheck);
            CardsCollection sortedCards = CardsCollection.SortDesc(handleCards);
            for (int i = 0; i < 7; i++)
            {
                Console.WriteLine(sortedCards.Cards[i].Name);
            }
            int value = controller.valueOfCards(handleCards);
            Console.WriteLine(value);


            Console.WriteLine("\nTest klas Player i HumanPlayer, GameTable i GameTableSettings\n");
            Player humanPlayer = new HumanPlayer("BasePlayer#1", PlayerType.Human);
            Console.WriteLine("Gracz przed dodaniem do stolika:");
            Console.WriteLine(humanPlayer);          

            humanPlayer.ChangeNick(null);
            GameTable testTableForPlayer = new GameTable("Table 1", (HumanPlayer)humanPlayer);
            Console.WriteLine("Gracz i stolik po dodaniu do stolika:");
            Console.WriteLine(humanPlayer);
            Console.WriteLine(testTableForPlayer);

            testTableForPlayer.KickOutPlayer(humanPlayer.Nick);
            Console.WriteLine("Gracz i stolik po odejściu do stolika:");
            Console.WriteLine(humanPlayer);
            Console.WriteLine(testTableForPlayer);

            humanPlayer.makeMove();
            humanPlayer.ChangeNick("Nick#1");
            testTableForPlayer.AddPlayer(humanPlayer);
            testTableForPlayer.ChangeName("Best Table In Town");
            Console.WriteLine("Gracz i stolik po ponownym dodaniu do stolika i zmianie nazwy:");
            Console.WriteLine(humanPlayer);
            Console.WriteLine(testTableForPlayer);

            humanPlayer.makeMove();
            Console.WriteLine("Gracz i stolik po wykonaniu ruchu przez gracza:");
            Console.WriteLine(humanPlayer);
            Console.WriteLine(testTableForPlayer);
            Console.WriteLine(testTableForPlayer.CurrentBid);
            Console.WriteLine(testTableForPlayer.TokensInGame);

            humanPlayer.BuyTokens(300);
            humanPlayer.BuyXP(100);
            Console.WriteLine("Gracz po zakupach:");
            Console.WriteLine(humanPlayer);

            GameTableSettings settings = new GameTableSettings();
            settings.changeMinXP(10000);
            GameTable newGameTable = ((HumanPlayer)humanPlayer).CreateYourTable("My new Table",settings);
            Console.WriteLine("Utworzenie nowego stolika przez gracza:");
            Console.WriteLine(newGameTable);
            Console.WriteLine(humanPlayer);
            Console.WriteLine(testTableForPlayer);

        }
    }
}
