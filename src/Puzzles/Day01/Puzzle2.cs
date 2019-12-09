using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day01
{
    public class Puzzle2 : IPuzzle
    {
        public object Solve()
        {
            var masses = File.ReadAllLines("Puzzles\\Day01\\input.txt");

            var totalFuel = masses.Select(int.Parse)
                                  .Select(CalculateFuel)
                                  .Sum();

            return totalFuel;
        }

        private int CalculateFuel(int mass)
        {
            var fuel = (mass / 3) - 2;

            if (fuel < 0)
            {
                fuel = 0;
            }
            else
            {
                var fuelForFuel = CalculateFuel(fuel);
                fuel += fuelForFuel;
            }

            return fuel;
        }
    }
}