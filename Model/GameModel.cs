﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace WindowsForm.Model
{
    public class GameModel
    {
        public event Action StateChanged;
        public event Action TheGameIsOver;
        public List<Bot> ArmyOfBots { get; private set; }
        public Playground Map { get; private set; }
        public Player Player { get; private set; }
        private int numberOfBots;
        public int NumberOfActiveBots { get; private set; }

        public GameModel(Playground map)
        {
            Map = map;
            var playerLocation = FindAPositionToCreateAnOject();
            Map[playerLocation] = new Player(90, playerLocation);
            Player = (Player)Map[playerLocation];
            ArmyOfBots = new List<Bot>();
        }

        public List<GameObjects>[,] GetCandidatesPerLocation()
        {
            var creatures = new List<GameObjects>[Map.Width, Map.Height];

            for (var x = 0; x < Map.Width; x++)
                for (var y = 0; y < Map.Height; y++)
                    creatures[x, y] = new List<GameObjects>();

            for (var x = 0; x < Map.Width; x++)
                for (var y = 0; y < Map.Height; y++)
                {
                    var targetLogicalLocation = Map[x, y].Location + Map[x, y].Delta;
                    creatures[targetLogicalLocation.X, targetLogicalLocation.Y].Add(Map[x, y]);
                }

            return creatures;
        }

        public void ExecuteTheCommandsOfTheHeroes()
        {
            var creaturesPerLocation = GetCandidatesPerLocation();

            for (var x = 0; x < Map.Width; x++)
                for (var y = 0; y < Map.Height; y++)
                {
                    Map[x, y] = SelectWinnerCandidatePerLocation(creaturesPerLocation, x, y);
                    Map[x, y].CommandAreExecuted(x, y);
                }

            StateChanged();
            if (Player != Map[Player.Location]) TheGameIsOver();
        }

        private static GameObjects SelectWinnerCandidatePerLocation(List<GameObjects>[,] creatures, int x, int y)
        {
            var sortedСreatures = creatures[x, y].OrderBy(creature => creature.Priority);

            return sortedСreatures.Any() ? sortedСreatures.Last() : new Stone(new Point(x, y));
        }

        public void CreateABot()
        {
            var location = FindAPositionToCreateAnOject();
            Map[location] = new Bot(location);
            ArmyOfBots.Add((Bot)Map[location]);
            numberOfBots++;
        }

        public void SetTheBotsInMotion(GameModel model)
        {
            ArmyOfBots = ArmyOfBots
                .Where(bot => bot == Map[bot.Location])
                .ToList();

            NumberOfActiveBots = numberOfBots - ArmyOfBots.Count;

            foreach (var bot in ArmyOfBots)
                bot.MakeAMove(model);
        }

        private Point FindAPositionToCreateAnOject()
        {
            var random = new Random();
            (var x, var y) = (int.MaxValue, int.MaxValue);

            while (!Map.InBounds(new Point(x, y)) || !(Map[x, y] is Stone) || Player != null && Math.Abs(Player.Location.X - x) > 4 && Math.Abs(Player.Location.Y - y) > 4)
                (x, y) = (random.Next(1, Map.Width - 1), random.Next(1, Map.Height - 1));

            return new Point(x, y);
        }
    }
}