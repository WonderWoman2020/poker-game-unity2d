using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    public class TexasHoldemDealer : ICardsDealer
    {
        public CardsCollection Deck 
        { get; set; }
        public int Position 
        { get; set; }

        public TexasHoldemDealer()
        {
            this.Deck = null;
            this.Position = -1;
        }
        public void CreateDeck()
        {
            List<Card> deck = new List<Card>(52);

            int index = 0;
            foreach (CardValue cardVal in Enum.GetValues(typeof(CardValue)))
            {
                foreach (CardSign cardSign in Enum.GetValues(typeof(CardSign)))
                {
                    deck.Add(new Card(cardSign, cardVal));
                    index++;
                }
            }
            this.Deck = new CardsCollection(deck);
        }
        public void ShuffleCards()
        {
            System.Random random = new System.Random();
            this.Deck.Cards = this.Deck.Cards.OrderBy(c => random.Next()).ToList();
        }
        public void DealCards(GameTable gameTable, int roundNr)
        {
            switch(roundNr)
            {
                case 0:
                    this.DealPreflop(gameTable);
                    break;
                case 1:
                    this.DealFlop(gameTable);
                    break;
                case 2:
                    this.DealTurn(gameTable);
                    break;
                case 3:
                    this.DealRiver(gameTable);
                    break;
                default:
                    break;
            }
        }

        public void TakeBackCards(GameTable gameTable)
        {
            foreach (Player player in gameTable.Players)
                player.PlayerHand = new CardsCollection();

            gameTable.shownHelpingCards = new CardsCollection();
            this.Deck = null;
        }

        private void DealPreflop(GameTable gameTable)
        {
            this.CreateDeck();
            this.ShuffleCards();

            foreach (Player player in gameTable.Players)
            {
                player.PlayerHand.AddCard(this.Deck.TakeOutCard(0));
                player.PlayerHand.AddCard(this.Deck.TakeOutCard(0));
            }
        }

        private void DealFlop(GameTable gameTable)
        {
            for (int i = 0; i < 3; i++)
            {
                gameTable.shownHelpingCards.AddCard(this.Deck.TakeOutCard(0));
            }
        }

        private void DealTurn(GameTable gameTable)
        {
            gameTable.shownHelpingCards.AddCard(this.Deck.TakeOutCard(0));
        }
        private void DealRiver(GameTable gameTable)
        {
            gameTable.shownHelpingCards.AddCard(this.Deck.TakeOutCard(0));
        }

        public void ChangePosition(GameTable gameTable)
        {
            this.Position++;
            this.Position = this.Position % gameTable.Players.Count;
        }
    }
}
