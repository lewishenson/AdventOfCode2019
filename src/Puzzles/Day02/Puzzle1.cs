using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day02
{
    public class Puzzle1 : IPuzzle
    {
        public object Solve()
        {
            var program = GetProgram();
            RunProgram(program);

            return program[0];
        }

        private IList<int> GetProgram()
        {
            var input = File.ReadAllText("Puzzles/Day02/input.txt");

            var program = input.Split(',')
                               .Select(int.Parse)
                               .ToList();

            // As instructed
            program[1] = 12;
            program[2] = 2;

            return program;
        }

        private void RunProgram(IList<int> program)
        {
            var index = 0;
            var continueRunning = true;

            while (continueRunning)
            {
                var opcode = program[index];

                switch (opcode)
                {
                    case OpCodes.Add:
                        PerformAddition(program, index);
                        break;

                    case OpCodes.Multiply:
                        PerformMultiplication(program, index);
                        break;

                    case OpCodes.Halt:
                        continueRunning = false;
                        break;
                }

                index += 4;
            }
        }

        private void PerformAddition(IList<int> program, int operatorIndex)
        {
            PerformOperation(program, operatorIndex, (a, b) => a + b);
        }

        private void PerformMultiplication(IList<int> program, int operatorIndex)
        {
            PerformOperation(program, operatorIndex, (a, b) => a * b);
        }

        private void PerformOperation(IList<int> program, int operatorIndex, Func<int, int, int> operation)
        {
            var firstOperandIndex = program[operatorIndex + 1];
            var firstOperand = program[firstOperandIndex];

            var secondOperandIndex = program[operatorIndex + 2];
            var secondOperand = program[secondOperandIndex];

            var result = operation(firstOperand, secondOperand);
            var resultIndex = program[operatorIndex + 3];
            program[resultIndex] = result;
        }

        private static class OpCodes
        {
            public const int Add = 1;
            public const int Multiply = 2;
            public const int Halt = 99;
        }
    }
}