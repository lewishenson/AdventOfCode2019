using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day08
{
    public class Puzzle1 : IPuzzle
    {
        public object Solve()
        {
            var imageData = File.ReadAllText("Puzzles\\Day08\\input.txt").Trim();

            const int width = 25;
            const int height = 6;
            const int size = width * height;

            var layers = GetLayers(imageData, size).ToList();

            var layerWithFewestZeroes = layers.OrderBy(layer => layer.Count(c => c == '0')).First();

            var oneCount = layerWithFewestZeroes.Count(c => c == '1');
            var twoCount = layerWithFewestZeroes.Count(c => c == '2');
            var answer = twoCount * oneCount;

            return answer;
        }

        private IEnumerable<string> GetLayers(string imageData, int size)
        {
            for (var i = 0; i < imageData.Length; i += size)
            {
                yield return imageData.Substring(i, Math.Min(size, imageData.Length - i));
            }
        }
    }
}