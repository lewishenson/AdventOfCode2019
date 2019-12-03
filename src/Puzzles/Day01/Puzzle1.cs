using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day01
{
    public class Puzzle1 : IPuzzle
    {
        public int Solve()
        {
            var masses = File.ReadAllLines("Puzzles\\Day01\\input.txt");

            var totalFuel = masses.Select(int.Parse)
                                  .Select(CalculateFuel)
                                  .Sum();

            return totalFuel;
        }

        private int CalculateFuel(int mass)
        {
            return (mass / 3) - 2;
        }
    }
}