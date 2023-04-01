using DidacticSimulator.Shared.Constants;
using DidacticSimulator.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidacticSimulator.Utilities
{
    public class Assembler
    {
        public void ChangeInMachineCode(string pathFile)
        {
            InstructionCollection instructionCollection = new InstructionCollection();
            Dictionary<string, Instruction> instructions = instructionCollection.Instructions; 
            
            Parser parser = new Parser(pathFile);
            Dictionary<int, string> fileInstructions = parser.Instructions;
            Dictionary<string, int> labels = parser.Labels;

            
            using (StreamWriter writer = new StreamWriter(@"..\..\Shared\Files\OutputFile.txt"))
            {
                foreach (var fileInstruction in fileInstructions)
                {
                    writer.WriteLine(instructions[fileInstruction.Value].ToString());
                }
            }
        }
    }
}
