using System;
using System.Diagnostics;

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
                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    var answer = puzzle.Solve();
                    stopwatch.Stop();

                    Console.WriteLine($"Answer: (calculated in {stopwatch.ElapsedMilliseconds}ms)");
                    Console.WriteLine(answer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
