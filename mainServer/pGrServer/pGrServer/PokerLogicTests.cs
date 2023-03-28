using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    class PokerLogicTests
    {
        public void RunExampleGame()
        {
            Card card = new Card(CardSign.Heart, CardValue.Ace);
            Console.WriteLine(card);

            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);

            gameTable.AddPlayer(player2);
            gameTable.AddPlayer(player3);

            Console.WriteLine(gameTable);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            Console.WriteLine(new Card(CardSign.Heart, CardValue.King) > new Card(CardSign.Heart, CardValue.Jack) ?
                "(Test) First Card is worth more" : "(Test) Second Card is worth more");


            TexasHoldemDealer dealer = new TexasHoldemDealer();
            dealer.CreateDeck();
            dealer.ShuffleCards();
            Console.WriteLine(dealer.Deck);
            Console.WriteLine();
            dealer.Deck.SortAsc();
            Console.WriteLine(dealer.Deck);

            gameTable.SortPlayersBySeats();
            gameTable.Players.ForEach(p => Console.WriteLine(p.SeatNr));

            //controller.dealCards();
            controller.playTheGame();
            controller.ConcludeGame();
            Console.ReadKey();
        }

    }
}
