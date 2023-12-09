using NUnit.Framework;
using PokerGameClasses;
using System.Collections.Generic;
using System.Linq;

namespace TestProjectWinner
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }
        public static bool IsSame(List<Player> players1, List<Player> players2)
        {
            if (players1.Count != players2.Count)
                return false;
            List<Player> list1 = players1.OrderBy(x => x.Nick).ToList();
            List<Player> list2 = players2.OrderBy(x => x.Nick).ToList();
            for (int i=0; i< list1.Count; i++)
                if (list1[i] != list2[i])
                    return false;
            return true;
        }

        [Test]
        public void DetermineWinner_RoyalStraightFlush_ReturnsListOfWinners()//Krolewski poker
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4  = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Jack));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Three));

            //para
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Six));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Five));

            //trojka
            player3.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Jack));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));

            //poker
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Nine));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Eight));

            //krolewski
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.King));

            
            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player2);


            Assert.AreEqual(winners, winnersShouldBe);
        }

        [Test]
        public void DetermineWinner_Poker_ReturnsListOfWinner()//Poker
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Five));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Ace));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Four));

            //para
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Two));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Two));

            //kolor
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));

            //full
            player3.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Four));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Ace));

            //poker
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player4);


            Assert.AreEqual(winners, winnersShouldBe);
        }

        [Test]
        public void DetermineWinner_FourOfKind_ReturnsListOfWinner()//Kareta
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Five));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Ten));
            
            //para
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Two));
            
            //strit
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Seven));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));
            
            //kolor
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Two));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Jack));
            
            //kareta
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Ace));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Ace));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player4);


            Assert.AreEqual(winners, winnersShouldBe);
        }

        [Test]
        public void DetermineWinner_Full_ReturnsListOfWinner()//Full
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Queen));
            
            //high card
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Three));
            
            //2 pary
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Four));
            
            //full
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Ten));
            
            //trojka
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Ten));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Three));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player3);


            Assert.AreEqual(winners, winnersShouldBe);
        }

        [Test]
        public void DetermineWinner_Colour_ReturnsListOfWinner()//Kolor
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Five));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Ace));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Jack));
            
            //para
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Jack));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            
            //strit
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Four));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Seven));
            
            //trojka
            player3.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Jack));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));
            
            //kolor
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Three));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ten));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player4);


            Assert.AreEqual(winners, winnersShouldBe);
        }

        [Test]
        public void DetermineWinner_Straight_ReturnsListOfWinner()//Strit
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Five));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Jack));
            
            //para
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Three));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Two));
            
            //trojka
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Eight));
            
            //2 pary
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Five));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));
            
            //strit
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Seven));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Nine));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player4);


            Assert.AreEqual(winners, winnersShouldBe);
        }

        [Test]
        public void DetermineWinner_ThreeOfKind_ReturnsListOfWinner()//Trojka
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Nine));
            
            //para
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Eight));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));
            
            //para
            player2.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Nine));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));
            
            //trojka
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Two));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Two));
            
            //2 pary
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Four));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Six));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player3);


            Assert.AreEqual(winners, winnersShouldBe);
        }

        [Test]
        public void DetermineWinner_TwoPairs_ReturnsListOfWinner()//Dwie pary
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Seven));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.King));
            
            //high card
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Two));
            
            //para
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Three));
            
            //2 pary
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.King));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));
            
            //1 para
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Four));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player3);


            Assert.AreEqual(winners, winnersShouldBe);
        }

        [Test]
        public void DetermineWinner_Pair_ReturnsListOfWinner()//Para
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            
            //high card
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Three));

            //high card
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.King));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Seven));

            //high card
            player3.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Queen));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Jack));
            
            //para
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Five));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player4);


            Assert.AreEqual(winners, winnersShouldBe);
        }

        [Test]
        public void DetermineWinner_HighestCard_ReturnsListOfWinner()//Najwyzsza karta
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            
            //high card
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.King));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Five));

            //high card
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Three));

            //high card
            player3.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Nine));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Five));

            //high card
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Ace));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Three));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player4);


            Assert.AreEqual(winners, winnersShouldBe);
        }

        [Test]
        public void DetermineWinner_EveryoneHasRoyalFlush_ReturnsListOfWinner()//Poker krolewski dla wszystkich
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.King));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Ten));
            
            //poker krolewski
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Nine));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));

            //poker krolewski
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Nine));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));

            //poker krolewski
            player3.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Seven));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Six));

            //poker krolewski
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Two));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Three));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player1);
            winnersShouldBe.Add(player3);
            winnersShouldBe.Add(player2);
            winnersShouldBe.Add(player4);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }

        [Test]
        public void DetermineWinner_DifferentPokers_ReturnsListOfWinner()//Rozne pokery
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Nine));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Seven));
            
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Nine));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Five));
            
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.King));
            
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Four));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Five));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player3);


            Assert.AreEqual(winners, winnersShouldBe);
        }

        [Test]
        public void DetermineWinner_DifferentFourOfKinds_ReturnsListOfWinner()//Rozne karety
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Five));
            
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Eight));
            
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.King));
            
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ten));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Ten));
            
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Queen));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Jack));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player3);


            Assert.AreEqual(winners, winnersShouldBe);
        }

        [Test]
        public void DetermineWinner_SameFourOfKindsWithSameHigh_ReturnsListOfWinner()//ta sama kareta, ten sam high
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Ten));
            
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Eight));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Ace));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.King));
            
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Nine));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Jack));
            
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Six));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player1);
            winnersShouldBe.Add(player2);
            winnersShouldBe.Add(player4);


            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }

        [Test]
        public void DetermineWinner_SameFourOfKindsWithDifferentHigh_ReturnsListOfWinner()//ta sama kareta, rozny high
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Nine));

            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Jack));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Seven));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Six));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.King));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Five));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player3);


            Assert.AreEqual(winners, winnersShouldBe);
        }
        [Test]
        public void DetermineWinner_SameFull_ReturnsListOfWinner()//Ten sam full
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Five));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Five));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Five));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Three));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Five));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Two));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Six));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player3);
            winnersShouldBe.Add(player4);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_DifferentFulls_ReturnsListOfWinner()//Rozne fulle
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Five));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Four));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Nine));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Five));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Six));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Four));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player3);


            Assert.AreEqual(winners, winnersShouldBe);
        }
        [Test]
        public void DetermineWinner_EveryoneHasColours_ReturnsListOfWinner()//Kolory, gdzie sprawdzamy najwyzsza karte
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Five));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Nine));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Three));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Four));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Three));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Four));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Four));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.King));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Six));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player3);


            Assert.AreEqual(winners, winnersShouldBe);
        }
        [Test]
        public void DetermineWinner_SameStraight_ReturnsListOfWinner()//ten sam strit
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Five));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Seven));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Eight));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Jack));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Ten));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Queen));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Jack));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.King));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Queen));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();

            winnersShouldBe.Add(player1);
            winnersShouldBe.Add(player2);
            winnersShouldBe.Add(player3);
            winnersShouldBe.Add(player4);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_StraightOnDifferentCards_ReturnsListOfWinner()//strity na roznych kartach
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Five));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Three));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Six));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Nine));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Jack));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Nine));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Jack));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Six));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Nine));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player2);
            winnersShouldBe.Add(player3);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_DifferentThreeOfKinds_ReturnsListOfWinner()//Trojki rozne
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Ten));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Jack));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Jack));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Queen));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Queen));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Seven));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Seven));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player3);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_SameThreeOfKindsWithSameHigh_ReturnsListOfWinner()//Trojki te same - highe te same
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Three));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Four));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Eight));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Ace));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Seven));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Ace));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player2);
            winnersShouldBe.Add(player4);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_SameThreeOfKindsWithDifferentHigh_ReturnsListOfWinner()//Trojki te same - highe rozne
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Three));

            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.King));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Ace));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Queen));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Five));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Jack));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Six));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player1);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_TwoDifferentPairs_ReturnsListOfWinner()//Rozne dwie pary
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.King));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Jack));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Seven));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Six));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Seven));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Six));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.King));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Ten));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Six));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player3);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_TwoSamePairsWithDifferentHigh_ReturnsListOfWinner()//te same dwie pary, rozne highe
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.King));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Two));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Ten));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Jack));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ten));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Jack));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.King));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.King));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player3);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_TwoSamePairsWithSameHigh_ReturnsListOfWinner()//te same dwie pary, ten sam high
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Seven));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Five));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Ace));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Seven));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Eight));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Five));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Two));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Five));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Two));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player1);
            winnersShouldBe.Add(player2);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_DifferentOnePairs_ReturnsListOfWinner()//rozne jedne pary
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Four));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Five));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Six));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Seven));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Two));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Jack));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Ten));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Queen));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player4);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_SamePairsWithThirdCardDifferent_ReturnsListOfWinner()//ta sama para, trzecia karta rozna
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Two));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Ace));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Two));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.King));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Five));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Three));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player1);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_SamePair_FourthCardDifferent_ReturnsListOfWinner()//ta sama para, czwarta karta rozna
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));

            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Two));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Two));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Nine));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Five));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Jack));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Three));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player2);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_SamePairWithDifferentFifthCard_ReturnsListOfWinner()//ta sama para, piata karta rozna
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));

            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Two));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Five));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Two));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Jack));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.King));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player2);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_SamePairWithSameRemainingCards_ReturnsListOfWinner()//ta sama para, reszta ta sama
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));

            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Two));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Seven));

            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Two));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));

            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));

            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Jack));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.King));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player1);
            winnersShouldBe.Add(player2);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_SameHighWithSameRemainingCards_ReturnsListOfWinner()//high ten sam, reszta rozna
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ten));

            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Jack));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Ace));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.King));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Queen));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player2);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_SameTwoCardsWithDifferentRemainingCards_ReturnsListOfWinner()//dwie karty te same, reszta rozna
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Four));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.King));

            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Queen));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Ace));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Jack));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Jack));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Queen));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ace));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Ten));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player1);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_SameThreeCardsWithDifferentRemainingCards_ReturnsListOfWinner()//trzy karty te same, reszta rozna
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Two));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Three));
            
            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Ten));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Seven));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Nine));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Six));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Ten));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Five));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Eight));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Seven));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player1);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_SameFourCardsWithDifferentRemaingCards_ReturnsListOfWinner()//cztery karty te same, reszta rozna
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

           gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
           gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Queen));
           gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));
           gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Nine));
           gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Six));
           
           //
           player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Eight));
           player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Four));
           
           //
           player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Seven));
           player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Four));
           
           //
           player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));
           player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Four));
           
           //
           player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Five));
           player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Four));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player1);
            winnersShouldBe.Add(player3);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
        [Test]
        public void DetermineWinner_HighestCardWithSameEveryCards_ReturnsListOfWinner()//najwyzsza karta - wszystkie te same
        {
            Player player1 = new HumanPlayer("First player", PlayerType.Human);
            GameTable gameTable = new GameTable("Table#1", (HumanPlayer)player1);
            Player player2 = new HumanPlayer("Second player", PlayerType.Human);
            Player player3 = new HumanPlayer("Third player", PlayerType.Human);
            Player player4 = new HumanPlayer("Fourth player", PlayerType.Human);

            gameTable.Players.Add(player2);
            gameTable.Players.Add(player3);
            gameTable.Players.Add(player4);

            GameplayController controller = new GameplayController(gameTable, new TexasHoldemDealer());

            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Ace));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Spade, CardValue.Queen));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Heart, CardValue.Jack));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Diamond, CardValue.Nine));
            gameTable.shownHelpingCards.Cards.Add(new Card(CardSign.Club, CardValue.Six));

            //
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Seven));
            player1.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Two));
            
            //
            player2.PlayerHand.Cards.Add(new Card(CardSign.Club, CardValue.Eight));
            player2.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Three));
            
            //
            player3.PlayerHand.Cards.Add(new Card(CardSign.Spade, CardValue.Eight));
            player3.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Four));
            
            //
            player4.PlayerHand.Cards.Add(new Card(CardSign.Heart, CardValue.Eight));
            player4.PlayerHand.Cards.Add(new Card(CardSign.Diamond, CardValue.Five));


            List<Player> winners = controller.determineWinner();
            List<Player> winnersShouldBe = new List<Player>();
            winnersShouldBe.Add(player2);
            winnersShouldBe.Add(player3);
            winnersShouldBe.Add(player4);

            Assert.IsTrue(IsSame(winners, winnersShouldBe));
        }
    }
}