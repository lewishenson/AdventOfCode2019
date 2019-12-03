using System;
using System.Reflection;
using AdventOfCode2019.Puzzles;

namespace AdventOfCode2019
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ADVENT OF CODE 2019");

            Console.WriteLine("Enter day number:");
            var dayNumber = Console.ReadLine();

            Console.WriteLine("Enter puzzle number:");
            var puzzleNumber = Console.ReadLine();

            var puzzle = PuzzleFactory.Create(dayNumber, puzzleNumber);

            if (puzzle == null)
            {
                Console.WriteLine("Puzzle not yet solved!");
            }
            else
            {
                var answer = puzzle.Solve();

                Console.WriteLine("Answer:");
                Console.WriteLine(answer);
            }
        }
    }
}
