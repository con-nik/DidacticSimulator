using DidacticSimulator.Shared.Constants;
using DidacticSimulator.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace DidacticSimulator.Utilities
{
    public class Assembler
    {
        private InstructionCollection instructionCollection;
        private Dictionary<string, Instruction> instructions;
        private Dictionary<int, string> fileInstructions;
        private Dictionary<string, int> labels;
        private Dictionary<int, List<string>> operands;
        private Dictionary<int, string> lines;

        private List<string> result;

        public Assembler() 
        {
            instructionCollection = new InstructionCollection();
            instructions = new Dictionary<string, Instruction>();
            fileInstructions = new Dictionary<int, string>();
            labels = new Dictionary<string, int>();
            operands = new Dictionary<int, List<string>>();
            lines = new Dictionary<int, string>();
            result = new List<string>();
        }

        public void ChangeInMachineCode(string pathFile)
        {
            instructionCollection.InitializeInstructions();
            instructions = instructionCollection.Instructions;

            Parser parser = new Parser();
            parser.ParseFile(pathFile);

            fileInstructions = parser.Instructions;
            labels = parser.Labels;
            operands = parser.Operands;
            lines = parser.Lines;



            int index = 0;
            foreach (var fileInstruction in fileInstructions)
            {
                switch (instructions[fileInstruction.Value].InstructionClass)
                {
                    case Shared.Enums.InstructionClass.B1:
                        ComputeB1Instruction(index, fileInstruction);
                        break;

                    case Shared.Enums.InstructionClass.B2:
                        ComputeB2Instruction(index, fileInstruction);
                        break;

                    case Shared.Enums.InstructionClass.B3:
                        ComputeB3Instruction(index, fileInstruction);
                        break;

                    case Shared.Enums.InstructionClass.B4:
                        result.Add(instructions[fileInstruction.Value].Opcode);
                        break;
                }
                index++;
            }

            using (StreamWriter writer = new StreamWriter(@"..\..\Shared\Files\OutputFile.txt"))
            {
                foreach(string line in result)
                {
                    writer.WriteLine(line);
                }
            }
        }

        private void ComputeB1Instruction(int index, KeyValuePair<int, string> fileInstruction)
        {
            string binaryValue = instructions[fileInstruction.Value].Opcode;
            binaryValue += operands[index][2];

            if (operands[index][2] == "00")
                binaryValue += "0000";

            if (operands[index][2] == "01" || operands[index][2] == "10" || operands[index][2] == "11")
            {
                Match match = Regex.Match(operands[index][3], @"R(\d+)");
                if (match.Success)
                {
                    string numberString = match.Groups[1].Value;
                    string binaryNumberString = Convert.ToString(int.Parse(numberString), 2).PadLeft(4, '0');
                    binaryValue += binaryNumberString;
                }
            }

            binaryValue += operands[index][0];

            if (operands[index][0] == "00")
                binaryValue += "0000";

            if (operands[index][0] == "01" || operands[index][0] == "10" || operands[index][0] == "11")
            {
                Match match = Regex.Match(operands[index][1], @"R(\d+)");
                if (match.Success)
                {
                    string numberString = match.Groups[1].Value;
                    string binaryNumberString = Convert.ToString(int.Parse(numberString), 2).PadLeft(4, '0');
                    binaryValue += binaryNumberString;
                }
            }

            result.Add(binaryValue);

            if (operands[index][2] == "00")
            {
                string binaryNumberString = Convert.ToString(int.Parse(operands[index][3]), 2).PadLeft(16, '0');
                result.Add(binaryNumberString);
            }

            if (operands[index][2] == "11")
            {
                Match match = Regex.Match(operands[index][3], @"(\d+)\(");

                if (match.Success)
                {
                    string numberString = match.Groups[1].Value;
                    string binaryNumberString = Convert.ToString(int.Parse(numberString), 2).PadLeft(16, '0');
                    result.Add(binaryNumberString);
                }
            }

            if (operands[index][0] == "11")
            {
                Match match = Regex.Match(operands[index][1], @"(\d+)\(");

                if (match.Success)
                {
                    string numberString = match.Groups[1].Value;
                    string binaryNumberString = Convert.ToString(int.Parse(numberString), 2).PadLeft(16, '0');
                    result.Add(binaryNumberString);
                }
            }
        }

        private void ComputeB2Instruction(int index, KeyValuePair<int, string> fileInstruction)
        {
            string binaryValue = instructions[fileInstruction.Value].Opcode;

            if (operands[index].Count != 0)
            {
                binaryValue += operands[index][0];
            }
            else
            {
                binaryValue += "000000";
                string[] words = lines[index].Split(' ');
                string lastWord = words[words.Length - 1];

                if (labels.ContainsKey(lastWord))
                {
                    int diferenta = labels[lastWord] - index + 1;
                    string binaryNumberString2 = Convert.ToString(diferenta, 2).PadLeft(16, '0');
                    binaryNumberString2 = binaryNumberString2.Substring(binaryNumberString2.Length - 16);
                    result.Add(binaryValue);
                    result.Add(binaryNumberString2);
                    return;
                }
            }


            if (operands[index][0] == "00")
            {
                binaryValue += "0000";
            }

            if (operands[index][0] == "01" || operands[index][0] == "10" || operands[index][0] == "11")
            {
                Match match = Regex.Match(operands[index][1], @"R(\d+)");
                if (match.Success)
                {
                    string numberString = match.Groups[1].Value;
                    string binaryNumberString = Convert.ToString(int.Parse(numberString), 2).PadLeft(4, '0');
                    binaryValue += binaryNumberString;
                }
            }

            result.Add(binaryValue);

            if (operands[index][0] == "00")
            {
                int decimalValue = Convert.ToInt32(operands[index][1], 16);
                string binaryString = Convert.ToString(decimalValue, 2).PadLeft(16, '0');
                result.Add(binaryString);
            }

            if (operands[index][0] == "11")
            {
                Match match = Regex.Match(operands[index][1], @"(\d+)\(");

                if (match.Success)
                {
                    string numberString = match.Groups[1].Value;
                    string binaryNumberString = Convert.ToString(int.Parse(numberString), 2).PadLeft(16, '0');
                    result.Add(binaryNumberString);
                }
            }
        }

        private void ComputeB3Instruction(int index, KeyValuePair<int, string> fileInstruction)
        {
            string binaryValue2 = instructions[fileInstruction.Value].Opcode;

            string[] words = lines[index].Split(' ');
            string secondWord = words[words.Length - 1];


            if (labels.ContainsKey(secondWord))
            {
                int diferenta = labels[secondWord] - index + 1;
                string binaryNumberString1 = Convert.ToString(diferenta, 2).PadLeft(8, '0');
                binaryNumberString1 = binaryNumberString1.Substring(binaryNumberString1.Length - 8);
                binaryValue2 += binaryNumberString1;
                result.Add(binaryValue2);
            }
            else
            {
                int decimalValue = Convert.ToInt32(secondWord.Substring(0, secondWord.Length - 1), 16);
                string binaryString = Convert.ToString(decimalValue, 2).PadLeft(16, '0');
                binaryValue2 = binaryValue2 + "00000000";
                result.Add(binaryValue2);
                result.Add(binaryString);
            }
        }
    }
}
