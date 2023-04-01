using DidacticSimulator.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidacticSimulator.Shared.Models
{
    internal class InstructionCollection
    {
        private Dictionary<string, Instruction> _instructions;

        public InstructionCollection()
        {
            _instructions = new Dictionary<string, Instruction>();

            Dictionary<string, string> instructionsB1 = new Dictionary<string, string>()
            {
                { "MOV", "0000" },
                { "ADD", "0001" },
                { "SUB", "0010" },
                { "CMP", "0011" },
                { "AND", "0100" },
                { "OR", "0101" },
                { "XOR", "0110"}
            };
            
            foreach(var kvp in instructionsB1)
            {
                Instruction instruction = new Instruction(kvp.Key, kvp.Value, Enums.InstructionClass.B1);
                _instructions.Add(kvp.Key, instruction);
            }

            Dictionary<string, string> instructionsB2 = new Dictionary<string, string>()
            {
                { "CLR", "1000000000" },
                { "NEG", "1000000001" },
                { "INC", "1000000010" },
                { "DEC", "1000000011" },
                { "ASL", "1000000100" },
                { "ASR", "1000000101" },
                { "LSR", "1000000110" },
                { "ROL", "1000000111" },
                { "ROR", "1000001000" },
                { "RLC", "1000001001" },
                { "RRC", "1000001010" },
                { "JMP", "1000001011" },
                { "CALL", "1000001100" },
                { "PUSH", "1000001101" },
                { "POP", "1000001110" }
            };

            foreach (var kvp in instructionsB2)
            {
                Instruction instruction = new Instruction(kvp.Key, kvp.Value, Enums.InstructionClass.B2);
                _instructions.Add(kvp.Key, instruction);
            }

            Dictionary<string, string> instructionsB3 = new Dictionary<string, string>()
            {
                { "BR", "11000000" },
                { "BNE", "11000001" },
                { "BEQ", "11000010" },
                { "BPL", "11000011" },
                { "BMI", "11000100" },
                { "BCS", "11000101" },
                { "BCC", "11000110" },
                { "BVS", "11000111" },
                { "BVC", "11001000" }
            };

            foreach (var kvp in instructionsB3)
            {
                Instruction instruction = new Instruction(kvp.Key, kvp.Value, Enums.InstructionClass.B3);
                _instructions.Add(kvp.Key, instruction);
            }

            Dictionary<string, string> instructionsB4 = new Dictionary<string, string>()
            {
                { "NOP", "1110000000000000" },
                { "CLC", "1110000000000001" },
                { "CLV", "1110000000000010" },
                { "CLZ", "1110000000000011" },
                { "CLS", "1110000000000100" },
                { "CCC", "1110000000000101" },
                { "SEC", "1110000000000110" },
                { "SEV", "1110000000000111" },
                { "SEZ", "1110000000001000" },
                { "SES", "1110000000001001" },
                { "RET", "1110000000001010" },
                { "RETI", "1110000000001011" },
                { "HALT", "1110000000001100" },
                { "WAIT", "1110000000001101" },
                { "PUSH PC", "1110000000001110" },
                { "POP PC", "1110000000001111" },
                { "PUSH FLAG", "1110000000010000" },
                { "POP FLAG", "1110000000010001" },
                { "SCC", "1110000000010010" }
            };

            foreach (var kvp in instructionsB4)
            {
                Instruction instruction = new Instruction(kvp.Key, kvp.Value, Enums.InstructionClass.B4);
                _instructions.Add(kvp.Key, instruction);
            }


        }

        public Dictionary<string, Instruction> Instructions => _instructions;
    }
}
