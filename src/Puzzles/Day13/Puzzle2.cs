using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode2019.Puzzles.Day13
{
    public class Puzzle2 : IPuzzle
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

            long score = -1;

            var outputTask = Task.Run(() =>
            {
                var screen = new Dictionary<Point, string>();

                var buffer = new List<long>();

                while (!computer.IsHalted)
                {
                    if (!output.TryTake(out var result, TimeSpan.FromMilliseconds(1)))
                    {
                        //DrawScreen(screen);

                        var joystickInput = CalculateJoystickInput(screen);
                        input.Add(joystickInput);
                    }
                    else
                    {
                        buffer.Add(result);

                        if (buffer.Count != 3)
                        {
                            continue;
                        }

                        var x = (int)buffer[0];
                        var y = (int)buffer[1];
                        var tile = (int)buffer[2];

                        var isScore = x == -1 && y == 0;
                        if (isScore)
                        {
                            score = tile;
                        }
                        else
                        {
                            var coordinate = new Point(x, y);

                            var displayTile = ConvertTileToDisplayValue(tile);
                            screen[coordinate] = displayTile;
                        }

                        buffer.Clear();
                    }
                }
            });

            Task.WaitAll(runTask, outputTask);
            
            return score;
        }

        private IList<long> GetProgram()
        {
            var input = File.ReadAllText("Puzzles\\Day13\\input.txt");

            var program = input.Split(',')
                               .Select(long.Parse)
                               .ToList();

            // As instructed
            program[0] = 2;

            var padding = Enumerable.Repeat((long)0, 10000 - program.Count);
            program.AddRange(padding);

            return program;
        }

        private string ConvertTileToDisplayValue(int tile)
        {
            return tile switch
            {
                0 => " ",
                1 => "#",
                2 => "+",
                3 => "_",
                4 => ".",
                _ => throw new ArgumentOutOfRangeException(nameof(tile), tile, "Unexpected tile")
            };
        }

        private void DrawScreen(IReadOnlyDictionary<Point, string> pixels)
        {
            var minX = pixels.Keys.Min(coordinate => coordinate.X);
            var maxX = pixels.Keys.Max(coordinate => coordinate.X);

            var minY = pixels.Keys.Min(coordinate => coordinate.Y);
            var maxY = pixels.Keys.Max(coordinate => coordinate.Y);

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var point = new Point(x, y);

                    if (!pixels.TryGetValue(point, out var pixel))
                    {
                        pixel = " ";
                    }

                    Console.Write(pixel);
                }

                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine(new string('@', maxX));
            Console.WriteLine();
        }

        private int CalculateJoystickInput(IReadOnlyDictionary<Point, string> screen)
        {
            var ballX = screen.Where(p => p.Value == ".").Select(p => p.Key.X).SingleOrDefault();
            var paddleX = screen.Where(p => p.Value == "_").Select(p => p.Key.X).SingleOrDefault();

            var joystickInput = JoystickInput.Neutral;

            if (ballX < paddleX)
            {
                joystickInput = JoystickInput.Left;
            }
            else if (ballX > paddleX)
            {
                joystickInput = JoystickInput.Right;
            }

            return joystickInput;
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
                    Modes.Position => (int) _program[_pointer],
                    Modes.Immediate => throw new InvalidOperationException(),
                    Modes.Relative => ((int) _program[_pointer] + _relativeBase),
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

        private static class JoystickInput
        {
            public const int Left = -1;
            public const int Neutral = 0;
            public const int Right = 1;
        }
    }
}