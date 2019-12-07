using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day07
{
    public class Puzzle1 : IPuzzle
    {
        public int Solve()
        {
            var highestSignal = 0;

            var phaseSettingSequences = GetPhaseSettingSequences();

            foreach (var phaseSettingSequence in phaseSettingSequences)
            {
                var computer1 = new IntcodeComputer();
                var program1 = GetProgram();
                var input1 = new Queue<int>();
                input1.Enqueue(phaseSettingSequence.Item1);
                input1.Enqueue(0);
                var output1 = computer1.Run(program1, input1);

                var computer2 = new IntcodeComputer();
                var program2 = GetProgram();
                var input2 = new Queue<int>();
                input2.Enqueue(phaseSettingSequence.Item2);
                input2.Enqueue(output1);
                var output2 = computer2.Run(program2, input2);

                var computer3 = new IntcodeComputer();
                var program3 = GetProgram();
                var input3 = new Queue<int>();
                input3.Enqueue(phaseSettingSequence.Item3);
                input3.Enqueue(output2);
                var output3 = computer3.Run(program3, input3);

                var computer4 = new IntcodeComputer();
                var program4 = GetProgram();
                var input4 = new Queue<int>();
                input4.Enqueue(phaseSettingSequence.Item4);
                input4.Enqueue(output3);
                var output4 = computer4.Run(program4, input4);

                var computer5 = new IntcodeComputer();
                var program5 = GetProgram();
                var input5 = new Queue<int>();
                input5.Enqueue(phaseSettingSequence.Item5);
                input5.Enqueue(output4);
                var output5 = computer5.Run(program5, input5);

                highestSignal = Math.Max(highestSignal, output5);
            }

            return highestSignal;
        }

        private IEnumerable<(int, int, int, int, int)> GetPhaseSettingSequences()
        {
            for (var a = 0; a < 5; a++)
            {
                for (var b = 0; b < 5; b++)
                {
                    if (b == a)
                    {
                        continue;
                    }

                    for (var c = 0; c < 5; c++)
                    {
                        if (c == b || c == a)
                        {
                            continue;
                        }

                        for (var d = 0; d < 5; d++)
                        {
                            if (d == c || d == b || d == a)
                            {
                                continue;
                            }

                            for (var e = 0; e < 5; e++)
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
            private Queue<int> _input;
            private int _output;

            public int Run(IList<int> program, Queue<int> input)
            {
                _pointer = 0;
                _program = program;
                _input = input;
                _output = 0;

                while (true)
                {
                    var opcode = program[_pointer];
                    _pointer++;

                    if (opcode == OpCodes.Halt)
                    {
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

                return _output;
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
                _program[inputIndex] = _input.Dequeue();
            }

            private void OutputOperation()
            {
                var parameterModes = new List<int> { Modes.Immediate };
                var parameterValues = GetParameterValues(parameterModes, 1);

                var outputIndex = parameterValues[0];
                _output = _program[outputIndex];
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