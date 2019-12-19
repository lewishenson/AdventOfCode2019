using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode2019.Puzzles.Day17
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

            long answer = 0;

            var summingTask = Task.Run(() =>
            {
                var currentLocation = new Point(0, 0);
                var view = new Dictionary<Point, char>();

                while (!computer.IsHalted || output.Count > 0)
                {
                    if (!output.TryTake(out var result, TimeSpan.FromMilliseconds(25)))
                    {
                        break;
                    }

                    if (result == Ascii.NewLine)
                    {
                        Console.WriteLine();

                        currentLocation = new Point(0, currentLocation.Y + 1);
                    }
                    else
                    {
                        var character = (char)result;
                        Console.Write(character);

                        view[currentLocation] = character;

                        currentLocation = new Point(currentLocation.X + 1, currentLocation.Y);
                    }
                }

                var movementRules = GetMovementRules(view);
                var movementInstructions = GenerateMovementInstructions(movementRules);

                // As instructed
                program = GetProgram(2);

                foreach (var movementInstruction in movementInstructions)
                {
                    input.Add(movementInstruction);
                }

                computer.Run(program, input, output);

                answer = output.Max();
            });

            Task.WaitAll(runTask, summingTask);

            return answer;
        }

        private MovementRules GetMovementRules(IReadOnlyDictionary<Point, char> view)
        {
            var route = GetRoute(view);
            var routeCsv = string.Join(",", route);
            Console.WriteLine(routeCsv);

            var movementRules = new MovementRules
            {
                MainRoutine = routeCsv
            };

            var remainingRoute = new List<string>(route);

            remainingRoute = FindFunction(movementRules, remainingRoute, (value) => movementRules.FunctionA = value, "A");
            remainingRoute = FindFunction(movementRules, remainingRoute, (value) => movementRules.FunctionB = value, "B");
            remainingRoute = FindFunction(movementRules, remainingRoute, (value) => movementRules.FunctionC = value, "C");

            if (remainingRoute.Count > 0)
            {
                throw new InvalidOperationException("Movement Rules not calculated");
            }

            return movementRules;
        }

        private IReadOnlyList<string> GetRoute(IReadOnlyDictionary<Point, char> view)
        {
            const char scaffold = '#';

            var route = new List<string>();

            var currentDirection = Direction.Up;
            var currentLocation = view.Single(pixel => pixel.Value == '^').Key;

            var keepGoing = true;
            var length = 0;

            while (keepGoing)
            {
                var lookAheadLocation = GetLookAheadLocation(currentLocation, currentDirection);
                view.TryGetValue(lookAheadLocation, out var lookAheadPixel);

                if (lookAheadPixel == scaffold)
                {
                    // Keep going in same direction
                    length++;

                    currentLocation = currentDirection.Id switch
                    {
                        'U' => PointUtilities.MoveUp(currentLocation),
                        'R' => PointUtilities.MoveRight(currentLocation),
                        'D' => PointUtilities.MoveDown(currentLocation),
                        'L' => PointUtilities.MoveLeft(currentLocation),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                else
                {
                    lookAheadLocation = GetLookAheadLocation(currentLocation, currentDirection.TurnRight);
                    view.TryGetValue(lookAheadLocation, out lookAheadPixel);

                    if (lookAheadPixel == scaffold)
                    {
                        // Scaffold on the right
                        if (length > 0)
                        {
                            route.Add(length.ToString());
                        }

                        route.Add(Direction.Right.Id.ToString());
                        currentDirection = currentDirection.TurnRight;
                        length = 0;
                    }
                    else
                    {
                        lookAheadLocation = GetLookAheadLocation(currentLocation, currentDirection.TurnLeft);
                        view.TryGetValue(lookAheadLocation, out lookAheadPixel);

                        if (lookAheadPixel == scaffold)
                        {
                            // Scaffold on the left
                            if (length > 0)
                            {
                                route.Add(length.ToString());
                            }

                            route.Add(Direction.Left.Id.ToString());
                            currentDirection = currentDirection.TurnLeft;
                            length = 0;
                        }
                        else
                        {
                            // Scaffold only behind so must have reached the end
                            if (length > 0)
                            {
                                route.Add(length.ToString());
                            }

                            keepGoing = false;
                        }
                    }
                }
            }

            return route;
        }

        private Point GetLookAheadLocation(Point currentLocation, Direction direction)
        {
            return direction.Id switch
            {
                'U' => PointUtilities.MoveUp(currentLocation),
                'R' => PointUtilities.MoveRight(currentLocation),
                'D' => PointUtilities.MoveDown(currentLocation),
                'L' => PointUtilities.MoveLeft(currentLocation),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unexpected direction")
            };
        }

        private List<string> FindFunction(MovementRules movementRules, List<string> remainingRoute, Action<string> setFunction, string functionName)
        {
            // Limit of 20 characters, this corresponds to 10 pairs of number and comma.
            for (var i = 10; i > 0; i -= 2)
            {
                var patternToMatch = string.Join(",", remainingRoute.Take(i));

                var numberOfMatches = Regex.Matches(movementRules.MainRoutine, patternToMatch).Count;

                if (numberOfMatches <= 1)
                {
                    continue;
                }

                setFunction(patternToMatch);

                while (movementRules.MainRoutine.Contains(patternToMatch))
                {
                    movementRules.MainRoutine = movementRules.MainRoutine.Replace(patternToMatch, functionName);
                }

                remainingRoute = movementRules.MainRoutine.Replace("A", string.Empty)
                    .Replace("B", string.Empty)
                    .Replace("C", string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

                break;
            }

            return remainingRoute;
        }

        private IEnumerable<int> GenerateMovementInstructions(MovementRules movementRules)
        {
            static List<int> ToAscii(string instruction)
            {
                return instruction.Select(i => (int)i).ToList();
            }

            var movementInstructions = new List<int>();

            var mainRoutine = ToAscii(movementRules.MainRoutine);
            movementInstructions.AddRange(mainRoutine);
            movementInstructions.Add(Ascii.NewLine);

            var functionA = ToAscii(movementRules.FunctionA);
            movementInstructions.AddRange(functionA);
            movementInstructions.Add(Ascii.NewLine);

            var functionB = ToAscii(movementRules.FunctionB);
            movementInstructions.AddRange(functionB);
            movementInstructions.Add(Ascii.NewLine);

            var functionC = ToAscii(movementRules.FunctionC);
            movementInstructions.AddRange(functionC);
            movementInstructions.Add(Ascii.NewLine);

            const int noLiveFeed = (int)'n';
            movementInstructions.Add(noLiveFeed);

            movementInstructions.Add(Ascii.NewLine);

            return movementInstructions;
        }

        private IList<long> GetProgram(int? addressZeroOverride = null)
        {
            var input = File.ReadAllText("Puzzles/Day17/input.txt");

            var program = input.Split(',')
                               .Select(long.Parse)
                               .ToList();

            if (addressZeroOverride.HasValue)
            {
                program[0] = addressZeroOverride.Value;
            }

            var padding = Enumerable.Repeat((long)0, 10000 - program.Count);
            program.AddRange(padding);

            return program;
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

        private sealed class Direction
        {
            public static readonly Direction Up = new Direction('U', 'L', 'R');
            public static readonly Direction Right = new Direction('R', 'U', 'D');
            public static readonly Direction Down = new Direction('D', 'R', 'L');
            public static readonly Direction Left = new Direction('L', 'D', 'U');

            private static readonly IEnumerable<Direction> All = new[] { Up, Right, Down, Left };

            private readonly char _leftId;
            private readonly char _rightId;

            private Direction(char id, char leftId, char rightId)
            {
                Id = id;
                _leftId = leftId;
                _rightId = rightId;
            }

            public char Id { get; }

            public Direction TurnLeft => All.Single(d => d.Id == _leftId);

            public Direction TurnRight => All.Single(d => d.Id == _rightId);
        }

        public static class PointUtilities
        {
            public static Point MoveUp(Point original)
            {
                // For this puzzle, counting from top left
                return new Point(original.X, original.Y - 1);
            }

            public static Point MoveRight(Point original)
            {
                return new Point(original.X + 1, original.Y);
            }

            public static Point MoveDown(Point original)
            {
                // For this puzzle, counting from top left
                return new Point(original.X, original.Y + 1);
            }

            public static Point MoveLeft(Point original)
            {
                return new Point(original.X - 1, original.Y);
            }
        }

        private class MovementRules
        {
            public string MainRoutine { get; set; }

            public string FunctionA { get; set; }

            public string FunctionB { get; set; }

            public string FunctionC { get; set; }
        }

        public static class Ascii
        {
            public const int NewLine = 10;
        }
    }
}