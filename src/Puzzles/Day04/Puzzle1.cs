using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day04
{
    public class Puzzle1 : IPuzzle
    {
        public object Solve()
        {
            var (minimum, maximum) = GetInput();

            var validPasswords = new List<int>();

            for (var password = minimum; password <= maximum; password++)
            {
                if (IsPotentialPassword(password))
                {
                    validPasswords.Add(password);
                }
            }

            return validPasswords.Count;
        }

        private (int minimum, int maximum) GetInput()
        {
            var input = File.ReadAllText("Puzzles/Day04/input.txt");

            var numbers = input.Split('-')
                               .Select(int.Parse)
                               .ToList();

            return (minimum: numbers[0], maximum: numbers[1]);
        }

        private bool IsPotentialPassword(int password)
        {
            if (!DigitsNeverDecrease(password))
            {
                return false;
            }

            if (!HasTwoAdjacentDigitsTheSame(password))
            {
                return false;
            }

            return true;
        }

        private bool DigitsNeverDecrease(int password)
        {
            var orderedDigits = password.ToString().OrderBy(c => c).ToArray();
            var orderedPasswordString = new string(orderedDigits);
            var orderedPassword = int.Parse(orderedPasswordString);

            return orderedPassword == password;
        }

        private bool HasTwoAdjacentDigitsTheSame(int password)
        {
            var groupedDigits = password.ToString().GroupBy(c => c);

            return groupedDigits.Any(group => group.Count() > 1);
        }
    }
}