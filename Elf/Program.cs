using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
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
            Day11();
        }

        public static void Day11()
        {
            const int rounds = 10000; // part1 =20 , part2 = 10000
            const bool withRelief = false; // part1=true , part2=false
            var dictionary = File.ReadAllText("/home/mkb/input.txt").Split(Environment.NewLine + Environment.NewLine).Select(x => new Monkey(x))
                .GroupBy(x => x.Id)
                .ToDictionary(x => x.Key, x => x.First());

            var monkies = dictionary.Values;
            var magicNumber = withRelief ? 0 : monkies.Select(x => x.DivisibleBy).Aggregate((a, b) => (a == 0 ? 1 : a) * b);
            for (int i = 0; i < rounds; i++)
            {
                foreach (var monkey in monkies)
                {
                    monkey.Turn(dictionary, withRelief ? i1 => i1 / 3 : i1 => i1 % magicNumber);
                }
            }

            Console.WriteLine(string.Join(Environment.NewLine, monkies.Select(t => $"{t.Id} -- {t.Inspect}")));
            var monkeyBusiness = monkies.OrderByDescending(x => x.Inspect).Take(2)
                .Aggregate<Monkey, BigInteger>(1, (current, e) => current * e.Inspect);
            Console.WriteLine($"Monkey Buisness = {monkeyBusiness}");
            Console.Read();
        }


        public class Monkey
        {
            private List<long> Items { get; set; }
            public int Inspect = 0;
            public int Id { get; }
            public int DivisibleBy { get; }
            private int OnTrue { get; }
            private int OnFalse { get; }

            private Func<long, long> Operation { get; }


            public Monkey(string setup)
            {
                var lines = setup.Split(Environment.NewLine);
                Id = int.Parse(lines[0].Split(':').First().Last().ToString());
                Items = lines[1].Split(":").Last().Split(',').Select(long.Parse).ToList();
                var part = lines[2].Split("=").Last()[5..];
                var numberStr = part.Split(" ").Last();
                var num = numberStr.Contains("old") ? 0 : int.Parse(numberStr);
                Operation = part.First() == '+'
                    ? numberStr.Contains("old") ? i => i + i : i=> i + num  
                    : numberStr.Contains("old")? i => i * i :i=> i * num;
                
                DivisibleBy = int.Parse(lines[3].Split(" ").Last());
                OnTrue = int.Parse(lines[4].Split(" ").Last());
                OnFalse = int.Parse(lines[5].Split(" ").Last());
            }

            public void Turn(Dictionary<int, Monkey> monkeys, Func<long, long> relief)
            {
                foreach (var worry in Items.Select(Operation).Select(relief))
                {
                    monkeys[(worry % DivisibleBy == 0 ? OnTrue : OnFalse)].AddItem(worry);
                }

                Inspect += Items.Count;
                Items.Clear();
            }

            private void AddItem(long item)
            {
                Items.Add(item);
            }
        }

        public static void Day10()
        {
            const int MaxCycles = 240;
            const char BackGroundDot = ' '; // they say to use . but find it hard to read 
            const char LetterDot = '#';
            var cyclesToStopAt = Enumerable.Range(0, 6).Select(i => 20 + (i * 40)).ToHashSet();
            var matrix = Enumerable.Range(0, 6).Select(x => Enumerable.Range(0, 40).Select(t => BackGroundDot).ToArray()).ToArray();

            int registerX = 1, cycle = 0, sum = 0;

            void AddCycle()
            {
                int row = cycle / 40, col = cycle % 40;
                cycle++;
                if (cycle > MaxCycles) return;
                matrix[row][col] = Math.Abs(registerX - col) < 2 ? LetterDot : BackGroundDot;
                if (cyclesToStopAt.Contains(cycle)) sum += registerX * cycle;
            }

            foreach (var item in File.ReadLines("/home/mkb/input.txt"))
            {
                if (cycle > MaxCycles) break;

                if (item == "noop")
                {
                    AddCycle();
                    continue;
                }

                AddCycle();
                AddCycle();
                registerX += int.Parse(item.Split(" ").Last());
            }

            Console.WriteLine($"Part 1: {sum}");
            Console.WriteLine($"Part 2:");

            foreach (var item in matrix)
            {
                Console.WriteLine(string.Join("", item));
            }

            Console.Read();
        }

        public async static Task Day9()
        {
            var items = new[] {1, 2, 3, 4, 5, 6};

            const int knots = 1; // flip from 1 or 9 for part 1 and 2
            var hash = new HashSet<(int, int)>();
            var input = File.ReadLines("/home/mkb/input.txt");
            var head = new Point(0, 0);
            var tails = Enumerable.Range(0, knots).Select(w => new Point(head.X, head.Y)).ToList();
            hash.Add((head.X, head.Y));
            foreach (var line in input)
            {
                var sp = line.Split(" ");
                for (int i = 0; i < int.Parse(sp.Last()); i++)
                {
                    switch (sp.First())
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

                    var sHead = head;
                    for (var index = 0; index < knots; index++)
                    {
                        var tail = tails[index];
                        var nextCurrent = new Point(tail.X, tail.Y);
                        var distanceX = sHead.X - tail.X;
                        var distanceY = sHead.Y - tail.Y;
                        if (Math.Abs(distanceX) < 2 && Math.Abs(distanceY) < 2) break;

                        tail.X += Math.Sign(distanceX);
                        tail.Y += Math.Sign(distanceY);
                        tails[index] = tail;
                        if (index == knots - 1 && !hash.Contains((tail.X, tail.Y))) hash.Add((tail.X, tail.Y));
                        sHead = tail;
                        if (nextCurrent == sHead) break;
                    }
                }
            }

            Console.WriteLine(hash.Count);
            Console.Read();
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
            var answer1 = File.ReadLines("/home/mkb/input.txt").Select(x => x.Split(",").Select(e => e.Split('-'))
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