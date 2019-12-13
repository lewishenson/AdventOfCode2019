using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day12
{
    public class Puzzle1 : IPuzzle
    {
        public object Solve()
        {
            var allMoons = GetMoons().ToList();

            var moonPairings = GetPairings(allMoons).ToList();

            const int numberOfTimeSteps = 1000;

            for (var index = 0; index < numberOfTimeSteps; index++)
            {
                foreach (var moonPairing in moonPairings)
                {
                    CalculateVelocities(moonPairing);
                }

                ApplyVelocities(allMoons);
            }

            var totalEnergy = CalculateTotalEnergy(allMoons);

            return totalEnergy;
        }

        private IEnumerable<Moon> GetMoons()
        {
            var lines = File.ReadAllLines("Puzzles/Day12/input.txt");

            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];

                yield return ParseLine(line, lineIndex);
            }
        }

        private static Moon ParseLine(string line, int lineIndex)
        {
            var strippedLine = line.Replace("<", string.Empty)
                                   .Replace(">", string.Empty)
                                   .Replace(" ", string.Empty);

            var coordinates = strippedLine.Split(',');

            var moon = new Moon(lineIndex);

            foreach (var coordinate in coordinates)
            {
                var data = coordinate.Split('=');

                switch (data[0])
                {
                    case "x":
                        moon.Position.X = int.Parse(data[1]);
                        break;

                    case "y":
                        moon.Position.Y = int.Parse(data[1]);
                        break;

                    case "z":
                        moon.Position.Z = int.Parse(data[1]);
                        break;
                }
            }

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

        private int CalculateTotalEnergy(IEnumerable<Moon> moons)
        {
            var totalEnergy = 0;

            foreach (var moon in moons)
            {
                var potentialEnergy = Math.Abs(moon.Position.X) + Math.Abs(moon.Position.Y) + Math.Abs(moon.Position.Z);
                var kineticEnergy = Math.Abs(moon.Velocity.X) + Math.Abs(moon.Velocity.Y) + Math.Abs(moon.Velocity.Z);
                var moonEnergy = potentialEnergy * kineticEnergy;

                totalEnergy += moonEnergy;
            }

            return totalEnergy;
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
    }
}