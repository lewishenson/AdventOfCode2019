using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day05
{
    public class Puzzle2 : IPuzzle
    {
        public object Solve()
        {
            var computer = new IntcodeComputer();

            var program = GetProgram();
            var output = computer.Run(program, 5);

            return output;
        }

        private IList<int> GetProgram()
        {
            var input = File.ReadAllText("Puzzles/Day05/input.txt");

            var program = input.Split(',')
                               .Select(int.Parse)
                               .ToList();

            return program;
        }

        private class IntcodeComputer
        {
            private int _pointer;
            private IList<int> _program;
            private int _input;
            private int _output;

            public int Run(IList<int> program, int input)
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
                _program[inputIndex] = _input;
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