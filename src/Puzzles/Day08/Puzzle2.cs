using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2019.Puzzles.Day08
{
    public class Puzzle2 : IPuzzle
    {
        public object Solve()
        {
            var imageData = File.ReadAllText("Puzzles/Day08/input.txt").Trim();

            const int width = 25;
            const int height = 6;
            const int size = width * height;

            var layers = GetLayers(imageData, size).ToList();

            var output = new StringBuilder();

            for (var i = 0; i < size; i++)
            {
                var pixel = layers.Select(layer => layer[i])
                                  .FirstOrDefault(c => c != Colours.Transparent);

                switch (pixel)
                {
                    case Colours.Black:
                        output.Append(" ");
                        break;

                    case Colours.White:
                        output.Append("█");
                        break;

                    default:
                        output.Append(" ");
                        break;
                }

                if ((i + 1) % width == 0)
                {
                    output.AppendLine();
                }
            }

            Console.WriteLine(output);

            return 0;
        }

        private IEnumerable<string> GetLayers(string imageData, int size)
        {
            for (var i = 0; i < imageData.Length; i += size)
            {
                yield return imageData.Substring(i, Math.Min(size, imageData.Length - i));
            }
        }

        private static class Colours
        {
            public const char Black = '0';
            public const char White = '1';
            public const char Transparent = '2';
        }
    }
}