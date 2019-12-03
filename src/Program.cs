using System;

namespace AdventOfCode2019
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var puzzle = new Puzzles.Day01.Puzzle1();
            var answer = puzzle.Solve();
            Console.WriteLine(answer);
        }
    }
}
