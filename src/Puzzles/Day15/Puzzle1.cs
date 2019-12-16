using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode2019.Puzzles.Day15
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

            var route = new Stack<Action>();

            var directionsTask = Task.Run(() =>
            {
                var position = new Point(0, 0);
                var action = new Action(position, MovementCommand.North);
                route.Push(action);

                input.Add(action.MovementCommand);

                while (!computer.IsHalted)
                {
                    if (!output.TryTake(out var result, TimeSpan.FromMilliseconds(100)))
                    {
                        break;
                    }

                    switch (result)
                    {
                        case StatusCodes.HitWall:
                            if (action.TryChangeDirection())
                            {
                                input.Add(action.MovementCommand);
                            }
                            else
                            {
                                throw new InvalidOperationException("Nowhere to go!");
                            }
                            break;

                        case StatusCodes.MovedSuccessfully:
                            if (action.IsMovingBackwards)
                            {
                                // Part of a dead end so don't care - throw away.
                                route.Pop();
                            }

                            var newPosition = GetNewPosition(action.Position, action.MovementCommand);
                            action = route.Peek();

                            if (action.Position != newPosition)
                            {
                                // First time visiting so need to add to the route.
                                action = new Action(newPosition, action.MovementCommand);
                                route.Push(action);
                                input.Add(action.MovementCommand);
                            }
                            else
                            {
                                // Retracing our steps - try another direction we've not been in before.
                                action.TryChangeDirection();
                                input.Add(action.MovementCommand);
                            }

                            break;

                        case StatusCodes.AtOxygenSystem:
                            input.Add(-1);
                            break;
                    }
                }
            });

            Task.WaitAll(runTask, directionsTask);

            return route.Count;
        }

        private IList<long> GetProgram()
        {
            var input = File.ReadAllText("Puzzles/Day15/input.txt");

            var program = input.Split(',')
                               .Select(long.Parse)
                               .ToList();

            var padding = Enumerable.Repeat((long)0, 10000 - program.Count);
            program.AddRange(padding);

            return program;
        }

        private Point GetNewPosition(Point location, MovementCommand direction)
        {
            if (direction == MovementCommand.North)
            {
                return new Point(location.X, location.Y + 1);
            }

            if (direction == MovementCommand.South)
            {
                return new Point(location.X, location.Y - 1);
            }

            if (direction == MovementCommand.West)
            {
                return new Point(location.X - 1, location.Y);
            }

            return new Point(location.X + 1, location.Y);
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

        public static class StatusCodes
        {
            public const int HitWall = 0;
            public const int MovedSuccessfully = 1;
            public const int AtOxygenSystem = 2;
        }

        [DebuggerDisplay("{Position} = {MovementCommand}")]
        private class Action
        {
            private readonly List<MovementCommand> _availableMovementCommands;

            public Action(Point position, MovementCommand movementCommand)
            {
                Position = position;
                MovementCommand = movementCommand;
                OriginalMovementCommand = movementCommand;

                _availableMovementCommands = MovementCommand.All.Except(new[] { movementCommand }).ToList();
            }

            public Point Position { get; }

            public MovementCommand MovementCommand { get; private set; }

            private MovementCommand OriginalMovementCommand { get; }

            public bool IsMovingBackwards => OriginalMovementCommand == MovementCommand.GetOpposite();

            public bool TryChangeDirection()
            {
                if (_availableMovementCommands.Count == 0)
                {
                    return false;
                }

                var backwardsDirection = OriginalMovementCommand.GetOpposite();

                var newDirection = _availableMovementCommands.Find(movementCommand => movementCommand != backwardsDirection)
                                   ?? backwardsDirection;

                _availableMovementCommands.Remove(newDirection);
                MovementCommand = newDirection;

                return true;
            }
        }

        private sealed class MovementCommand
        {
            public static readonly MovementCommand North = new MovementCommand(1);
            public static readonly MovementCommand South = new MovementCommand(2);
            public static readonly MovementCommand West = new MovementCommand(3);
            public static readonly MovementCommand East = new MovementCommand(4);

            public static readonly IEnumerable<MovementCommand> All = new[]
            {
                North, South, West, East
            };

            private MovementCommand(int value)
            {
                Value = value;
            }

            private int Value { get; }

            public MovementCommand GetOpposite()
            {
                if (this == North)
                {
                    return South;
                }

                if (this == South)
                {
                    return North;
                }

                return this == West ? East : West;
            }

            public static implicit operator int(MovementCommand input) => input.Value;
        }
    }
}