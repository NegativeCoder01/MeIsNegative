﻿using System;
using System.IO;

namespace NeuralNetwork
{
    public class AIConfig
    {
        public int Num1, Num2, Num3;
        public int Distance1, Distance2, Distance3;
        public int Ran1, Ran2, Ran3;

        public static AIConfig LoadFromFile(string path)
        {
            var config = new AIConfig();

            if (!File.Exists(path))
                return config;

            var lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;

                var parts = trimmed.Split('=');
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var valueStr = parts[1].Trim();

                if (!int.TryParse(valueStr, out int value))
                    continue;

                switch (key)
                {
                    case "Num1": config.Num1 = value; break;
                    case "Num2": config.Num2 = value; break;
                    case "Num3": config.Num3 = value; break;
                    case "Distance1": config.Distance1 = value; break;
                    case "Distance2": config.Distance2 = value; break;
                    case "Distance3": config.Distance3 = value; break;
                    case "Ran1": config.Ran1 = value; break;
                    case "Ran2": config.Ran2 = value; break;
                    case "Ran3": config.Ran3 = value; break;
                }
            }

            return config;
        }

        public void SaveToFile(string path)
        {
            string[] lines =
            {
                $"Num1 = {Num1}",
                $"Num2 = {Num2}",
                $"Num3 = {Num3}",
                $"Distance1 = {Distance1}",
                $"Distance2 = {Distance2}",
                $"Distance3 = {Distance3}",
                $"Ran1 = {Ran1}",
                $"Ran2 = {Ran2}",
                $"Ran3 = {Ran3}"
            };

            File.WriteAllLines(path, lines);
        }
    }

    public class Program
    {
        public static void Main()
        {
            string configPath = "/workspaces/MeIsNegative/Csharp/Computer/GeneticNeuralNetwork/NumberSequencePredictor/project/var.conf";
            AIConfig config = AIConfig.LoadFromFile(configPath);

            Random rand = new Random();

            if (config.Distance1 == 0) config.Distance1 = int.MaxValue;
            if (config.Distance2 == 0) config.Distance2 = int.MaxValue;
            if (config.Distance3 == 0) config.Distance3 = int.MaxValue;

            do
            {
                if (config.Distance1 != 0)
                {
                    int min1 = Math.Min(config.Ran1, config.Num1);
                    int max1 = Math.Max(config.Ran1, config.Num1);
                    config.Ran1 = rand.Next(min1, max1 + 1);
                }

                if (config.Distance2 != 0)
                {
                    int min2 = Math.Min(config.Ran2, config.Num2);
                    int max2 = Math.Max(config.Ran2, config.Num2);
                    config.Ran2 = rand.Next(min2, max2 + 1);
                }

                if (config.Distance3 != 0)
                {
                    int min3 = Math.Min(config.Ran3, config.Num3);
                    int max3 = Math.Max(config.Ran3, config.Num3);
                    config.Ran3 = rand.Next(min3, max3 + 1);
                }
                
                if (config.Distance1 != 0)
                {
                    config.Ran1 = rand.Next(1, 51);
                }
                if (config.Distance2 != 0)
                {
                    config.Ran2 = rand.Next(1, 51);
                }
                if (config.Distance3 != 0)
                {
                    config.Ran3 = rand.Next(1, 51);
                }

                int newDistance1 = Math.Abs(config.Ran1 - config.Num1);
                int newDistance2 = Math.Abs(config.Ran2 - config.Num2);
                int newDistance3 = Math.Abs(config.Ran3 - config.Num3);

                if (newDistance1 < config.Distance1)
                {
                    config.Distance1 = newDistance1;
                }
                if (newDistance2 < config.Distance2)
                {
                    config.Distance2 = newDistance2;
                }
                if (newDistance3 < config.Distance3)
                {
                    config.Distance3 = newDistance3;
                }

                Console.WriteLine($"[{config.Ran1}, {config.Ran2}, {config.Ran3}]");

                config.SaveToFile(configPath);

            } while (config.Distance1 != 0 || config.Distance2 != 0 || config.Distance3 != 0);
        }
    }
}
