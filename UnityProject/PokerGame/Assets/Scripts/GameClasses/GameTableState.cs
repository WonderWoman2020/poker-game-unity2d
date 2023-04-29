using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;



namespace PokerGameClasses
{
    public class GameTableState
    {
        public string Name { get; set; }
        public CardsCollection Cards { get; set; }
        public int TokensInGame { get; set; }
        public int CurrentBid { get; set; }

        public GameTableState()
        {
            this.Name = null;
            this.Cards = null;
            this.TokensInGame = 0;
            this.CurrentBid = 0;
        }

        public GameTableState(string name, CardsCollection cards, int tokensInGame, int currentBid)
        {
            this.Name = name;
            this.Cards = cards;
            this.TokensInGame = tokensInGame;
            this.CurrentBid = currentBid;
        }

        public void UnpackGameState(string[] splitted)
        {
            string[] tableState = splitted[1].Split(new string(":"));
            this.Name = tableState[2];
            string cards = tableState[4];
            CardsCollection cardsCollection = CardsHelper.StringToCardsCollection(cards);
            this.Cards = cardsCollection;
            this.TokensInGame = Convert.ToInt32(tableState[6]);
            this.CurrentBid = Convert.ToInt32(tableState[8]);
        }

        public override string ToString()
        {
            return "Table's '" + this.Name + "' game state:" +
                "\nCards: " + this.Cards +
                "\nTokens in game: " + this.TokensInGame +
                "\nCurrent Bid: " + this.CurrentBid + "\n";
        }
    }
}
