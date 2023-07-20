﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameClasses
{
    public enum GameMode
    {
        No_Bots,
        You_And_Bots,
        Mixed
    }
    public class GameTableSettings
    {
        public const int MaxPlayersCountByRules = 9;
        public const int MinPlayersCountByRules = 2;
        public GameMode Mode
        { get; set; }
        public int MaxPlayersCountInGame
        { get; set; }

        // (note: BotsCount != BotsNumberOnStart)
        // bots count is dynamic and changes depending on how many bots sits at the table,
        // and bots number on start is just an initial setting
        public int BotsNumberOnStart
        { get; set; }
        public int MinPlayersXP
        { get; set; }
        public int BigBlind
        { get; private set; }
        public int MinTokens { get; private set; }
        public GameTableSettings()
        {
            this.Mode = GameMode.Mixed;
            this.MaxPlayersCountInGame = MaxPlayersCountByRules;
            this.BotsNumberOnStart = 0;
            this.MinPlayersXP = 0;
            this.BigBlind = 0;
            this.MinTokens = 0;
        }

        override public string ToString()
        {
            return "Game mode: " + Enum.GetName(typeof(GameMode), this.Mode) + "\n"
                + "Max player count: " + this.MaxPlayersCountInGame + "\n"
                + "Bots starting number: " + this.BotsNumberOnStart + "\n"
                + "Min XP: " + this.MinPlayersXP + "\n"
                + "Min Chips: " + this.MinTokens + "\n"
                + "Big Blind: " + this.BigBlind + "\n";
        }

        public bool changeMode(GameMode mode)
        {
            this.Mode = mode;
            if (mode == GameMode.No_Bots)
                this.BotsNumberOnStart = 0;
            
            if (mode == GameMode.You_And_Bots)
                this.BotsNumberOnStart = MinPlayersCountByRules - 1;

            return true;
        }

        public bool changeMaxPlayers(int maxPlayers)
        {
            if (maxPlayers < MinPlayersCountByRules)
            {
                this.MaxPlayersCountInGame = MinPlayersCountByRules;
                return true;
            }

            if (maxPlayers > MaxPlayersCountByRules)
            {
                this.MaxPlayersCountInGame = MaxPlayersCountByRules;
                return true;
            }

            this.MaxPlayersCountInGame = maxPlayers;
            return true;
        }

        public bool changeBotsNumber(int botsNumber)
        {
            if (this.Mode == GameMode.No_Bots)
                return false;

            if (botsNumber < 0)
            {
                if (this.Mode == GameMode.You_And_Bots)
                    this.BotsNumberOnStart = MinPlayersCountByRules - 1;
                else
                    this.BotsNumberOnStart = 0;

                return true;
            }

            if (botsNumber > MaxPlayersCountByRules - 1)
            {
                this.BotsNumberOnStart = MaxPlayersCountByRules - 1;
                return true;
            }

            this.BotsNumberOnStart = botsNumber;
            return true;
        }

        public bool changeMinXP(int minXP)
        {
            this.MinPlayersXP = minXP;
            return true;
        }

        public bool changeMinTokens(int minTokens)
        {
            this.MinTokens = minTokens;
            return true;
        }
        public bool changeBigBlind(int bigBlind)
        {
            this.BigBlind = bigBlind;
            return true;
        }
    }
}
