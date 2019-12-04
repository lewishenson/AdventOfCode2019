using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day04
{
    public class Puzzle1 : IPuzzle
    {
        public int Solve()
        {
            var input = GetInput();

            var validPasswords = new List<int>();

            for (var password = input.minimum; password <= input.maximum; password++)
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
            var input = File.ReadAllText("Puzzles\\Day04\\input.txt");

            var numbers = input.Split('-')
                               .Select(int.Parse)
                               .ToList();

            return (minimum: numbers[0], maximum: numbers[1]);
        }

        private bool IsPotentialPassword(int password)
        {
            if (!HasTwoAdjacentDigitsTheSame(password))
            {
                return false;
            }

            if (!DigitsNeverDecrease(password))
            {
                return false;
            }

            return true;
        }

        private bool HasTwoAdjacentDigitsTheSame(int password)
        {
            var digits = password.ToString().ToArray();

            return digits[0] == digits[1]
                || digits[1] == digits[2]
                || digits[2] == digits[3]
                || digits[3] == digits[4]
                || digits[4] == digits[5];
        }

        private bool DigitsNeverDecrease(int password)
        {
            var orderedDigits = password.ToString().OrderBy(c => c).ToArray();
            var orderedPasswordString = new string(orderedDigits);
            var orderedPassword = int.Parse(orderedPasswordString);

            return orderedPassword == password;
        }
    }
}