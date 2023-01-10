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
            foreach (Player player in gameTable.Players)
            {
                player.PlayerHand.AddCard(deck.Cards[cardNumber]);
                player.PlayerHand.AddCard(deck.Cards[cardNumber + 1]);
                cardNumber += 2;
            }
            for(; cardNumber<7; cardNumber++)
            {
                helpingCards.AddCard(deck.Cards[cardNumber]);
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
            determineWinner();
        }
        public void determineWinner()
        {
            int biggest_score = 11;//gorzej niz najwyzsza karta
            Player winner;
            foreach (Player player in gameTable.Players)
            {
                //TODO
                Card PlayerCards[7];// Karty gracza i 5 wspolnych lezacych na stole
                //TODO
                //posortuj karty
                if (valueOfCards(PlayerCards) <= biggest_score)
                {
                    winner = player;
                }
            }
        }
        //kazda funkcja musi dostac posortowane 7 kart od najwyzszej - As do najnizszej
        public bool isRoyalFlush(Card Cards[7])//Krolewski
        {
            if (Cards[0].CardValue == 14)
            {
                if (Cards[1].CardValue == 13 && Cards[1].CardSign == Cards[0].CardSign)
                {
                    if (Cards[2].CardValue == 12 && Cards[2].CardSign == Cards[0].CardSign)
                    {
                        if (Cards[3].CardValue == 11 && Cards[3].CardSign == Cards[0].CardSign)
                        {
                            if (Cards[4].CardValue == 10 && Cards[4].CardSign == Cards[0].CardSign)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool isStraigthFlush(Card Cards[7])//Poker
        {
            for (int i = 0; i < 3; i++)
            {
                int valueOfTheBiggest = Cards[i].CardValue;
                if (Cards[i + 1].CardValue == valueOfTheBiggest - 1 && Cards[i + 1].CardSign == Cards[i].CardSign)
                {
                    if (Cards[i + 2].CardValue == valueOfTheBiggest - 2 && Cards[i + 2].CardSign == Cards[i].CardSign)
                    {
                        if (Cards[i + 3].CardValue == valueOfTheBiggest - 3 && Cards[i + 3].CardSign == Cards[i].CardSign)
                        {
                            if (Cards[i + 4].CardValue == valueOfTheBiggest - 4 && Cards[i + 4].CardSign == Cards[i].CardSign)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool isQuads(Card Cards[7])//Kareta
        {
            for (int i = 0; i < 4; i++)
            {
                int quadsValue = Cards[i].CardValue;
                if (Cards[i + 1].CardValue == quadsValue)
                {
                    if (Cards[i + 2].CardValue == quadsValue)
                    {
                        if (Cards[i + 3].CardValue == quadsValue)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool isFullHouse(Card Cards[7])//Full
        {
            int counterPair = 0;
            int counterThree = 0;
            for (int i = 0; i < 6; i++)
            {
                if (counterThree != 1)
                {
                    if (Cards[i].CardValue == Cards[i + 1].CardValue)
                    {
                        if (Cards[i].CardValue == Cards[i + 2].CardValue)
                        {
                            counterThree += 1;
                            i += 2;
                        }
                    }
                }
                if (Cards[i].CardValue == Cards[i + 1].CardValue)
                {
                    counterPairs += 1;
                    i += 1;
                }
            }
            if (counterThree == 1 && counterPairs != 0)
            {
                return true;
            }
            return false;
        }
        public bool isFlush(Card Cards[7])//Kolor
        {
            int HeartCounter = 0;
            int SpadeCounter = 0;
            int DiamondCounter = 0;
            int ClubCounter = 0;
            for (int i = 0; i < 7; i++)
            {
                if (Cards[i].CardValue == 0)
                {
                    HeartCounter++;
                }
                else if (Cards[i].CardValue == 1)
                {
                    SpadeCounter++;
                }
                else if (Cards[i].CardValue == 2)
                {
                    DiamondCounter++;
                }
                else if (Cards[i].CardValue == 3)
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
        public bool isStraight(Card Cards[7])//Strit
        {
            for (int i = 0; i < 3; i++)
            {
                int valueOfTheBiggest = Cards[i].CardValue;
                if (Cards[i + 1].CardValue == valueOfTheBiggest - 1)
                {
                    if (Cards[i + 2].CardValue == valueOfTheBiggest - 2)
                    {
                        if (Cards[i + 3].CardValue == valueOfTheBiggest - 3)
                        {
                            if (Cards[i + 4].CardValue == valueOfTheBiggest - 4)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool isThreeOfKind(Card Cards[7])//Trojka
        {
            for (int i = 0; i < 5; i++)
            {
                int valueOfTheThree = Cards[i].CardValue;
                if (Cards[i + 1].CardValue == valueOfTheBiggest - 1)
                {
                    if (Cards[i + 2].CardValue == valueOfTheBiggest - 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool isTwoPairs(Card Cards[7])
        {
            int counterPairs = 0;
            for (int i = 0; i < 6; i++)
            {
                if (Cards[i].CardValue == Cards[i + 1].CardValue)
                {
                    counterPairs += 1;
                    i += 1;
                }
            }
            if (counterPairs >= 2)
            {
                return true;
            }
            return false;
        }
        public bool isOnePair(Card Cards[7])
        {
            for (int i = 0; i < 6; i++)
            {
                if (Cards[i].CardValue == Cards[i + 1].CardValue)
                {
                    return true;
                }
            }
            return false;
        }

        public int valueOfCards(Card Cards[7])){
            if (isRoyalFlush(Cards)) {
                return 1;
            }
            else if isStraigthFlush(Cards)
            {
                return 2;
            }
            else if isQuads(Cards)
            {
                return 3;
            }
            else if isFullHouse(Cards)
            {
                return 4;
            }
            else if isFlush(Cards)
            {
                return 5;
            }
            else if isStraight(Cards)
            {
                return 6;
            }
            else if isThreeOfKind(Cards)
            {
                return 7;
            }
            else if isTwoPairs(Cards)
            {
                return 8;
            }
            else if isOnePair(Cards)
            {
                return 9;
            }
            else {
                return 10;// zawsze ma sie jakas najwyzsza karte
            }
        }

    }
}
