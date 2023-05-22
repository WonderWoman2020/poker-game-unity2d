using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    class HandsComparer : IComparer<CardsCollection>
    {
        // TODO zaimplementować to jako Comparer i zrobić refactor
        public int Compare(CardsCollection x, CardsCollection y)
        {
            throw new NotImplementedException();
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
        public Card HighestCardOfPoker(CardsCollection Cards)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Cards.Cards[i].Value == Cards.Cards[i + 1].Value - 1 && Cards.Cards[i].Sign == Cards.Cards[i + 1].Sign)
                {
                    if (Cards.Cards[i + 1].Value == Cards.Cards[i + 2].Value - 1 && Cards.Cards[i + 1].Sign == Cards.Cards[i + 2].Sign)
                    {
                        if (Cards.Cards[i + 2].Value == Cards.Cards[i + 3].Value - 2 && Cards.Cards[i + 2].Sign == Cards.Cards[i + 3].Sign)
                        {
                            if (Cards.Cards[i + 3].Value == Cards.Cards[i + 4].Value - 3 && Cards.Cards[i + 3].Sign == Cards.Cards[i + 4].Sign)
                            {
                                return Cards.Cards[i];
                            }
                        }
                    }
                }
            }
            return null;
        }
        public Card HighestCardOfStraigth(CardsCollection Cards)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Cards.Cards[i].Value == Cards.Cards[i + 1].Value - 1)
                {
                    if (Cards.Cards[i + 1].Value == Cards.Cards[i + 2].Value - 1)
                    {
                        if (Cards.Cards[i + 2].Value == Cards.Cards[i + 3].Value - 2)
                        {
                            if (Cards.Cards[i + 3].Value == Cards.Cards[i + 4].Value - 3)
                            {
                                return Cards.Cards[i];
                            }
                        }
                    }
                }
            }
            return null;
        }
        public Card GiveCardOfTree(CardsCollection Cards)
        {
            for (int i = 0; i < 5; i++)
            {
                if (Cards.Cards[i].Value == Cards.Cards[i + 1].Value)
                {
                    if (Cards.Cards[i + 1].Value == Cards.Cards[i + 2].Value)
                    {
                        return Cards.Cards[i];
                    }
                }
            }
            return null;
        }
        public Card GiveCardOfTwo(CardsCollection Cards)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Cards.Cards[i].Value == Cards.Cards[i + 1].Value)
                {
                    return Cards.Cards[i];
                }
            }
            return null;
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
        public Card CardOfQuads(CardsCollection Cards)
        {
            for (int i = 0; i < 4; i++)
            {
                if (Cards.Cards[i].Value == Cards.Cards[i + 1].Value
                    && Cards.Cards[i].Value == Cards.Cards[i + 2].Value
                    && Cards.Cards[i].Value == Cards.Cards[i + 3].Value)
                {
                    return Cards.Cards[i];
                }
            }
            return null;
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
                //dopisane, żeby nie wywalało, nwm czy wystarczy w logice gry
                if (i >= 5)
                    break;
                if (Cards.Cards[i].Value == Cards.Cards[i + 1].Value)
                {
                    counterPair += 1;
                    i += 1;
                }
                //dopisane, żeby nie wywalało, nwm czy wystarczy w logice gry
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

                //dopisane, żeby nie wywalało, nwm czy wystarczy w logice gry
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
            if (isRoyalFlush(Cards))
            {
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
            else
            {
                return 10;// zawsze ma sie jakas najwyzsza karte
            }
        }
    }
}
