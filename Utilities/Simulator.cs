using DidacticSimulator.Shared.Enums;
using DidacticSimulator.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DidacticSimulator
{
    internal class Simulator
    {
        public int[] RG;
        public int PC, T, IR, SP, IVR, ADR, MDR, FLAG;
        public int SBUS, DBUS, RBUS;
        public int[] MEM;
        public int MAR;
        public long MIR;
        public long[] MPM;
        public int BPO, BVI;
        public int ACLOW, CIL, C, Z, S, V;
        public int CIN;
        public int BE, BI;

        public Simulator()
        {
            RG = new int[16];
            MEM = new int[65536];
            MPM = new long[116];
            MAR = 0;
            BPO = 1;
            ACLOW = 0;
            CIL = 0;
            Z = 0;
            C = 0;
            S = 0;
            V = 0;
            PC = 0;
            BE = 1;
            BI = 1;
        }

        public void LoadMicroprogram(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                MPM[i] = Convert.ToInt64(lines[i], 2);
            }
        }

        public void LoadProgram(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                MEM[i] = Convert.ToInt32(lines[i], 2);
            }
        }

        public void Simulate()
        {
            SeqState seqState = SeqState.S0;
            bool g;
            int microaddress, index;

            while (BPO == 1)
            {
                switch(seqState)
                {
                    case SeqState.S0:
                        MIR = MPM[MAR];
                        seqState = SeqState.S1;
                        break;
                    case SeqState.S1:
                        g = ComputeG(MIR);
                        microaddress = GetMicroaddress(MIR);
                        index = GetIndex(MIR, IR);

                        if (g)
                        {
                            MAR = microaddress + index;
                        }
                        else
                        {
                            MAR++;
                        }

                        SbusSource sbusSource = DecodeSbusSource(MIR);
                        DbusSource dbusSource = DecodeDbusSource(MIR);
                        ALUOperation aluOperation = DecodeALUOperation(MIR);
                        RbusDestination rbusDestination = DecodeRbusDestination(MIR);
                        OtherOperations otherOperations = DecodeOtherOperations(MIR);

                        ComputeSbusSource(sbusSource);
                        ComputeDbusSource(dbusSource);
                        ComputeALUOperation(aluOperation);
                        ComputeRbusDestination(rbusDestination);
                        ComputeOtherOperations(otherOperations);

                        seqState = SeqState.S2;
                        break;
                    case SeqState.S2:
                        seqState = SeqState.S3;
                        break;
                    case SeqState.S3:
                        MemoryOperation memoryOperation = DecodeMemoryOperation(MIR);
                        ComputeMemoryOperation(memoryOperation);
                        seqState = SeqState.S0;
                        break;
                }
            }
        }

        private bool ComputeG(long MIR)
        {
            // get bits 13 - 11
            long succesor = (MIR >> 11) & 0b111;
            int MIR7 = Convert.ToInt32((MIR >> 7) & 0b1);
            bool valueMIR7 = MIR7 != 0 ? true : false;

            switch(succesor)
            {
                case 0:
                    return valueMIR7 ^ false;
                case 1:
                    return !valueMIR7 ^ true;
                case 2:
                    return Convert.ToBoolean(ACLOW) ^ valueMIR7;
                case 3:
                    return Convert.ToBoolean(CIL) ^ valueMIR7;
                case 4:
                    return Convert.ToBoolean(C) ^ valueMIR7;
                case 5:
                    return Convert.ToBoolean(Z) ^ valueMIR7;
                case 6:
                    return Convert.ToBoolean(S) ^ valueMIR7;
                case 7:
                    return Convert.ToBoolean(V) ^ valueMIR7;
                default:
                    return false;
            }
        }

        #region DecodeMIR

        private int GetMicroaddress(long MIR)
        {
            //get bits 6-0
            int microaddress = Convert.ToInt32(MIR & 0b1111111);
            return microaddress;
        }


        private int GetIndex(long MIR, int IR)
        {
            //get bits 10-8
            int index = Convert.ToInt32((MIR >> 8) & 0b111);
            switch (index)
            {
                case 0:
                    return 0;
                case 1:
                    return getClass(IR);
                case 2:
                    return (IR >> 10) & 0b11;
                case 3:
                    return (IR >> 4) & 0b11;
                case 4:
                    return (IR >> 12) & 0b111;
                case 5:
                    return (IR >> 8) & 0b1111;
                case 6:
                    return (IR >> 7) & 0b11110;
                case 7:
                    // TODO: INTR
                    return 0;
                default:
                    return 0;
            }
        }

        private int getClass(int IR)
        {
            //get bit 15
            int bit15 = ((IR >> 15) & 0b1);
            //get bit 14
            int bit14 = ((IR >> 14) & 0b1);
            //get bit 13
            int bit13 = ((IR >> 15) & 0b1);

            if(bit15 == 0)
            {
                return 0;
            }
            else if(bit15 == 1 && bit14 == 0 && bit13 == 0) 
            {
                return 1;
            }
            else if(bit15 == 1 && bit14 == 1  && bit13 == 0)
            {
                return 2;
            }
            else if(bit15 == 1 && bit14 == 1 && bit13 == 1)
            {
                return 3;
            }

            return 0;
        }

        private SbusSource DecodeSbusSource(long MIR)
        {
            //get bits 35-32
            int sbusSource = Convert.ToInt32((MIR >> 32) & 0b1111);

            switch( sbusSource )
            {
                case 0:
                    return SbusSource.None;
                case 1:
                    return SbusSource.PdFlag;
                case 2:
                    return SbusSource.PdRG;
                case 3:
                    return SbusSource.PdSP;
                case 4:
                    return SbusSource.PdT;
                case 5:
                    return SbusSource.PDNotT;
                case 6:
                    return SbusSource.PdPC;
                case 7:
                    return SbusSource.PdIVR;
                case 8:
                    return SbusSource.PdADR;
                case 9:
                    return SbusSource.PdMDR;
                case 10:
                    return SbusSource.PDIR70;
                case 11:
                    return SbusSource.PD0;
                case 12:
                    return SbusSource.PdMinus1;
                default:
                    return SbusSource.None;
            }
        }


        private DbusSource DecodeDbusSource(long MIR)
        {
            //get bits 31-28
            int dbusSource = Convert.ToInt32((MIR >> 28) & 0b1111);

            switch (dbusSource)
            {
                case 0:
                    return DbusSource.None;
                case 1:
                    return DbusSource.PdFlag;
                case 2:
                    return DbusSource.PdRG;
                case 3:
                    return DbusSource.PdSP;
                case 4:
                    return DbusSource.PdT;
                case 5:
                    return DbusSource.PdPC;
                case 6:
                    return DbusSource.PdIVR;
                case 7:
                    return DbusSource.PdADR;
                case 8:
                    return DbusSource.PdMDR;
                case 9:
                    return DbusSource.PdNotMDR;
                case 10:
                    return DbusSource.PDIR70;
                case 11:
                    return DbusSource.PD0;
                case 12:
                    return DbusSource.PdMinus1;
                default:
                    return DbusSource.None;
            }
        }

        private ALUOperation DecodeALUOperation(long MIR)
        {
            //get bits 27-24
            int aluOperation = Convert.ToInt32((MIR >> 24) & 0b1111);

            switch (aluOperation)
            {
                case 0:
                    return ALUOperation.None;
                case 1:
                    return ALUOperation.SBUS;
                case 2:
                    return ALUOperation.DBUS;
                case 3:
                    return ALUOperation.ADD;
                case 4:
                    return ALUOperation.SUB;
                case 5:
                    return ALUOperation.AND;
                case 6:
                    return ALUOperation.OR;
                case 7:
                    return ALUOperation.XOR;
                case 8:
                    return ALUOperation.ASL;
                case 9:
                    return ALUOperation.ASR;
                case 10:
                    return ALUOperation.LSR;
                case 11:
                    return ALUOperation.ROL;
                case 12:
                    return ALUOperation.ROR;
                case 13:
                    return ALUOperation.RLC;
                case 14:
                    return ALUOperation.RRC;
                default:
                    return ALUOperation.None;
            }
        }

        private RbusDestination DecodeRbusDestination(long MIR)
        {
            //get bits 23-20
            int rbusDestination = Convert.ToInt32((MIR >> 20) & 0b1111);

            switch (rbusDestination)
            {
                case 0:
                    return RbusDestination.None;
                case 1:
                    return RbusDestination.PmFLAG;
                case 2:
                    return RbusDestination.PMFLAG30;
                case 3:
                    return RbusDestination.PmRG;
                case 4:
                    return RbusDestination.PmSP;
                case 5:
                    return RbusDestination.PmT;
                case 6:
                    return RbusDestination.PmPC;
                case 7:
                    return RbusDestination.PmIVR;
                case 8:
                    return RbusDestination.PmADR;
                case 9:
                    return RbusDestination.PmADR;
                default:
                    return RbusDestination.None;
            }
        }

        private OtherOperations DecodeOtherOperations(long MIR)
        {
            //get bits 17-14
            int otherOperations = Convert.ToInt32((MIR >> 14) & 0b1111);

            switch (otherOperations)
            {
                case 0:
                    return OtherOperations.None;
                case 1:
                    return OtherOperations.plus2SP;
                case 2:
                    return OtherOperations.minus2SP;
                case 3:
                    return OtherOperations.plus2PC;
                case 4:
                    return OtherOperations.A1BE0;
                case 5:
                    return OtherOperations.A1BE1;
                case 6:
                    return OtherOperations.PdCONDA;
                case 7:
                    return OtherOperations.CinPdCONDA;
                case 8:
                    return OtherOperations.PdCONDL;
                case 9:
                    return OtherOperations.A1BVI;
                case 10:
                    return OtherOperations.A0BVI;
                case 11:
                    return OtherOperations.A0BPO;
                case 12:
                    return OtherOperations.INTAminus2SP;
                case 13:
                    return OtherOperations.A0BEA0BI;
                default:
                    return OtherOperations.None;
            }
        }

        private MemoryOperation DecodeMemoryOperation(long MIR)
        {
            int memoryOperation = Convert.ToInt32((MIR >> 18) & 0b11);

            switch (memoryOperation)
            {
                case 0:
                    return MemoryOperation.None;
                case 1:
                    return MemoryOperation.IFCH;
                case 2:
                    return MemoryOperation.RD;
                case 3:
                    return MemoryOperation.WR;
                default:
                    return MemoryOperation.None;
            }
        }

        #endregion

        #region ComputeMIR

        private void ComputeSbusSource(SbusSource sbusSource)
        {
            switch(sbusSource)
            {
                case SbusSource.None:
                    break;
                case SbusSource.PdFlag:
                    SBUS = FLAG;
                    break;
                case SbusSource.PdRG:
                    int indexRG = (IR >> 6) & 0b1111;
                    SBUS = RG[indexRG];
                    break;
                case SbusSource.PdSP:
                    SBUS = SP;
                    break;
                case SbusSource.PdT:
                    SBUS = T;
                    break;
                case SbusSource.PDNotT:
                    SBUS = ~T;
                    break;
                case SbusSource.PdPC:
                    SBUS = PC;
                   break;
                case SbusSource.PdIVR:
                    SBUS = IVR;
                    break;
                case SbusSource.PdADR:
                    SBUS = ADR;
                    break;
                case SbusSource.PdMDR:
                    SBUS = MDR;
                    break;
                case SbusSource.PDIR70:
                    SBUS = IR & 0b11111111;
                    break;
                case SbusSource.PD0:
                    SBUS = 0;
                    break;
                case SbusSource.PdMinus1:
                    SBUS = -1;
                    break;
            }
        }
        private void ComputeDbusSource(DbusSource dbusSource)
        {
            switch(dbusSource)
            {
                case DbusSource.None:
                    break;
                case DbusSource.PdFlag:
                    DBUS = FLAG;
                    break;
                case DbusSource.PdRG:
                    int indexRG = IR & 0b1111;
                    DBUS = RG[indexRG];
                    break;
                case DbusSource.PdSP:
                    DBUS = SP;
                    break;
                case DbusSource.PdT:
                    DBUS = T;
                    break;
                case DbusSource.PdPC:
                    DBUS = PC;
                    break;
                case DbusSource.PdIVR:
                    DBUS = IVR;
                    break;
                case DbusSource.PdADR:
                    DBUS = ADR;
                    break;
                case DbusSource.PdMDR:
                    DBUS = MDR;
                    break;
                case DbusSource.PdNotMDR:
                    DBUS = ~MDR;  
                    break;
                case DbusSource.PDIR70:
                    SBUS = IR & 0b11111111;
                    break;
                case DbusSource.PD0:
                    DBUS = 0;
                    break;
                case DbusSource.PdMinus1:
                    DBUS = -1;
                    break;
            }
        }
        private void ComputeALUOperation(ALUOperation aluOperation)
        {
            switch (aluOperation)
            {
                case ALUOperation.None:
                    break;
                case ALUOperation.SBUS:
                    RBUS = SBUS;
                    break;
                case ALUOperation.DBUS:
                    RBUS = DBUS;
                    break;
                case ALUOperation.ADD:
                    RBUS = DBUS + SBUS;
                    break;
                case ALUOperation.SUB:
                    RBUS = DBUS - SBUS;
                    break;
                case ALUOperation.AND:
                    RBUS = DBUS & SBUS;
                    break;
                case ALUOperation.OR:
                    RBUS = DBUS | SBUS;
                    break;
                case ALUOperation.XOR:
                    RBUS = DBUS ^ SBUS; 
                    break;
                case ALUOperation.ASL:
                    RBUS = (DBUS << 1) & 0xFFFF;
                    break;
                case ALUOperation.ASR:
                    int bit16ASR = (DBUS >> 15) & 0b1;
                    if(bit16ASR == 0)
                    {
                        RBUS = (DBUS >> 1);
                    }
                    else
                    {
                        RBUS = (DBUS >> 1) | 0x8000;
                    }
                    break;
                case ALUOperation.LSR:
                    RBUS = (DBUS >> 1);
                    break;
                case ALUOperation.ROL:
                    int bit16ROL = (DBUS >> 15) & 0b1;
                    if(bit16ROL == 0) 
                    {
                        RBUS = (DBUS << 1);
                    }
                    else 
                    {
                        RBUS = (DBUS << 1) ^ 0b1;
                    }
                    break;
                case ALUOperation.ROR:
                    int bit1ROR = DBUS & 0b1;
                    if (bit1ROR == 0)
                    {
                        RBUS = (DBUS >> 1);
                    }
                    else
                    {
                        RBUS = (DBUS >> 1) ^ 0x8000;
                    }
                    break;
                case ALUOperation.RLC:
                    int bit16RLC = (DBUS >> 15) & 0b1;

                    if(C == 0)
                    {
                        RBUS = (DBUS << 1);
                    }
                    else
                    {
                        RBUS = (DBUS << 1) ^ 0b1;
                    }

                    C = bit16RLC;
                    break;
                case ALUOperation.RRC:
                    int bit1RRC = DBUS & 0b1;
                    if (C == 0)
                    {
                        RBUS = (DBUS >> 1);
                    }
                    else
                    {
                        RBUS = (DBUS >> 1) ^ 0x8000;
                    }
                    C = bit1RRC;
                    break;

            }
            
        }
        private void ComputeRbusDestination(RbusDestination rbusDestination)
        {
            switch (rbusDestination)
            {
                case RbusDestination.None:
                    break;
                case RbusDestination.PmFLAG:
                    FLAG = RBUS;
                    break;
                case RbusDestination.PMFLAG30:
                    FLAG = (FLAG & 0b10000) | (RBUS & 0b1111);
                    break;
                case RbusDestination.PmRG:
                    int indexRG = IR & 0b1111;
                    RG[indexRG] = RBUS;
                    break;
                case RbusDestination.PmSP:
                    SP = RBUS;
                    break;
                case RbusDestination.PmT:
                    T = RBUS;
                    break;
                case RbusDestination.PmPC:
                    PC = RBUS;
                    break;
                case RbusDestination.PmIVR:
                    IVR = RBUS;
                    break;
                case RbusDestination.PmADR:
                    ADR = RBUS;
                    break;
                case RbusDestination.PmMDR:
                    MDR = RBUS;
                    break;
            }
        }
        private void ComputeOtherOperations(OtherOperations otherOperations)
        {
            switch(otherOperations)
            {
                case OtherOperations.None:
                    break;
                case OtherOperations.plus2SP:
                    SP += 2;
                    break;
                case OtherOperations.minus2SP:
                    SP -= 2;
                    break;
                case OtherOperations.plus2PC:
                    PC += 1;
                    break;
                case OtherOperations.A1BE0:
                    ACLOW = 1;
                    break;
                case OtherOperations.A1BE1:
                    CIL = 1;
                    break;
                case OtherOperations.PdCONDA:
                    
                    break;
                case OtherOperations.CinPdCONDA: 

                    break;
                case OtherOperations.PdCONDL: 

                    break;
                case OtherOperations.A1BVI:
                    BVI = 1;
                    break;
                case OtherOperations.A0BVI:
                    BVI = 0;
                    break;
                case OtherOperations.A0BPO:
                    BPO = 0;
                    break;
                case OtherOperations.INTAminus2SP:
                    SP -= 2;
                    break;
                case OtherOperations.A0BEA0BI:
                    BE = 0;
                    BI = 0;
                    break;
            }
        }

        private void ComputeMemoryOperation(MemoryOperation memoryOperation)
        {
            switch(memoryOperation)
            {
                case MemoryOperation.None:
                    break;
                case MemoryOperation.IFCH:
                    IR = MEM[PC];
                    break;
                case MemoryOperation.RD:
                    if(ADR>0)
                    MDR = MEM[ADR];
                    break;
                case MemoryOperation.WR:
                    MEM[ADR] = MDR;
                    break;
            }
        }

        #endregion
    }
}
