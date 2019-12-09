using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode2019.Puzzles.Day07
{
    public class Puzzle2 : IPuzzle
    {
        public object Solve()
        {
            var highestSignal = 0;

            var phaseSettingSequences = GetPhaseSettingSequences();

            foreach (var phaseSettingSequence in phaseSettingSequences)
            {
                var program1 = GetProgram();
                var input1 = new BlockingCollection<int>
                {
                    phaseSettingSequence.Item1,
                    0
                };

                var program2 = GetProgram();
                var input2 = new BlockingCollection<int>
                {
                    phaseSettingSequence.Item2
                };

                var program3 = GetProgram();
                var input3 = new BlockingCollection<int>
                {
                    phaseSettingSequence.Item3
                };

                var program4 = GetProgram();
                var input4 = new BlockingCollection<int>
                {
                    phaseSettingSequence.Item4
                };

                var program5 = GetProgram();
                var input5 = new BlockingCollection<int>
                {
                    phaseSettingSequence.Item5
                };

                var task1 = Task.Run(() =>
                {
                    var computer1 = new IntcodeComputer();

                    while (!computer1.IsHalted)
                    {
                        computer1.Run(program1, input1, input2);
                    }
                });

                var task2 = Task.Run(() =>
                {
                    var computer2 = new IntcodeComputer();

                    while (!computer2.IsHalted)
                    {
                        computer2.Run(program2, input2, input3);
                    }
                });

                var task3 = Task.Run(() =>
                {
                    var computer3 = new IntcodeComputer();

                    while (!computer3.IsHalted)
                    {
                        computer3.Run(program3, input3, input4);
                    }
                });

                var task4 = Task.Run(() =>
                {
                    var computer4 = new IntcodeComputer();

                    while (!computer4.IsHalted)
                    {
                        computer4.Run(program4, input4, input5);
                    }
                });

                var task5 = Task.Run(() =>
                {
                    var output5 = 0;

                    var computer5 = new IntcodeComputer();

                    while (!computer5.IsHalted)
                    {
                        output5 = computer5.Run(program5, input5, input1);
                    }

                    highestSignal = Math.Max(highestSignal, output5);
                });

                Task.WaitAll(task1, task2, task3, task4, task5);
            }

            return highestSignal;
        }

        private IEnumerable<(int, int, int, int, int)> GetPhaseSettingSequences()
        {
            for (var a = 5; a < 10; a++)
            {
                for (var b = 5; b < 10; b++)
                {
                    if (b == a)
                    {
                        continue;
                    }

                    for (var c = 5; c < 10; c++)
                    {
                        if (c == b || c == a)
                        {
                            continue;
                        }

                        for (var d = 5; d < 10; d++)
                        {
                            if (d == c || d == b || d == a)
                            {
                                continue;
                            }

                            for (var e = 5; e < 10; e++)
                            {
                                if (e == d || e == c || e == b || e == a)
                                {
                                    continue;
                                }

                                yield return (a, b, c, d, e);
                            }
                        }
                    }
                }
            }
        }

        private IList<int> GetProgram()
        {
            var input = File.ReadAllText("Puzzles\\Day07\\input.txt");

            var program = input.Split(',')
                               .Select(int.Parse)
                               .ToList();

            return program;
        }

        private class IntcodeComputer
        {
            private int _pointer;
            private IList<int> _program;
            private BlockingCollection<int> _input;
            private BlockingCollection<int> _output;
            private int _finalOutput;

            public bool IsHalted { get; private set; }

            public int Run(IList<int> program, BlockingCollection<int> input, BlockingCollection<int> output)
            {
                _pointer = 0;
                _program = program;
                _input = input;
                _output = output;
                _finalOutput = 0;

                while (true)
                {
                    var opcode = program[_pointer];
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
                            InputOperation();
                            break;

                        case OpCodes.StoreOutput:
                            OutputOperation();
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
                    }
                }

                return _finalOutput;
            }

            private IReadOnlyList<int> GetParameterModes(int opcode, int? overrideThirdParameterMode = null)
            {
                var firstParameterMode = (opcode / 100) % 10;
                var secondParameterMode = (opcode / 1000) % 10;
                var thirdParameterMode = overrideThirdParameterMode ?? (opcode / 10000) % 10;

                return new List<int> { firstParameterMode, secondParameterMode, thirdParameterMode };
            }

            private IReadOnlyList<int> GetParameterValues(IReadOnlyList<int> parameterModes, int numberOfParameters)
            {
                var parameterValues = new List<int>(numberOfParameters);

                for (var i = 0; i < numberOfParameters; i++)
                {
                    var parameterMode = parameterModes[i];

                    switch (parameterMode)
                    {
                        case Modes.Position:
                            var positionParameterValueIndex = _program[_pointer];
                            var positionParameterValue = _program[positionParameterValueIndex];
                            parameterValues.Add(positionParameterValue);
                            break;

                        case Modes.Immediate:
                            var immediateParameterValue = _program[_pointer];
                            parameterValues.Add(immediateParameterValue);
                            break;
                    }

                    _pointer++;
                }

                return parameterValues;
            }

            private void AddOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode, Modes.Immediate);
                var parameterValues = GetParameterValues(parameterModes, 3);

                var result = parameterValues[0] + parameterValues[1];
                var resultIndex = parameterValues[2];
                _program[resultIndex] = result;
            }

            private void MultiplyOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode, Modes.Immediate);
                var parameterValues = GetParameterValues(parameterModes, 3);

                var result = parameterValues[0] * parameterValues[1];
                var resultIndex = parameterValues[2];
                _program[resultIndex] = result;
            }

            private void InputOperation()
            {
                var parameterModes = new List<int> { Modes.Immediate };
                var parameterValues = GetParameterValues(parameterModes, 1);

                var inputIndex = parameterValues[0];
                _program[inputIndex] = _input.Take();
            }

            private void OutputOperation()
            {
                var parameterModes = new List<int> { Modes.Immediate };
                var parameterValues = GetParameterValues(parameterModes, 1);

                var outputIndex = parameterValues[0];
                var output = _program[outputIndex];
                _finalOutput = output;
                _output.Add(output);
            }

            private void JumpIfTrueOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode);
                var parameterValues = GetParameterValues(parameterModes, 2);

                var firstParameter = parameterValues[0];
                if (firstParameter != 0)
                {
                    var secondParameter = parameterValues[1];
                    _pointer = secondParameter;
                }
            }

            private void JumpIfFalseOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode);
                var parameterValues = GetParameterValues(parameterModes, 2);

                var firstParameter = parameterValues[0];
                if (firstParameter == 0)
                {
                    var secondParameter = parameterValues[1];
                    _pointer = secondParameter;
                }
            }

            private void LessThanOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode, Modes.Immediate);
                var parameterValues = GetParameterValues(parameterModes, 3);

                var firstParameter = parameterValues[0];
                var secondParameter = parameterValues[1];
                var result = firstParameter < secondParameter ? 1 : 0;

                var thirdParameter = parameterValues[2];
                _program[thirdParameter] = result;
            }

            private void EqualsOperation(int opcode)
            {
                var parameterModes = GetParameterModes(opcode, Modes.Immediate);
                var parameterValues = GetParameterValues(parameterModes, 3);

                var firstParameter = parameterValues[0];
                var secondParameter = parameterValues[1];
                var result = firstParameter == secondParameter ? 1 : 0;

                var thirdParameter = parameterValues[2];
                _program[thirdParameter] = result;
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
            public const int Halt = 99;
        }

        private static class Modes
        {
            public const int Position = 0;
            public const int Immediate = 1;
        }
    }
}