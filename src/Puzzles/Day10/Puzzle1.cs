using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day10
{
    public class Puzzle1 : IPuzzle
    {
        public object Solve()
        {
            var asteroids = GetAsteroids().ToList();

            var bestVisibility = 0;
            Point instantMonitoringStationLocation = default;

            foreach (var asteroid in asteroids)
            {
                var visibility = GetVisibleAsteroidsCount(asteroid, asteroids);

                if (visibility > bestVisibility)
                {
                    bestVisibility = visibility;
                    instantMonitoringStationLocation = asteroid;
                }
            }

            Console.WriteLine($"X: {instantMonitoringStationLocation.X}, Y: {instantMonitoringStationLocation.Y}");

            return bestVisibility;
        }

        private IEnumerable<Point> GetAsteroids()
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
                        yield return new Point(x, y);
                    }
                }
            }
        }

        private int GetVisibleAsteroidsCount(Point home, IEnumerable<Point> allAsteroids)
        {
            var distinctAngles = new HashSet<double>();

            foreach (var asteroid in allAsteroids)
            {
                if (asteroid == home)
                {
                    continue;
                }

                var angle = CalculateViewingAngle(home, asteroid);

                if (!distinctAngles.Contains(angle))
                {
                    distinctAngles.Add(angle);
                }
            }

            return distinctAngles.Count;
        }

        private double CalculateViewingAngle(Point home, Point asteroid)
        {
            var adjacent = asteroid.X - home.X;
            var opposite = asteroid.Y - home.Y;

            var absoluteAdjacent = Math.Abs(adjacent);
            var absoluteOpposite = Math.Abs(opposite);

            var angle = Math.Atan2(absoluteOpposite, absoluteAdjacent) * (180 / Math.PI);

            if (adjacent >= 0 && opposite >= 0)
            {
                angle = 90 - angle;
            }
            else if (adjacent >= 0 && opposite < 0)
            {
                angle += 90;
            }
            else if (adjacent < 0 && opposite >= 0)
            {
                angle += 270;
            }
            else
            {
                angle = 270 - angle;
            }

            return angle;
        }
    }
}