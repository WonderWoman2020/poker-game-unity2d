using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    public class GameplayController
    {
        /*public GameTable gameTable
        { get; set; }
        public CardsCollection deck;
        public CardsCollection helpingCards; //karty pomocnicze lezace na stole, ale niewidoczne dla graczy

        public GameplayController()
        {
        }
        public GameplayController(GameTable gameTable)
        {
            this.gameTable = gameTable;
            this.deck = CardsCollection.CreateStandardDeck();
            this.helpingCards = new CardsCollection();
        }


        public void dealCards()
        {
            deck.ShuffleCards();
            int cardNumber = 0;
            foreach (Player player in gameTable.Players)
            {
                player.PlayerHand.AddCard(deck.Cards[cardNumber]);
                player.PlayerHand.AddCard(deck.Cards[cardNumber + 1]);
                cardNumber += 2;
            }

            for(int i = 0; i< 7; i++)
            {
                helpingCards.AddCard(deck.Cards[cardNumber+i]);
            }
        }

        public void playTheGame()
        {
            for (int i = 0; i < 4; i++)
            {
                gameTable.makeTurn();
                if (gameTable.checkIfEveryoneFolded())
                {
                    break;
                }
                if (i == 0)
                {
                    gameTable.shownHelpingCards.AddCard(helpingCards.Cards[0]);
                    gameTable.shownHelpingCards.AddCard(helpingCards.Cards[1]);
                    gameTable.shownHelpingCards.AddCard(helpingCards.Cards[2]);
                }
                else if (i == 1)
                {
                    gameTable.shownHelpingCards.AddCard(helpingCards.Cards[3]);
                }
                else if (i == 3)
                {
                    gameTable.shownHelpingCards.AddCard(helpingCards.Cards[4]);
                }
            }
            
        }

        public void ConcludeGame()
        {
            Player winner = determineWinner();
            if (winner != null)
            {
                winner.TokensCount = winner.TokensCount + this.gameTable.TokensInGame;
                winner.XP = winner.XP + 100; // ew. do zmiany
            }
            this.ResetGame();
        }
        public Player determineWinner()
        {
            int biggest_score = 11;//gorzej niz najwyzsza karta
            Player winner = (this.gameTable.Players.Count > 0) ? this.gameTable.Players[0] : null;
            foreach (Player player in gameTable.Players)
            {
                // Karty gracza i 5 wspolnych lezacych na stole
                CardsCollection PlayerCards = CardsCollection.MergeTwoDecks(player.PlayerHand, gameTable.shownHelpingCards);
                CardsCollection SortedPlayerCards = CardsCollection.SortDesc(PlayerCards);
                if (valueOfCards(PlayerCards) <= biggest_score)
                {
                    winner = player;
                }
            }

            return winner;
        }

        public void ResetGame()
        {
            this.helpingCards = new CardsCollection();
            this.deck = CardsCollection.CreateStandardDeck();
            this.gameTable.ResetGameState();
        }

        //kazda funkcja musi dostac posortowane 7 kart od najwyzszej - As do najnizszej
        public bool isRoyalFlush(CardsCollection Cards)//Krolewski
        {
            if ((int)Cards.Cards[0].Value == 14)
            {
                if ((int)Cards.Cards[1].Value == 13 && Cards.Cards[1].Sign == Cards.Cards[0].Sign)
                {
                    if ((int)Cards.Cards[2].Value == 12 && Cards.Cards[2].Sign == Cards.Cards[0].Sign)
                    {
                        if ((int)Cards.Cards[3].Value == 11 && Cards.Cards[3].Sign == Cards.Cards[0].Sign)
                        {
                            if ((int)Cards.Cards[4].Value == 10 && Cards.Cards[4].Sign == Cards.Cards[0].Sign)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool isStraigthFlush(CardsCollection Cards)//Poker
        {
            for (int i = 0; i < 3; i++)
            {
                int valueOfTheBiggest = (int)Cards.Cards[i].Value;
                if ((int)Cards.Cards[i + 1].Value == valueOfTheBiggest - 1 && Cards.Cards[i + 1].Sign == Cards.Cards[i].Sign)
                {
                    if ((int)Cards.Cards[i + 2].Value == valueOfTheBiggest - 2 && Cards.Cards[i + 2].Sign == Cards.Cards[i].Sign)
                    {
                        if ((int)Cards.Cards[i + 3].Value == valueOfTheBiggest - 3 && Cards.Cards[i + 3].Sign == Cards.Cards[i].Sign)
                        {
                            if ((int)Cards.Cards[i + 4].Value == valueOfTheBiggest - 4 && Cards.Cards[i + 4].Sign == Cards.Cards[i].Sign)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool isQuads(CardsCollection Cards)//Kareta
        {
            for (int i = 0; i < 4; i++)
            {
                int quadsValue = (int)Cards.Cards[i].Value;
                if ((int)Cards.Cards[i + 1].Value == quadsValue)
                {
                    if ((int)Cards.Cards[i + 2].Value == quadsValue)
                    {
                        if ((int)Cards.Cards[i + 3].Value == quadsValue)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool isFullHouse(CardsCollection Cards)//Full
        {
            int counterPair = 0;
            int counterThree = 0;
            for (int i = 0; i < 5; i++)
            {
                if (counterThree != 1)
                {
                    if (Cards.Cards[i].Value == Cards.Cards[i + 1].Value)
                    {
                        if (Cards.Cards[i].Value == Cards.Cards[i + 2].Value)
                        {
                            counterThree += 1;
                            i += 2;
                        }
                    }
                }
                //dopisane, ¿eby nie wywala³o, nwm czy wystarczy w logice gry
                if (i >= 5)
                    break;
                if (Cards.Cards[i].Value == Cards.Cards[i + 1].Value)
                {
                    counterPair += 1;
                    i += 1;
                }
                //dopisane, ¿eby nie wywala³o, nwm czy wystarczy w logice gry
                if (i >= 5)
                    break;
            }
            if (counterThree == 1 && counterPair != 0)
            {
                return true;
            }
            return false;
        }
        public bool isFlush(CardsCollection Cards)//Kolor
        {
            int HeartCounter = 0;
            int SpadeCounter = 0;
            int DiamondCounter = 0;
            int ClubCounter = 0;
            for (int i = 0; i < 7; i++)
            {
                if ((int)Cards.Cards[i].Value == 0)
                {
                    HeartCounter++;
                }
                else if ((int)Cards.Cards[i].Value == 1)
                {
                    SpadeCounter++;
                }
                else if ((int)Cards.Cards[i].Value == 2)
                {
                    DiamondCounter++;
                }
                else if ((int)Cards.Cards[i].Value == 3)
                {
                    ClubCounter++;
                }
            }
            if (HeartCounter >= 4 || SpadeCounter >= 4 || DiamondCounter >= 4 || ClubCounter >= 4)
            {
                return true;
            }
            return false;
        }
        public bool isStraight(CardsCollection Cards)//Strit
        {
            for (int i = 0; i < 3; i++)
            {
                int valueOfTheBiggest = (int)Cards.Cards[i].Value;
                if ((int)Cards.Cards[i + 1].Value == valueOfTheBiggest - 1)
                {
                    if ((int)Cards.Cards[i + 2].Value == valueOfTheBiggest - 2)
                    {
                        if ((int)Cards.Cards[i + 3].Value == valueOfTheBiggest - 3)
                        {
                            if ((int)Cards.Cards[i + 4].Value == valueOfTheBiggest - 4)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool isThreeOfKind(CardsCollection Cards)//Trojka
        {
            for (int i = 0; i < 5; i++)
            {
                int valueOfTheThree = (int)Cards.Cards[i].Value;
                if ((int)Cards.Cards[i + 1].Value == valueOfTheThree)
                {
                    if ((int)Cards.Cards[i + 2].Value == valueOfTheThree)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool isTwoPairs(CardsCollection Cards)
        {
            int counterPairs = 0;
            for (int i = 0; i < 6; i++)
            {
                if (Cards.Cards[i].Value == Cards.Cards[i + 1].Value)
                {
                    counterPairs += 1;
                    i += 1;
                }

                //dopisane, ¿eby nie wywala³o, nwm czy wystarczy w logice gry
                if (i >= 6)
                    break;
            }
            if (counterPairs >= 2)
            {
                return true;
            }
            return false;
        }
        public bool isOnePair(CardsCollection Cards)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Cards.Cards[i].Value == Cards.Cards[i + 1].Value)
                {
                    return true;
                }
            }
            return false;
        }

        public int valueOfCards(CardsCollection Cards)
        {
            if (isRoyalFlush(Cards)) {
                return 1;
            }
            else if (isStraigthFlush(Cards))
            {
                return 2;
            }
            else if (isQuads(Cards))
            {
                return 3;
            }
            else if (isFullHouse(Cards))
            {
                return 4;
            }
            else if (isFlush(Cards))
            {
                return 5;
            }
            else if (isStraight(Cards))
            {
                return 6;
            }
            else if (isThreeOfKind(Cards))
            {
                return 7;
            }
            else if (isTwoPairs(Cards))
            {
                return 8;
            }
            else if (isOnePair(Cards))
            {
                return 9;
            }
            else {
                return 10;// zawsze ma sie jakas najwyzsza karte
            }
        }*/


}
}
