﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Elf
{
    public class Round
    {
        public Round(string playerOne, string playerTwo)
        {
            Player1 = GetFromStr(playerOne);
            Player2 = playerTwo.ToLower() switch
            {
                "x" => Player1 switch
                {
                    RPS.Paper => RPS.Rock,
                    RPS.Rock => RPS.Scissors,
                    _ => RPS.Paper
                },
                "y" => Player1 switch
                {
                    RPS.Paper => RPS.Paper,
                    RPS.Rock => RPS.Rock,
                    _ => RPS.Scissors
                },
                "z" => Player1 switch
                {
                    RPS.Paper => RPS.Scissors,
                    RPS.Rock => RPS.Paper,
                    _ => RPS.Rock
                },
                _ => Player2
            };
        }

        private static RPS GetFromStr(string a)
        {
            switch (a.ToLower())
            {
                case "a":
                case "x":
                    return RPS.Rock;
                case "b":
                case "y":
                    return RPS.Paper;
                case "c":
                case "z":
                    return RPS.Scissors;
            }

            throw new NotImplementedException();
        }

        private RPS Player1 { get; set; }
        private RPS Player2 { get; set; }

        public int Score => (int) Player2 + (int) WLD();

        private WLD WLD()
        {
            switch (Player1)
            {
                case RPS.Rock:
                    switch (Player2)
                    {
                        case RPS.Paper:
                            return Elf.WLD.Win;
                        case RPS.Scissors:
                            return Elf.WLD.Loose;
                    }

                    break;
                case RPS.Paper:
                    switch (Player2)
                    {
                        case RPS.Rock:
                            return Elf.WLD.Loose;
                        case RPS.Scissors:
                            return Elf.WLD.Win;
                    }

                    break;
                case RPS.Scissors:
                    switch (Player2)
                    {
                        case RPS.Rock:
                            return Elf.WLD.Win;
                        case RPS.Paper:
                            return Elf.WLD.Loose;
                    }

                    break;
            }

            return Elf.WLD.Draw;
        }
    }

    public enum WLD
    {
        Win = 6,
        Loose = 0,
        Draw = 3
    }

    public enum RPS
    {
        Rock = 1,
        Paper = 2,
        Scissors = 3
    }


    class Program
    {
        static async Task Main(string[] args)
        {
            await Day9();
        }

        public class RawPoint
        {
            public RawPoint(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; set; }
            public int Y { get; set; }
        }


        public async static Task Day9()
        {
            const int size = 3000;
            const int knots = 9;
            var grid = Enumerable.Range(0, size).Select(x => Enumerable.Range(0, size).Select(x => 0).ToArray())
                .ToArray();
            
            var input =File.ReadLines("/home/mkb/input.txt");
            var head = new Point(size/2,size/2);
            var tails = Enumerable.Range(0, knots).Select(w => new RawPoint(head.X,head.Y)).ToList();
            grid[head.X][head.Y] += 1;
            foreach (var line in input)
            {
                var sp = line.Split(" ");
                var dir = sp.First();
                for (int i = 0; i < int.Parse(sp.Last()); i++)
                {
                    var start = new Point(head.X, head.Y);
                    switch (dir)
                    {
                        case "U":
                            head.Y -= 1;
                            break;
                        case "R":
                            head.X += 1;
                            break;
                        case "D":
                            head.Y += 1;
                            break;
                        case "L":
                            head.X -= 1;
                            break;
                    }

                    var current = start;
                    var sHead = head;
                    foreach (var tail in tails)
                    {
                        if (current == sHead) continue;
                        var nextCurrent = new Point(tail.X, tail.Y);
                        var distanceX = sHead.X - tail.X;
                        var distanceY = sHead.Y - tail.Y;
                        if ((Math.Abs(distanceX) < 2 && Math.Abs(distanceY) < 2)) break;
                        
                        tail.X += Math.Sign(distanceX);
                        tail.Y += Math.Sign(distanceY);
                        if (tails.Last() == tail)
                        {
                            grid[tail.X][tail.Y] += 1;
                        }

                        sHead = new Point(tail.X, tail.Y);
                        current = nextCurrent;
                    }
                }
            }

            var count = grid.SelectMany(x => x.Where(t => t > 0)).Count();
            Console.WriteLine(count);
        }

        private static void Draw(int size, List<RawPoint> points, Point point)
        {
            var array = Enumerable.Range(0, size).Select(x => Enumerable.Range(0, size).Select(x => '.').ToArray())
                .ToArray();

            array[point.Y][point.X] = 'H';
            for (int i = 0; i < points.Count; i++)
            {
                array[points[i].Y][points[i].X] = i.ToString().First();
            }

            foreach (var item in array)
            {
                Console.WriteLine(string.Join("",item));
            }
        }


        public static void Day8LinqForLinqSake()
        {
            var grid = File.ReadAllLines("/home/mkb/input.txt")
                .Select(t => t.Select(x => int.Parse(x.ToString())).ToList()).ToList();
            int score = 0;

            int CountFirstOfMyHeightOrBigger(IList<int> items, int value) => items.Any(x => x >= value)
                ? items.IndexOf(items.FirstOrDefault(x => x >= value)) + 1
                : items.Count;

            var visible = Enumerable.Range(0, grid.Count).Select(i => Enumerable.Range(0, grid[i].Count).Select(j =>
            {
                var leftOfMe = grid[i].Take(j).Reverse().ToArray();
                var rightOfMe = grid[i].Skip(j + 1).ToArray();
                var aboveMe = grid.Select(x => x[j]).Take(i).Reverse().ToArray();
                var belowMe = grid.Select(x => x[j]).Skip(i + 1).ToArray();
                score = Math.Max(score, CountFirstOfMyHeightOrBigger(leftOfMe, grid[i][j]) *
                                        CountFirstOfMyHeightOrBigger(aboveMe, grid[i][j]) *
                                        CountFirstOfMyHeightOrBigger(belowMe, grid[i][j]) *
                                        CountFirstOfMyHeightOrBigger(rightOfMe, grid[i][j]));
                return !(leftOfMe.Any(x => x >= grid[i][j]) && rightOfMe.Any(x => x >= grid[i][j]) &&
                         aboveMe.Any(x => x >= grid[i][j]) && belowMe.Any(x => x >= grid[i][j]));
            })).Sum(x => x.Count(e => e));

            Console.WriteLine(visible);
            Console.WriteLine(score);
        }

        public static void Day8()
        {
            var grid = File.ReadAllLines("/home/mkb/input.txt")
                .Select(t => t.Select(x => int.Parse(x.ToString())).ToList()).ToList();
            int visible = 0, score = 0;

            int CountFirstOfMyHeightOrBigger(IList<int> items, int value) => items.Any(x => x >= value)
                ? items.IndexOf(items.FirstOrDefault(x => x >= value)) + 1
                : items.Count;

            for (var i = 0; i < grid.Count; i++)
            {
                for (var j = 0; j < grid[i].Count; j++)
                {
                    var leftOfMe = grid[i].Take(j).Reverse().ToArray();
                    var rightOfMe = grid[i].Skip(j + 1).ToArray();
                    var aboveMe = grid.Select(x => x[j]).Take(i).Reverse().ToArray();
                    var belowMe = grid.Select(x => x[j]).Skip(i + 1).ToArray();

                    if (!(leftOfMe.Any(x => x >= grid[i][j]) && rightOfMe.Any(x => x >= grid[i][j]) &&
                          aboveMe.Any(x => x >= grid[i][j]) && belowMe.Any(x => x >= grid[i][j]))) visible++;


                    score = Math.Max(score, CountFirstOfMyHeightOrBigger(leftOfMe, grid[i][j]) *
                                            CountFirstOfMyHeightOrBigger(aboveMe, grid[i][j]) *
                                            CountFirstOfMyHeightOrBigger(belowMe, grid[i][j]) *
                                            CountFirstOfMyHeightOrBigger(rightOfMe, grid[i][j]));
                }
            }

            Console.WriteLine(visible);
            Console.WriteLine(score);
        }

        public class Directory
        {
            public string Name { get; init; }
            public Dictionary<string, int> Files { get; } = new();
            public List<Directory> SubDirectories { get; } = new();
            public Directory Parent { get; init; }
            public long Size => Files.Sum(x => x.Value) + SubDirectories.Sum(s => s.Size);
        }

        public static void Day7()
        {
            var proceess = System.Diagnostics.Process.GetProcesses().Select(x => x.ProcessName);
            Console.Write(String.Join(Environment.NewLine, proceess));


            var files = System.IO.Directory.GetFiles(Environment.CurrentDirectory);


            var random = new Random();
            const int diceAmount = 2;
            const int diceSides = 6;
            var total = Enumerable.Range(0, 100 * 1000)
                .Select(x => Enumerable.Range(0, diceAmount)
                    .Select(x => random.Next(1, diceSides + 1)).Sum())
                .OrderBy(x => x).GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

            var output = string.Join(",", total.OrderByDescending(x => x.Value).Select(x => $"{x.Key} - {x.Value}"));

            var topLevel = new Directory
            {
                Name = "/",
            };
            var current = topLevel;
            var regex = new Regex("^[0-9]", RegexOptions.Compiled);
            foreach (var line in File.ReadAllLines("/home/mkb/input.txt").Skip(1))
            {
                if (regex.Match(line).Success)
                {
                    var split = line.Split(" ");
                    if (current.Files.ContainsKey(split.Last())) continue;
                    current.Files.Add(split.Last(), int.Parse(split.First()));
                    continue;
                }

                if (!line.StartsWith("$ cd")) continue;
                var command = line[4..].Trim();
                switch (command)
                {
                    case "/":
                        current = topLevel;
                        continue;
                    case "..":
                        current = current.Parent;
                        continue;
                }

                var sub = current.SubDirectories.FirstOrDefault(s => s.Name == command);
                if (sub is null)
                {
                    sub = new Directory {Name = command, Parent = current};
                    current.SubDirectories.Add(sub);
                }

                current = sub;
            }

            var all = Flatten(topLevel);
            var answer1 = all.Where(x => x.Size <= 100000).Sum(x => x.Size);
            var answer2 = all.Where(x => x.Size >= (30000000 - (70000000 - topLevel.Size))).OrderBy(x => x.Size).First()
                .Size;
        }

        static List<Directory> Flatten(Directory directory) => directory.SubDirectories
            .Concat(directory.SubDirectories.SelectMany(Flatten)).ToList();

        static void Day6()
        {
            var text = File.ReadAllText("/home/mkb/input#.txt");
            const int startMarker = 4; // change to 4 for part 1  , 14 for part 2
            Console.WriteLine(Enumerable.Range(0, text.Length)
                .First(i => text[i..(i + startMarker)].Distinct().Count() == startMarker) + startMarker);
        }

        static void Day5()
        {
            var stacks = Enumerable.Range(0, 9).Select(x => new List<char>()).ToArray();
            foreach (var line in File.ReadLines("/home/mkb/input.txt"))
            {
                if (line.Contains("["))
                {
                    foreach (var kv in line.Chunk(4)
                                 .Select((x) => x.Where(y => "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(y)))
                                 .Select((x, y) => new {Key = y, value = x})
                                 .Where(x => x.value.Any())) stacks[kv.Key].Add(kv.value.First());
                }

                if (!line.Contains("move")) continue;

                var parts = line.Replace("move", "").Replace("from", "").Replace("to", "").Split(' ')
                    .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                var amount = int.Parse(parts[0]);
                var from = int.Parse(parts[1]) - 1;
                var to = int.Parse(parts[2]) - 1;

                // Part 1
                // while (amount > 0)
                // {
                //     var item = stacks[from].First();
                //     stacks[from] = stacks[from].Skip(1).ToList();
                //     stacks[to] = new[] {item}.Concat(stacks[to]).ToList();
                //     amount--;
                // }

                var item = stacks[from].Take(amount).ToArray();
                stacks[from] = stacks[from].Skip(amount).ToList();
                stacks[to] = item.Concat(stacks[to]).ToList();
            }

            Console.WriteLine($"{string.Join(",", stacks.Select(x => x.First()))}");
        }

        static void Day4()
        {
            var answer1 = File.ReadLines("/home/mkb/input.txt").Select(x => x.Split(",").Select(x => x.Split('-'))
                    .Select(x => new {start = int.Parse(x.First()), stop = int.Parse(x.Last())})
                    .Select(x => Enumerable.Range(x.start, x.stop - x.start + 1).ToArray()))
                .Where(x => x.First().Union(x.Last()).Count() == Math.Max(x.First().Length, x.Last().Length)).ToArray();

            var answer2 = File.ReadLines("/home/mkb/input.txt").Select(x => x.Split(",").Select(x => x.Split('-'))
                    .Select(x => new {start = int.Parse(x.First()), stop = int.Parse(x.Last())})
                    .Select(x => Enumerable.Range(x.start, x.stop - x.start + 1).ToArray()))
                .Where(x => x.First().Union(x.Last()).Count() != x.First().Length + x.Last().Length).ToArray();
        }

        static void Day3()
        {
            var values = ("abcdefghijklmnopqrstuvwxyz" + "abcdefghijklmnopqrstuvwxyz".ToUpper())
                .Select((x, index) => new {index, x}).ToDictionary(x => x.x, x => x.index + 1);

            var parts1 = File.ReadLines("/home/mkb/input.txt")
                .Select(x => new {p1 = x[..(x.Length / 2)], p2 = x[(x.Length / 2)..]})
                .Select(w => values[(w.p1.Intersect(w.p2).First())]).Sum();

            var parts = File.ReadLines("/home/mkb/input.txt")
                .Chunk(3)
                .Select(w => values[(w.First().Intersect(w[1]).Intersect(w.Last()).First())]).Sum();
        }

        static void Day2()
        {
            var total = File.ReadLines("/home/mkb/input.txt")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Split(" "))
                .Select(t => new Round(t.First(), t.Last()).Score)
                .Sum();

            Console.WriteLine(total);
        }

        static void TinyDay2()
        {
            var total = File.ReadLines("/home/mkb/input.txt")
                .Select(e => new {Me = e[2] - 87, Com = e[0] - 64})
                .Select(e =>
                    e.Me + (e.Com == e.Me ? 3 :
                        e.Me < e.Com ? e.Me == 1 && e.Com == 3 ? 6 : 0 :
                        e.Me == 3 && e.Com == 1 ? 0 : 6));

            Console.WriteLine(total.Sum());
        }


        static void Day1()
        {
            var lines = File.ReadAllText("/home/mkb/Input01.txt")
                .Split("\r\n\r\n")
                .Select(x => x.Split("\r\n").Select(x => int.Parse(x == string.Empty ? "0" : x)).Sum())
                .OrderByDescending(r => r)
                .ToList();

            Console.WriteLine(lines.Take(1).Sum());
            Console.WriteLine(lines.Take(3).Sum());
        }
    }
}