using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode2019.Puzzles.Day12
{
    public class Puzzle2 : IPuzzle
    {
        public object Solve()
        {
            var xAxisCycle = GetCycle(moon => moon.Position.X, moon => moon.Velocity.X);
            var yAxisCycle = GetCycle(moon => moon.Position.Y, moon => moon.Velocity.Y);
            var zAxisCycle = GetCycle(moon => moon.Position.Z, moon => moon.Velocity.Z);

            var lcmInput = new List<long> { xAxisCycle, yAxisCycle, zAxisCycle };
            var numberOfSteps = Lcm(lcmInput);

            return numberOfSteps;
        }

        private int GetCycle(Func<Moon, int> getPosition, Func<Moon, int> getVelocity)
        {
            var allMoons = GetMoons().ToList();
            var moonPairings = GetPairings(allMoons).ToList();
            var cycle = RunCycle(allMoons, moonPairings, getPosition, getVelocity);

            return cycle;
        }

        private IEnumerable<Moon> GetMoons()
        {
            var lines = File.ReadAllLines("Puzzles\\Day12\\input.txt");

            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];

                yield return ParseLine(line, lineIndex);
            }
        }

        private static Moon ParseLine(string line, int lineIndex)
        {
            var regex = new Regex("<x=(.+), y=(.+), z=(.+)>");

            var match = regex.Match(line);

            var moon = new Moon(lineIndex);
            moon.Position.X = int.Parse(match.Groups[1].Value);
            moon.Position.Y = int.Parse(match.Groups[2].Value);
            moon.Position.Z = int.Parse(match.Groups[3].Value);

            return moon;
        }

        private IEnumerable<Tuple<Moon, Moon>> GetPairings(IReadOnlyList<Moon> allMoons)
        {
            for (var i = 0; i < allMoons.Count; i++)
            {
                for (var j = i + 1; j < allMoons.Count; j++)
                {
                    var moon1 = allMoons[i];
                    var moon2 = allMoons[j];

                    yield return new Tuple<Moon, Moon>(moon1, moon2);
                }
            }
        }

        private int RunCycle(
            IReadOnlyList<Moon> allMoons,
            IReadOnlyCollection<Tuple<Moon, Moon>> moonPairings,
            Func<Moon, int> getPosition,
            Func<Moon, int> getVelocity)
        {
            int index;

            // String not as fast as a 8-tuple but don't want to be hard coding lengths.
            var previousStates = new HashSet<string>();

            for (index = 0; index < int.MaxValue; index++)
            {
                var positions = allMoons.Select(getPosition);
                var velocities = allMoons.Select(getVelocity);
                var currentState = string.Join('_', positions.Concat(velocities));

                if (previousStates.Contains(currentState))
                {
                    break;
                }

                previousStates.Add(currentState);

                foreach (var moonPairing in moonPairings)
                {
                    CalculateVelocities(moonPairing);
                }

                ApplyVelocities(allMoons);
            }

            return index;
        }

        private void CalculateVelocities(Tuple<Moon, Moon> moonPairing)
        {
            if (moonPairing.Item1.Position.X > moonPairing.Item2.Position.X)
            {
                moonPairing.Item1.Velocity.X--;
                moonPairing.Item2.Velocity.X++;
            }
            else if (moonPairing.Item1.Position.X < moonPairing.Item2.Position.X)
            {
                moonPairing.Item1.Velocity.X++;
                moonPairing.Item2.Velocity.X--;
            }

            if (moonPairing.Item1.Position.Y > moonPairing.Item2.Position.Y)
            {
                moonPairing.Item1.Velocity.Y--;
                moonPairing.Item2.Velocity.Y++;
            }
            else if (moonPairing.Item1.Position.Y < moonPairing.Item2.Position.Y)
            {
                moonPairing.Item1.Velocity.Y++;
                moonPairing.Item2.Velocity.Y--;
            }

            if (moonPairing.Item1.Position.Z > moonPairing.Item2.Position.Z)
            {
                moonPairing.Item1.Velocity.Z--;
                moonPairing.Item2.Velocity.Z++;
            }
            else if (moonPairing.Item1.Position.Z < moonPairing.Item2.Position.Z)
            {
                moonPairing.Item1.Velocity.Z++;
                moonPairing.Item2.Velocity.Z--;
            }
        }

        private void ApplyVelocities(IEnumerable<Moon> allMoons)
        {
            foreach (var moon in allMoons)
            {
                moon.Position.Adjust(moon.Velocity);
            }
        }

        [DebuggerDisplay("{" + nameof(Id) + "}")]
        private class Moon
        {
            public Moon(int id)
            {
                Id = id;
                Position = new Axis3d();
                Velocity = new Axis3d();
            }

            public int Id { get; }

            public Axis3d Position { get; }

            public Axis3d Velocity { get; }
        }

        private class Axis3d
        {
            public int X { get; set; }

            public int Y { get; set; }

            public int Z { get; set; }

            public void Adjust(Axis3d adjustment)
            {
                X += adjustment.X;
                Y += adjustment.Y;
                Z += adjustment.Z;
            }
        }

        // Based on https://stackoverflow.com/a/29717490
        private static long Lcm(IEnumerable<long> numbers)
        {
            return numbers.Aggregate(Lcm);
        }

        private static long Lcm(long a, long b)
        {
            return Math.Abs(a * b) / Gcd(a, b);
        }

        private static long Gcd(long a, long b)
        {
            while (true)
            {
                if (b == 0)
                {
                    return a;
                }

                var a1 = a;
                a = b;
                b = a1 % b;
            }
        }
    }
}