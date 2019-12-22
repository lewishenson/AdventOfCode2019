using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day22
{
    public class Puzzle1 : IPuzzle
    {
        public object Solve()
        {
            var instructions = GetInstructions();

            var cards = GetSpaceCards();

            var pipeline = GetPipeline(instructions);
            var shuffledCards = pipeline.Shuffle(cards).ToList();

            var answer = shuffledCards.IndexOf(2019);

            return answer;
        }

        private IReadOnlyList<string> GetInstructions()
        {
            var lines = File.ReadAllLines("Puzzles/Day22/input.txt");

            return lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        }

        private IReadOnlyList<int> GetSpaceCards()
        {
            return Enumerable.Range(0, 10007).ToList();
        }

        private IShufflingTechnique GetPipeline(IEnumerable<string> instructions)
        {
            IShufflingTechnique pipeline = new NullShufflingTechnique();

            var reversedInstructions = instructions.Reverse();

            foreach (var instruction in reversedInstructions)
            {
                if (instruction == "deal into new stack")
                {
                    pipeline = new DealIntoNewStack(pipeline.Shuffle);
                }
                else if (instruction.StartsWith("cut"))
                {
                    var n = ExtractN(instruction);
                    pipeline = new CutN(pipeline.Shuffle, n);
                }
                else if (instruction.StartsWith("deal with increment"))
                {
                    var n = ExtractN(instruction);
                    pipeline = new DealWithIncrementN(pipeline.Shuffle, n);
                }
                else
                {
                    throw new InvalidOperationException($"Unknown instruction '{instruction}'!");
                }
            }

            return pipeline;
        }

        private int ExtractN(string instruction)
        {
            var parts = instruction.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var rawN = parts.Last();

            return int.Parse(rawN);
        }

        private interface IShufflingTechnique
        {
            IReadOnlyList<int> Shuffle(IReadOnlyList<int> cards);
        }

        private class NullShufflingTechnique : IShufflingTechnique
        {
            public IReadOnlyList<int> Shuffle(IReadOnlyList<int> cards)
            {
                return cards;
            }
        }

        private class DealIntoNewStack : IShufflingTechnique
        {
            private readonly Func<IReadOnlyList<int>, IReadOnlyList<int>> _next;

            public DealIntoNewStack(Func<IReadOnlyList<int>, IReadOnlyList<int>> next)
            {
                _next = next;
            }

            public IReadOnlyList<int> Shuffle(IReadOnlyList<int> cards)
            {
                var shuffledCards = cards.Reverse().ToList();

                return _next(shuffledCards);
            }
        }

        private class CutN : IShufflingTechnique
        {
            private readonly Func<IReadOnlyList<int>, IReadOnlyList<int>> _next;
            private readonly int _n;

            public CutN(Func<IReadOnlyList<int>, IReadOnlyList<int>> next, int n)
            {
                _next = next;
                _n = n;
            }

            public IReadOnlyList<int> Shuffle(IReadOnlyList<int> cards)
            {
                IReadOnlyList<int> shuffledCards;

                if (_n > 0)
                {
                    shuffledCards = cards.Skip(_n).Concat(cards.Take(_n)).ToList();
                }
                else
                {
                    var positiveN = _n * -1;
                    var remainingCount = cards.Count - positiveN;
                    shuffledCards = cards.TakeLast(positiveN).Concat(cards.Take(remainingCount)).ToList();
                }

                return _next(shuffledCards);
            }
        }

        private class DealWithIncrementN : IShufflingTechnique
        {
            private readonly Func<IReadOnlyList<int>, IReadOnlyList<int>> _next;
            private readonly int _n;

            public DealWithIncrementN(Func<IReadOnlyList<int>, IReadOnlyList<int>> next, int n)
            {
                _next = next;
                _n = n;
            }

            public IReadOnlyList<int> Shuffle(IReadOnlyList<int> cards)
            {
                var pointer = 0;

                var array = new int[cards.Count];

                foreach (var card in cards)
                {
                    array[pointer] = card;

                    pointer += _n;

                    if (pointer >= array.Length)
                    {
                        pointer -= array.Length;
                    }
                }

                var shuffledCards = array.ToList();

                return _next(shuffledCards);
            }
        }
    }
}