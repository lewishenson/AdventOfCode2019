using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day10
{
    public class Puzzle2 : IPuzzle
    {
        public object Solve()
        {
            var allAsteroids = GetEmptyAsteroids().ToList();

            // Co-ordinates from puzzle 1.
            var instantMonitoringStationLocation = allAsteroids.Single(asteroid => asteroid.X == 13 && asteroid.Y == 17);

            var otherAsteroids = allAsteroids.Where(asteroid => asteroid != instantMonitoringStationLocation).ToList();
            CalculateViewingAnglesAndDistances(instantMonitoringStationLocation, otherAsteroids);

            var asteroidsInRotationSequenceOrderedByDistance = otherAsteroids.GroupBy(asteroid => asteroid.ViewingAngle)
                                                                             .OrderBy(group => group.Key)
                                                                             .Select(group => group.OrderBy(asteroid => asteroid.Distance))
                                                                             .Select(asteroids => new Queue<Asteroid>(asteroids))
                                                                             .ToList();

            var destroyedAsteroids = new List<Asteroid>();

            while (destroyedAsteroids.Count < 200)
            {
                foreach (var currentAngleAsteroids in asteroidsInRotationSequenceOrderedByDistance)
                {
                    var destroyedAsteroid = currentAngleAsteroids.Dequeue();
                    if (destroyedAsteroid != null)
                    {
                        destroyedAsteroids.Add(destroyedAsteroid);
                    }
                }
            }

            var twoHundredthAsteroid = destroyedAsteroids.ElementAt(199);

            var answer = (twoHundredthAsteroid.X * 100) + twoHundredthAsteroid.Y;

            return answer;
        }

        private IEnumerable<Asteroid> GetEmptyAsteroids()
        {
            var lines = File.ReadAllLines("Puzzles\\Day10\\input.txt");

            for (var y = 0; y < lines.Length; y++)
            {
                var line = lines[y];

                for (var x = 0; x < line.Length; x++)
                {
                    var location = line[x];

                    if (location == '#')
                    {
                        yield return new Asteroid(x, y);
                    }
                }
            }
        }

        private void CalculateViewingAnglesAndDistances(Asteroid home, IEnumerable<Asteroid> allAsteroids)
        {
            foreach (var asteroid in allAsteroids)
            {
                if (asteroid == home)
                {
                    continue;
                }

                CalculateViewingAngleAndDistance(home, asteroid);
            }
        }

        private void CalculateViewingAngleAndDistance(Asteroid home, Asteroid asteroid)
        {
            var adjacent = asteroid.X - home.X;
            var opposite = asteroid.Y - home.Y;

            var absoluteAdjacent = Math.Abs(adjacent);
            var absoluteOpposite = Math.Abs(opposite);

            var angle = Math.Atan2(absoluteOpposite, absoluteAdjacent) * (180 / Math.PI);

            if (adjacent >= 0 && opposite >= 0)
            {
                angle += 90;
            }
            else if (adjacent >= 0 && opposite < 0)
            {
                angle = 90 - angle;
            }
            else if (adjacent < 0 && opposite >= 0)
            {
                angle = 270 - angle;
            }
            else
            {
                angle += 270;
            }

            asteroid.ViewingAngle = angle;

            asteroid.Distance = Math.Sqrt((absoluteAdjacent * absoluteAdjacent) + (absoluteOpposite * absoluteOpposite));
        }

        private class Asteroid
        {
            public Asteroid(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }

            public int Y { get; }

            public double? ViewingAngle { get; set; }

            public double? Distance { get; set; }
        }
    }
}