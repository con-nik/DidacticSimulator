using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidacticSimulator.Shared.Enums
{ 
    public enum ALUOperation
    {
        None,
        SBUS,
        DBUS,
        ADD,
        SUB,
        AND,
        OR,
        XOR,
        ASL,
        ASR,
        LSR,
        ROL,
        ROR,
        RLC,
        RRC
    }
}
