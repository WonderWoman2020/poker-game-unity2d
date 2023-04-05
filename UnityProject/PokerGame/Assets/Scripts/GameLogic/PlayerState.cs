using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System;
using System.Text;

using ClientSideCardsHelper;

namespace PokerGameClasses
{
    public class PlayerState
    {
        public string Nick { get; set; }
        public ClientSideCardsHelper.CardsCollection Hand { get; set; }
        public int TokensCount { get; set; }
        public int CurrentBet { get; set; }
        public int Xp { get; set; }

        public PlayerState()
        {
            this.Nick = null;
            this.Hand = null;
            this.TokensCount = 0;
            this.CurrentBet = 0;
            this.Xp = 0;
        }

        public PlayerState(string nick, ClientSideCardsHelper.CardsCollection hand, int tokensCount, int currentBet, int xp)
        {
            this.Nick = nick;
            this.Hand = hand;
            this.TokensCount = tokensCount;
            this.CurrentBet = currentBet;
            this.Xp = xp;
        }

        public void UnpackGameState(string[] splitted)
        {
            string[] playerState = splitted[1].Split(new string(":"));
            this.Nick = playerState[2];
            string hand = playerState[4];
            ClientSideCardsHelper.CardsCollection cardsCollection = ClientSideCardsHelper.CardsHelper.StringToCardsCollection(hand);
            this.Hand = cardsCollection;
            this.TokensCount = Convert.ToInt32(playerState[6]);
            this.CurrentBet = Convert.ToInt32(playerState[8]);
            this.Xp = Convert.ToInt32(playerState[10]);
            
        }

        public override string ToString()
        {
            return "Player's '" + this.Nick + "' game state:" +
                "\nHand: " + this.Hand +
                "\nTokens: " + this.TokensCount +
                "\nCurrent Bet: " + this.CurrentBet +
                "\nXP: " + this.Xp + "\n";
        }
    }
}
