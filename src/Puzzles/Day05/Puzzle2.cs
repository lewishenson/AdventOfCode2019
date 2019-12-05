using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day05
{
    public class Puzzle2 : IPuzzle
    {
        public int Solve()
        {
            var program = GetProgram();
            var output = RunProgram(program, 5);

            return output;
        }

        private IList<int> GetProgram()
        {
            var input = File.ReadAllText("Puzzles\\Day05\\input.txt");

            var program = input.Split(',')
                               .Select(int.Parse)
                               .ToList();

            return program;
        }

        private int RunProgram(IList<int> program, int input)
        {
            var index = 0;
            var continueRunning = true;
            var programOutput = 0;

            while (continueRunning)
            {
                var opcode = program[index];

                if (opcode == OpCodes.Halt)
                {
                    return programOutput;
                }

                var instruction = opcode % 100;
                var firstParameterMode = (opcode / 100) % 10;
                var secondParameterMode = (opcode / 1000) % 10;

                var processInstructionOutputs = ProcessInstruction(program, instruction, index, firstParameterMode, secondParameterMode, input);

                if (index == processInstructionOutputs.NewIndex)
                {
                    var x = index;
                }

                index = processInstructionOutputs.NewIndex;

                if (processInstructionOutputs.Output.HasValue)
                {
                    programOutput = processInstructionOutputs.Output.Value;
                }

                continueRunning = processInstructionOutputs.ContinueRunning;
            }

            return programOutput;
        }

        private ProcessInstructionOutputs ProcessInstruction(IList<int> program, int instruction, int currentIndex, int firstParameterMode, int secondParameterMode, int input)
        {
            var result = new ProcessInstructionOutputs
            {
                ContinueRunning = true,
                NewIndex = currentIndex
            };

            switch (instruction)
            {
                case OpCodes.Add:
                    PerformAddition(program, currentIndex, firstParameterMode, secondParameterMode);
                    result.NewIndex += 4;
                    break;

                case OpCodes.Multiply:
                    PerformMultiplication(program, currentIndex, firstParameterMode, secondParameterMode);
                    result.NewIndex += 4;
                    break;

                case OpCodes.StoreInput:
                    WriteInput(program, currentIndex, input);
                    result.NewIndex += 2;
                    break;

                case OpCodes.StoreOutput:
                    result.Output = ReadOutput(program, currentIndex);
                    result.NewIndex += 2;
                    break;

                case OpCodes.JumpIfTrue:
                    PerformJumpIfTrue(program, currentIndex, firstParameterMode, secondParameterMode, result);
                    break;

                case OpCodes.JumpIfFalse:
                    PerformJumpIfFalse(program, currentIndex, firstParameterMode, secondParameterMode, result);
                    break;

                case OpCodes.LessThan:
                    PerformLessThan(program, currentIndex, firstParameterMode, secondParameterMode);
                    result.NewIndex += 4;
                    break;

                case OpCodes.Equal:
                    PerformEquals(program, currentIndex, firstParameterMode, secondParameterMode);
                    result.NewIndex += 4;
                    break;
            }

            return result;
        }

        private void PerformAddition(IList<int> program, int operatorIndex, int firstParameterMode, int secondParameterMode)
        {
            PerformOperation(program, operatorIndex, firstParameterMode, secondParameterMode, (a, b) => a + b);
        }

        private void PerformMultiplication(IList<int> program, int operatorIndex, int firstParameterMode, int secondParameterMode)
        {
            PerformOperation(program, operatorIndex, firstParameterMode, secondParameterMode, (a, b) => a * b);
        }

        private void PerformOperation(IList<int> program, int operatorIndex, int firstParameterMode, int secondParameterMode, Func<int, int, int> operation)
        {
            var firstOperandIndex = program[operatorIndex + 1];
            var firstOperand = firstParameterMode == Modes.Position ? program[firstOperandIndex] : firstOperandIndex;

            var secondOperandIndex = program[operatorIndex + 2];
            var secondOperand = secondParameterMode == Modes.Position ? program[secondOperandIndex] : secondOperandIndex;

            var result = operation(firstOperand, secondOperand);
            var resultIndex = program[operatorIndex + 3];
            program[resultIndex] = result;
        }

        private void WriteInput(IList<int> program, int index, int input)
        {
            var storeIndex = program[index + 1];
            program[storeIndex] = input;
        }

        private int ReadOutput(IList<int> program, int index)
        {
            var storeIndex = program[index + 1];

            return program[storeIndex];
        }

        private void PerformJumpIfTrue(IList<int> program, int currentIndex, int firstParameterMode, int secondParameterMode, ProcessInstructionOutputs result)
        {
            var firstOperandIndex = program[currentIndex + 1];
            var firstOperand = firstParameterMode == Modes.Position ? program[firstOperandIndex] : firstOperandIndex;
            if (firstOperand != 0)
            {
                var secondOperandIndex = program[currentIndex + 2];
                var secondOperand = secondParameterMode == Modes.Position ? program[secondOperandIndex] : secondOperandIndex;

                result.NewIndex = secondOperand;
            }
            else
            {
                result.NewIndex += 3;
            }
        }

        private void PerformJumpIfFalse(IList<int> program, int currentIndex, int firstParameterMode, int secondParameterMode, ProcessInstructionOutputs result)
        {
            var firstOperandIndex = program[currentIndex + 1];
            var firstOperand = firstParameterMode == Modes.Position ? program[firstOperandIndex] : firstOperandIndex;
            if (firstOperand == 0)
            {
                var secondOperandIndex = program[currentIndex + 2];
                var secondOperand = secondParameterMode == Modes.Position ? program[secondOperandIndex] : secondOperandIndex;

                result.NewIndex = secondOperand;
            }
            else
            {
                result.NewIndex += 3;
            }
        }

        private void PerformLessThan(IList<int> program, int currentIndex, int firstParameterMode, int secondParameterMode)
        {
            var firstOperandIndex = program[currentIndex + 1];
            var firstOperand = firstParameterMode == Modes.Position ? program[firstOperandIndex] : firstOperandIndex;

            var secondOperandIndex = program[currentIndex + 2];
            var secondOperand = secondParameterMode == Modes.Position ? program[secondOperandIndex] : secondOperandIndex;

            var thirdOperandIndex = program[currentIndex + 3];
            var thirdOperand = firstOperand < secondOperand ? 1 : 0;
            program[thirdOperandIndex] = thirdOperand;
        }

        private void PerformEquals(IList<int> program, int currentIndex, int firstParameterMode, int secondParameterMode)
        {
            var firstOperandIndex = program[currentIndex + 1];
            var firstOperand = firstParameterMode == Modes.Position ? program[firstOperandIndex] : firstOperandIndex;

            var secondOperandIndex = program[currentIndex + 2];
            var secondOperand = secondParameterMode == Modes.Position ? program[secondOperandIndex] : secondOperandIndex;

            var thirdOperandIndex = program[currentIndex + 3];
            var thirdOperand = firstOperand == secondOperand ? 1 : 0;
            program[thirdOperandIndex] = thirdOperand;
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

        private class ProcessInstructionOutputs
        {
            public int NewIndex { get; set; }

            public int? Output { get; set; }

            public bool ContinueRunning { get; set; }
        }
    }
}