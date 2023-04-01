using DidacticSimulator.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidacticSimulator.Shared.Constants
{
    public class Instruction
    {
        public string Name { get; set; }
        public string Opcode { get; set; }
        public InstructionClass InstructionClass { get; set; }

        public Instruction(string name, string opcode, InstructionClass instructionClass)
        {
            Name = name;
            Opcode = opcode;
            InstructionClass = instructionClass;
        }

        public override string ToString()
        {
            return $"Name: {Name}, Opcode: {Opcode}, Class: {InstructionClass}";
        }
    }
}
