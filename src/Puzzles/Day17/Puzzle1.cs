using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode2019.Puzzles.Day17
{
    public class Puzzle1 : IPuzzle
    {
        public object Solve()
        {
            var program = GetProgram();
            var input = new BlockingCollection<long>();
            var output = new BlockingCollection<long>();

            var computer = new IntcodeComputer();

            var runTask = Task.Run(() =>
            {
                while (!computer.IsHalted)
                {
                    computer.Run(program, input, output);
                }
            });

            var answer = 0;

            var summingTask = Task.Run(() =>
            {
                const int newLineCode = 10;

                var currentLocation = new Point(0, 0);
                var view = new Dictionary<Point, char>();

                while (!computer.IsHalted || output.Count > 0)
                {
                    if (!output.TryTake(out var result, TimeSpan.FromMilliseconds(10)))
                    {
                        break;
                    }

                    if (result == newLineCode)
                    {
                        Console.WriteLine();

                        currentLocation = new Point(0, currentLocation.Y + 1);
                    }
                    else
                    {
                        var character = (char) result;
                        Console.Write(character);

                        view[currentLocation] = character;

                        currentLocation = new Point(currentLocation.X + 1, currentLocation.Y);
                    }
                }

                answer = GetAlignmentParametersSum(view);
            });

            Task.WaitAll(runTask, summingTask);

            return answer;
        }

        private IList<long> GetProgram()
        {
            var input = File.ReadAllText("Puzzles/Day17/input.txt");

            var program = input.Split(',')
                               .Select(long.Parse)
                               .ToList();

            var padding = Enumerable.Repeat((long)0, 10000 - program.Count);
            program.AddRange(padding);

            return program;
        }

        private int GetAlignmentParametersSum(IReadOnlyDictionary<Point, char> view)
        {
            var intersectionLocations = FindIntersectionLocations(view);

            var sum = intersectionLocations.Select(location => location.X * location.Y).Sum();

            return sum;
        }

        private IEnumerable<Point> FindIntersectionLocations(IReadOnlyDictionary<Point, char> view)
        {
            var maxX = view.Keys.Max(location => location.X);
            var maxY = view.Keys.Max(location => location.Y);

            var pixelsNotOnEdge = view.Where(pixel => pixel.Key.X > 1
                                                      && pixel.Key.Y > 1
                                                      && pixel.Key.X < maxX
                                                      && pixel.Key.Y < maxY);

            var intersectionPixels = pixelsNotOnEdge.Where(pixel =>
            {
                const char scaffold = '#';

                var abovePixelLocation = new Point(pixel.Key.X, pixel.Key.Y - 1);
                var abovePixel = view[abovePixelLocation];
                if (abovePixel != scaffold)
                {
                    return false;
                }

                var rightPixelLocation = new Point(pixel.Key.X + 1, pixel.Key.Y);
                var rightPixel = view[rightPixelLocation];
                if (rightPixel != scaffold)
                {
                    return false;
                }

                var belowPixelLocation = new Point(pixel.Key.X, pixel.Key.Y + 1);
                var belowPixel = view[belowPixelLocation];
                if (belowPixel != scaffold)
                {
                    return false;
                }

                var leftPixelLocation = new Point(pixel.Key.X - 1, pixel.Key.Y);
                var leftPixel = view[leftPixelLocation];
                if (leftPixel != scaffold)
                {
                    return false;
                }

                return true;
            });

            var intersectionLocations = intersectionPixels.Select(pixel => pixel.Key);

            return intersectionLocations;
        }

        private class IntcodeComputer
        {
            private int _pointer;
            private int _relativeBase;
            private IList<long> _program;
            private BlockingCollection<long> _input;
            private BlockingCollection<long> _output;
            private long _finalOutput;

            public bool IsHalted { get; private set; }

            public long Run(IList<long> program, BlockingCollection<long> input, BlockingCollection<long> output)
            {
                _pointer = 0;
                _relativeBase = 0;
                _program = program;
                _input = input;
                _output = output;
                _finalOutput = 0;

                while (true)
                {
                    var opcode = (int)program[_pointer];
                    _pointer++;

                    if (opcode == OpCodes.Halt)
                    {
                        this.IsHalted = true;
                        break;
                    }

                    var instruction = opcode % 100;

                    switch (instruction)
                    {
                        case OpCodes.Add:
                            AddOperation(opcode);
                            break;

                        case OpCodes.Multiply:
                            MultiplyOperation(opcode);
                            break;

                        case OpCodes.StoreInput:
                            InputOperation(opcode);
                            break;

                        case OpCodes.StoreOutput:
                            OutputOperation(opcode);
                            break;

                        case OpCodes.JumpIfTrue:
                            JumpIfTrueOperation(opcode);
                            break;

                        case OpCodes.JumpIfFalse:
                            JumpIfFalseOperation(opcode);
                            break;

                        case OpCodes.LessThan:
                            LessThanOperation(opcode);
                            break;

                        case OpCodes.Equal:
                            EqualsOperation(opcode);
                            break;

                        case OpCodes.RelativeBaseAdjustment:
                            AdjustRelativeBase(opcode);
                            break;

                        default:
                            throw new InvalidOperationException($"Invalid instruction '{instruction}'.");
                    }
                }

                return _finalOutput;
            }

            private IReadOnlyList<int> GetParameterModes(int opcode)
            {
                var firstParameterMode = (opcode / 100) % 10;
                var secondParameterMode = (opcode / 1000) % 10;
                var thirdParameterMode = (opcode / 10000) % 10;

                return new List<int> { firstParameterMode, secondParameterMode, thirdParameterMode };
            }

            private long GetInputParameterValue(int parameterMode)
            {
                long parameterValue;

                switch (parameterMode)
                {
                    case Modes.Position:
                        var positionParameterValueIndex = (int)_program[_pointer];
                        parameterValue = _program[positionParameterValueIndex];
                        break;

                    case Modes.Immediate:
                        parameterValue = _program[_pointer];
                        break;

                    case Modes.Relative:
                        var relativeParameterValueIndex = (int)_program[_pointer] + _relativeBase;
                        parameterValue = _program[relativeParameterValueIndex];
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameterMode), parameterMode, "Unsupported parameter mode");
                }

                _pointer++;

                return parameterValue;
            }

            private long GetOutputParameterIndex(int parameterMode)
            {
                var parameterIndex = parameterMode switch
                {
                    Modes.Position => (int)_program[_pointer],
                    Modes.Immediate => throw new InvalidOperationException(),
                    Modes.Relative => ((int)_program[_pointer] + _relativeBase),
                    _ => throw new ArgumentOutOfRangeException(nameof(parameterMode), parameterMode, "Unsupported parameter mode")
                };

                _pointer++;

                return parameterIndex;
            }

            private void AddOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode);

                var parameter1 = GetInputParameterValue(parameterModes[0]);
                var parameter2 = GetInputParameterValue(parameterModes[1]);

                var result = parameter1 + parameter2;

                var parameter3 = (int)GetOutputParameterIndex(parameterModes[2]);
                _program[parameter3] = result;
            }

            private void MultiplyOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode);

                var parameter1 = GetInputParameterValue(parameterModes[0]);
                var parameter2 = GetInputParameterValue(parameterModes[1]);

                var result = parameter1 * parameter2;

                var parameter3 = (int)GetOutputParameterIndex(parameterModes[2]);
                _program[parameter3] = result;
            }

            private void InputOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode);
                var parameter1 = (int)GetOutputParameterIndex(parameterModes[0]);

                _program[parameter1] = _input.Take();
            }

            private void OutputOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode);
                var parameter1 = GetInputParameterValue(parameterModes[0]);

                _finalOutput = parameter1;
                _output.Add(parameter1);
            }

            private void JumpIfTrueOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode);

                var firstParameter = GetInputParameterValue(parameterModes[0]);
                var secondParameter = (int)GetInputParameterValue(parameterModes[1]);

                if (firstParameter != 0)
                {
                    _pointer = secondParameter;
                }
            }

            private void JumpIfFalseOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode);

                var firstParameter = GetInputParameterValue(parameterModes[0]);
                var secondParameter = (int)GetInputParameterValue(parameterModes[1]);

                if (firstParameter == 0)
                {
                    _pointer = secondParameter;
                }
            }

            private void LessThanOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode);

                var parameter1 = GetInputParameterValue(parameterModes[0]);
                var parameter2 = GetInputParameterValue(parameterModes[1]);

                var result = parameter1 < parameter2 ? 1 : 0;

                var parameter3 = (int)GetOutputParameterIndex(parameterModes[2]);
                _program[parameter3] = result;
            }

            private void EqualsOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode);

                var parameter1 = GetInputParameterValue(parameterModes[0]);
                var parameter2 = GetInputParameterValue(parameterModes[1]);

                var result = parameter1 == parameter2 ? 1 : 0;

                var parameter3 = (int)GetOutputParameterIndex(parameterModes[2]);
                _program[parameter3] = result;
            }

            private void AdjustRelativeBase(int opcode)
            {
                var parameterModes = GetParameterModes(opcode);
                var parameter1 = (int)GetInputParameterValue(parameterModes[0]);

                _relativeBase += parameter1;
            }
        }

        private static class OpCodes
        {
            public const int Add = 1;
            public const int Multiply = 2;
            public const int StoreInput = 3;
            public const int StoreOutput = 4;
            public const int JumpIfTrue = 5;
            public const int JumpIfFalse = 6;
            public const int LessThan = 7;
            public const int Equal = 8;
            public const int RelativeBaseAdjustment = 9;
            public const int Halt = 99;
        }

        private static class Modes
        {
            public const int Position = 0;
            public const int Immediate = 1;
            public const int Relative = 2;
        }
    }
}