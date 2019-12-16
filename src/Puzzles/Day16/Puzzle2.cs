using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day16
{
    public class Puzzle2 : IPuzzle
    {
        public object Solve()
        {
            var inputSignal = GetInputSignal();

            var signalCleaner = new SignalCleaner();
            var outputSignal = signalCleaner.Clean(inputSignal, 100);

            var answer = string.Join(string.Empty, outputSignal);

            return answer;
        }

        private IReadOnlyList<int> GetInputSignal()
        {
            var line = File.ReadAllText("Puzzles/Day16/input.txt").Trim();
            
            var single = line.Select(c => int.Parse(c.ToString())).ToList();

            const int repeatedCount = 10000;

            var inputSignal = Enumerable.Empty<int>();

            for (var i = 0; i < repeatedCount; i++)
            {
                inputSignal = inputSignal.Concat(single);
            }

            return inputSignal.ToList();
        }

        private class SignalCleaner
        {
            public IEnumerable<int> Clean(IReadOnlyCollection<int> inputSignal, int phases)
            {
                var messageOffset = GetMessageOffset(inputSignal);

                // Checking where message is because if more than halfway the offset will always be 1 so we don't need to work it out at runtime.
                var messageInSecondHalf = messageOffset > (inputSignal.Count / 2);
                if (!messageInSecondHalf)
                {
                    throw new InvalidOperationException("Solution only supports messages in second half.");
                }

                var relevantInputSignal = inputSignal.Skip(messageOffset).ToList();

                var outputSignal = new List<int>(relevantInputSignal);

                for (var phaseIndex = 0; phaseIndex < phases; phaseIndex++)
                {
                    var phaseResult = new List<int>(outputSignal)
                    {
                        // The last digit never changes so can just copy across.
                        [relevantInputSignal.Count - 1] = outputSignal[relevantInputSignal.Count - 1]
                    };
                    
                    // As we are only keeping the units column can just work from right to left adding to the already computed totals.
                    for (var digitIndex = relevantInputSignal.Count - 2; digitIndex >= 0; digitIndex--)
                    {
                        var justCalculatedTotalForNextDigit = phaseResult[digitIndex + 1];
                        var currentValueForCurrentDigit = phaseResult[digitIndex];
                        var newValueForCurrentDigit = justCalculatedTotalForNextDigit + currentValueForCurrentDigit;

                        var keptDigit = Math.Abs(newValueForCurrentDigit % 10);
                        phaseResult[digitIndex] = keptDigit;
                    }

                    outputSignal = phaseResult;
                }

                var message = outputSignal.Take(8).ToList();

                return message;
            }
            
            private int GetMessageOffset(IEnumerable<int> inputSignal)
            {
                var offsetDigits = inputSignal.Take(7);
                var rawOffset = string.Join(string.Empty, offsetDigits);
                var offset = int.Parse(rawOffset);

                return offset;
            }
        }
    }
}