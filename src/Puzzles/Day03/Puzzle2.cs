using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day03
{
    public class Puzzle2 : IPuzzle
    {
        public object Solve()
        {
            var paths = GetPaths().ToList();

            var coordinates1 = GetCoordinates(paths[0]).ToList();
            var coordinates2 = GetCoordinates(paths[1]).ToList();

            var intersectionPoints = coordinates1.Intersect(coordinates2).ToList();

            var lowestStepsTotal = int.MaxValue;

            foreach (var intersectionPoint in intersectionPoints)
            {
                var firstPathSteps = coordinates1.IndexOf(intersectionPoint) + 1;
                var secondPathSteps = coordinates2.IndexOf(intersectionPoint) + 1;
                var totalSteps = firstPathSteps + secondPathSteps;
                lowestStepsTotal = Math.Min(lowestStepsTotal, totalSteps);
            }

            return lowestStepsTotal;
        }

        private IEnumerable<IReadOnlyList<Move>> GetPaths()
        {
            var lines = File.ReadAllLines("Puzzles\\Day03\\input.txt");

            foreach (var line in lines)
            {
                yield return line.Split(',')
                                 .Select(rawInput => new Move(rawInput))
                                 .ToList();
            }
        }

        private IEnumerable<Point> GetCoordinates(IEnumerable<Move> path)
        {
            var coordinates = new List<Point>();

            var currentPosition = new Point(0, 0);

            foreach (var move in path)
            {
                var offset = GetOffset(move.Direction);

                for (var i = 0; i < move.Distance; i++)
                {
                    var newX = currentPosition.X + offset.X;
                    var newY = currentPosition.Y + offset.Y;
                    currentPosition = new Point(newX, newY);
                    coordinates.Add(currentPosition);
                }
            }

            return coordinates;
        }

        private Point GetOffset(Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Point(0, 1),
                Direction.Down => new Point(0, -1),
                Direction.Left => new Point(-1, 0),
                Direction.Right => new Point(1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
        }

        private class Move
        {
            public Move(string rawInput)
            {
                Direction = (Direction)rawInput[0];
                Distance = int.Parse(rawInput.Substring(1));
            }

            public Direction Direction { get; }

            public int Distance { get; }
        }

        private enum Direction
        {
            Down = 'D',
            Left = 'L',
            Right = 'R',
            Up = 'U'
        }
    }
}