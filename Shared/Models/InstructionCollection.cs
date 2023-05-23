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
        }

        public void InitializeInstructions()
        {
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

            foreach (var kvp in instructionsB1)
            {
                Instruction instruction = new Instruction(kvp.Key, kvp.Value, Enums.InstructionClass.B1);
                _instructions.Add(kvp.Key, instruction);
            }

            Dictionary<string, string> instructionsB2 = new Dictionary<string, string>()
            {
                { "CLR", "1010000000" },
                { "NEG", "1010000100" },
                { "INC", "1010001000" },
                { "DEC", "1010001100" },
                { "ASL", "1010010000" },
                { "ASR", "1010010100" },
                { "LSR", "1010011000" },
                { "ROL", "1010011100" },
                { "ROR", "1010100000" },
                { "RLC", "1010100100" },
                { "RRC", "1010101000" },
                { "PUSH", "1010101100" },
                { "POP", "1010110000" }
            };

            foreach (var kvp in instructionsB2)
            {
                Instruction instruction = new Instruction(kvp.Key, kvp.Value, Enums.InstructionClass.B2);
                _instructions.Add(kvp.Key, instruction);
            }

            Dictionary<string, string> instructionsB3 = new Dictionary<string, string>()
            {
                { "BEQ", "11000000" },
                { "BNE", "11000001" },
                { "BMI", "11000010" },
                { "BPL", "11000011" },
                { "BCS", "11000100" },
                { "BCC", "11000101" },
                { "BVS", "11000110" },
                { "BVC", "11000111" },
                { "JMP", "11001000" },
                { "CALL", "11001001" },

            };

            foreach (var kvp in instructionsB3)
            {
                Instruction instruction = new Instruction(kvp.Key, kvp.Value, Enums.InstructionClass.B3);
                _instructions.Add(kvp.Key, instruction);
            }

            Dictionary<string, string> instructionsB4 = new Dictionary<string, string>()
            {

                { "CLC", "1110000011110111" },
                { "CLZ", "1110000011111011" },
                { "CLS", "1110000011111101" },
                { "CLV", "1110000011111110" },
                { "CCC", "1110000011110000" },
                { "SEC", "1110000100001000" },
                { "SEZ", "1110000100000100" },
                { "SES", "1110000100000010" },
                { "SEV", "1110000100000001" },
                { "SCC", "1110000100001111" },
                { "NOP", "1110001000000000" },
                { "HALT", "1110001100000000" },
                { "EI", "1110010000000000" },
                { "DI", "1110010100000000" },
                { "PUSH PC", "1110011000000000" },
                { "POP PC", "1111011100000000" },
                { "PUSH FLAG", "1111100000000000" },
                { "POP FLAG", "1111100100000000" },
                { "RET", "1111101000000000" },
                { "RETI", "1111101100000000" },
                
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
