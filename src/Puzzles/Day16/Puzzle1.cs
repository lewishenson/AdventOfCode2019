using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day16
{
    public class Puzzle1 : IPuzzle
    {
        public object Solve()
        {
            var inputSignal = GetInputSignal();

            var signalCleaner = new SignalCleaner();
            var outputSignal = signalCleaner.Clean(inputSignal, 100);

            var answer = string.Join(string.Empty, outputSignal.Take(8));

            return answer;
        }

        private IReadOnlyList<int> GetInputSignal()
        {
            var line = File.ReadAllText("Puzzles/Day16/input.txt").Trim();

            return line.Select(c => int.Parse(c.ToString())).ToList();
        }

        private class SignalCleaner
        {
            public IEnumerable<int> Clean(IReadOnlyCollection<int> inputSignal, int phases)
            {
                var offsetPatterns = GenerateOffsetPatterns(inputSignal.Count);

                var outputSignal = new List<int>(inputSignal);

                for (var phaseIndex = 0; phaseIndex < phases; phaseIndex++)
                {
                    var phaseResult = new List<int>(inputSignal);

                    for (var offsetPatternIndex = 0; offsetPatternIndex < offsetPatterns.Count; offsetPatternIndex++)
                    {
                        var offsetPattern = offsetPatterns[offsetPatternIndex];

                        var runningTotal = 0;

                        for (var elementIndex = 0; elementIndex < outputSignal.Count; elementIndex++)
                        {
                            var element = outputSignal[elementIndex];
                            var elementOffset = offsetPattern[elementIndex];
                            var elementResult = element * elementOffset;
                            runningTotal += elementResult;
                        }

                        var keptDigit = Math.Abs(runningTotal % 10);
                        phaseResult[offsetPatternIndex] = keptDigit;
                    }

                    outputSignal = phaseResult;
                }

                return outputSignal;
            }

            private IReadOnlyDictionary<int, IReadOnlyList<int>> GenerateOffsetPatterns(int inputSignalLength)
            {
                var basePattern = new[] { 0, 1, 0, -1 };
                var offsetPattern = new Dictionary<int, IReadOnlyList<int>>();

                for (var i = 0; i < inputSignalLength; i++)
                {
                    var pattern = new List<int>();

                    while (pattern.Count <= inputSignalLength)
                    {
                        foreach (var basePatternValue in basePattern)
                        {
                            pattern.AddRange(Enumerable.Repeat(basePatternValue, i + 1));
                        }
                    }

                    offsetPattern[i] = pattern.Skip(1).Take(inputSignalLength).ToList();
                }

                return offsetPattern;
            }
        }
    }
}