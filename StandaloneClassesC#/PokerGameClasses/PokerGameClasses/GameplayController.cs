using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    class GameplayController
    {
        public GameTable gameTable
        { get; set; }
        public CardsCollection deck;
        public CardsCollection helpingCards; //karty pomocnicze lezace na stole, ale niewidoczne dla graczy

        public GameplayController(GameTable gameTable)
        {
            this.gameTable = gameTable;
            this.deck = CardsCollection.CreateStandardDeck();
        }

        public void dealCards()
        {
            deck.ShuffleCards();
            int cardNumber = 0;
            foreach(Player player in gameTable.Players)
            {
                player.PlayerHand.AddCard(deck.Cards[cardNumber]);
                player.PlayerHand.AddCard(deck.Cards[cardNumber+1]);
                cardNumber+=2;
            }
            for(; cardNumber<7; cardNumber++)
            {
                helpingCards.AddCard(deck.Cards[cardNumber]);
            }
        }

        public void playTheGame()
        {
            for(int i=0; i<4; i++)
            {
                gameTable.makeTurn();
                if(gameTable.checkIfEveryoneFolded())
                {
                    break;
                }
                if(i==0)
                {
                    gameTable.shownHelpingCards.AddCard(helpingCards.Cards[0]);
                    gameTable.shownHelpingCards.AddCard(helpingCards.Cards[1]);
                    gameTable.shownHelpingCards.AddCard(helpingCards.Cards[2]);
                }
                else if(i==1)
                {
                    gameTable.shownHelpingCards.AddCard(helpingCards.Cards[3]);
                }
                else if(i==3)
                {
                    gameTable.shownHelpingCards.AddCard(helpingCards.Cards[4]);
                }
            }
            determineWinner();
        }

        public void determineWinner()
        {
            //TODO
        }

    }
}
