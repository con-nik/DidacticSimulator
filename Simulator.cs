using DidacticSimulator.Shared.Enums;
using DidacticSimulator.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace DidacticSimulator
{
    internal class Simulator
    {
        public int[] RG;
        public int PC, T, IR;
        public int SBUS;
        public int[] MEM;
        public int MAR;
        public long MIR;
        public long[] MPM;
        public bool BPO;
        public bool ACLOW, CIL, C, Z, S, V;

        public Simulator()
        {
            RG = new int[16];
            MEM = new int[65536];
            MPM = new long[116];
            MAR = 0;
            BPO = true;
            ACLOW = false;
            CIL = false;
            Z = false;
            C = false;
            S = false;
            V = false;
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

            while (BPO)
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
                        index = GetIndex(MIR);

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

                        break;
                    case SeqState.S3:

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
                    return ACLOW ^ valueMIR7;
                case 3:
                    return CIL ^ valueMIR7;
                case 4:
                    return C ^ valueMIR7;
                case 5:
                    return Z ^ valueMIR7;
                case 6:
                    return S ^ valueMIR7;
                case 7:
                    return V ^ valueMIR7;
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


        private int GetIndex(long MIR)
        {
            //get bits 10-8
            int index = Convert.ToInt32((MIR >> 8) & 0b111);
            return index;
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

        #endregion

        #region ComputeMIR

        private void ComputeSbusSource(SbusSource sbusSource)
        {

        }
        private void ComputeDbusSource(DbusSource dbusSource)
        {

        }
        private void ComputeALUOperation(ALUOperation aluOperation)
        {

        }
        private void ComputeRbusDestination(RbusDestination rbusDestination)
        {

        }
        private void ComputeOtherOperations(OtherOperations otherOperations)
        {

        }

        #endregion
    }
}
