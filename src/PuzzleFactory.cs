using System;
using System.Reflection;
using AdventOfCode2019.Puzzles;

namespace AdventOfCode2019
{
    public static class PuzzleFactory
    {
        public static IPuzzle Create(string day, string puzzle)
        {
            var paddedDay = day.PadLeft(2, '0').Substring(0, 2);
            var puzzleTypeName = $"AdventOfCode2019.Puzzles.Day{paddedDay}.Puzzle{puzzle}";

            var puzzleType = Assembly.GetExecutingAssembly().GetType(puzzleTypeName);

            return puzzleType != null
                ? (IPuzzle)Activator.CreateInstance(puzzleType)
                : null;
        }
    }
}
