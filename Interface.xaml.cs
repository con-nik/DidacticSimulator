using DidacticSimulator.Shared.Enums;
using DidacticSimulator.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DidacticSimulator
{
    /// <summary>
    /// Interaction logic for Interface.xaml
    /// </summary>
    public partial class Interface : Window
    {

        Assembler assembler;
        public short[] RG;
        public short PC, T, IR, SP, IVR, MDR, FLAG;
        public ushort ADR;
        public short SBUS, DBUS, RBUS;
        public short[] MEM;
        public int MAR;
        public long MIR;
        public long[] MPM;
        public string[] MPMText;
        public int BPO, BVI;
        public int ACLOW, CIL, C, Z, S, V;
        public int CIN;
        public int BE, BI;
        public string inputFilePath;


        public Interface()
        {
            InitializeComponent();
            Clear();
            assembler = new Assembler();

            #region Initialize

            RG = new short[16];

            for(int i=0; i < RG.Length; i++)
            {
                RG[i] = 0;
            }
            MEM = new short[65536];

            for(int i=0;i < MEM.Length; i++)
            {
                MEM[i] = 0;
            }
            MPM = new long[116];

            for(int i=0; i<MPM.Length; i++)
            {
                MPM[i] = 0;
            }

            MPMText = new string[116];
            MAR = 0;
            BPO = 1;
            BVI = 1;
            ACLOW = 0;
            CIL = 0;
            Z = C = S = V = 0;
            PC = -1;
            T = 0;
            IR = 0;
            SP = 0;
            IVR = 0;
            MDR = 0;
            FLAG = 0;
            ADR = 0;
            SBUS = 0;
            DBUS = 0;
            RBUS = 0;
            BE = 1;
            BI = 1;
            inputFilePath = @"..\..\Shared\Files\InputFile.txt";

            #endregion
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            assembler.ChangeInMachineCode(inputFilePath);
            LoadMicroprogram(@"..\..\Shared\Files\Microprogram.txt", @"..\..\Shared\Files\MicroprogramText.txt");
            LoadProgram(@"..\..\Shared\Files\Program.txt");
            await Simulate();
        }

        public void LoadMicroprogram(string filePath, string filePathText)
        {
            string[] lines = File.ReadAllLines(filePath);
            
            for (int i = 0; i < lines.Length; i++)
            {
                MPM[i] = Convert.ToInt64(lines[i], 2);
            }

            string[] linesText = File.ReadAllLines(filePathText);

            for (int i = 0; i < linesText.Length; i++)
            {
                MPMText[i] = linesText[i];
            }
        }

        public void LoadProgram(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                MEM[i] = Convert.ToInt16(lines[i], 2);
            }
        }

        public async Task Simulate()
        {
            SeqState seqState = SeqState.S0;
            bool g;
            int microaddress, index;

            while (BPO == 1)
            {
                switch (seqState)
                {
                    case SeqState.S0:
                        MIR = MPM[MAR];
                        microinstruction.Content = MPMText[MAR];
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

                        int bit18 = (int)(MIR >> 18) & 0b1;
                        int bit19 = (int)(MIR >> 19) & 0b1;

                        if(bit18 == 0 && bit19 == 0)
                        {
                            seqState = SeqState.S0;
                        }
                        else
                        {
                            seqState = SeqState.S2;
                        }

                        SbusSource sbusSource = DecodeSbusSource(MIR);
                        DbusSource dbusSource = DecodeDbusSource(MIR);
                        ALUOperation aluOperation = DecodeALUOperation(MIR);
                        RbusDestination rbusDestination = DecodeRbusDestination(MIR);
                        OtherOperations otherOperations = DecodeOtherOperations(MIR);

                        await ComputeOtherOperations(otherOperations);
                        await ComputeSbusSource(sbusSource);
                        await ComputeDbusSourceAsync(dbusSource);
                        await ComputeALUOperation(aluOperation);
                        await ComputeRbusDestinationAsync(rbusDestination);
                        

                        seqState = SeqState.S2;
                        break;
                    case SeqState.S2:
                        seqState = SeqState.S3;
                        break;
                    case SeqState.S3:
                        MemoryOperation memoryOperation = DecodeMemoryOperation(MIR);
                        await ComputeMemoryOperation(memoryOperation);
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
            bool valueMIR7 = MIR7 != 0;

            switch (succesor)
            {
                case 0:
                    return false;
                case 1:
                    return true;
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
            int bit13 = ((IR >> 13) & 0b1);

            if (bit15 == 0)
            {
                return 0;
            }
            else if (bit15 == 1 && bit14 == 0 && bit13 == 0)
            {
                return 1;
            }
            else if (bit15 == 1 && bit14 == 1 && bit13 == 0)
            {
                return 2;
            }
            else if (bit15 == 1 && bit14 == 1 && bit13 == 1)
            {
                return 3;
            }

            return 0;
        }

        private SbusSource DecodeSbusSource(long MIR)
        {
            //get bits 35-32
            int sbusSource = Convert.ToInt32((MIR >> 32) & 0b1111);

            switch (sbusSource)
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
                    return RbusDestination.PmMDR;
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

        private async Task ComputeSbusSource(SbusSource sbusSource)
        {
            switch (sbusSource)
            {
                case SbusSource.None:
                    break;

                case SbusSource.PdFlag:
                    SBUS = FLAG;
                    await ChangeTwoBordersBrush(FLAGSBUS, SBUSBorder);
                    break;

                case SbusSource.PdRG:
                    int indexRG = (IR >> 6) & 0b1111;
                    SBUS = RG[indexRG];
                    await ChangeTwoBordersBrush(RGSBUS, SBUSBorder);
                    break;

                case SbusSource.PdSP:
                    SBUS = SP;
                    await ChangeTwoBordersBrush(SPSBUS, SBUSBorder);
                    break;

                case SbusSource.PdT:
                    SBUS = T;
                    await ChangeTwoBordersBrush(TSBUS, SBUSBorder);
                    break;

                case SbusSource.PDNotT:
                    SBUS = Convert.ToInt16(~T);
                    await ChangeTwoBordersBrush(TSBUS, SBUSBorder);
                    break;

                case SbusSource.PdPC:
                    SBUS = PC;
                    await ChangeTwoBordersBrush(PCSBUS, SBUSBorder);
                    break;

                case SbusSource.PdIVR:
                    SBUS = IVR;
                    await ChangeTwoBordersBrush(IVRSBUS, SBUSBorder);
                    break;

                case SbusSource.PdADR:
                    SBUS = (short)ADR;
                    await ChangeTwoBordersBrush(ADRSBUS, SBUSBorder);
                    break;

                case SbusSource.PdMDR:
                    SBUS = MDR;
                    await ChangeTwoBordersBrush(MDRSBUS, SBUSBorder);
                    break;

                case SbusSource.PDIR70:
                    SBUS = Convert.ToInt16(IR & 0b11111111);
                    await ChangeTwoBordersBrush(IRSBUS, SBUSBorder);
                    break;

                case SbusSource.PD0:
                    SBUS = 0;
                    await ChangeOneBorderColor(SBUSBorder);
                    break;

                case SbusSource.PdMinus1:
                    SBUS = -1;
                    await ChangeOneBorderColor(SBUSBorder);
                    break;
            }
        }
        private async Task ComputeDbusSourceAsync(DbusSource dbusSource)
        {
            switch (dbusSource)
            {
                case DbusSource.None:
                    break;
                case DbusSource.PdFlag:
                    DBUS = FLAG;
                    await ChangeTwoBordersBrush(FLAGDBUS, DBUSBorder);
                    break;

                case DbusSource.PdRG:
                    int indexRG = IR & 0b1111;
                    DBUS = RG[indexRG];
                    await ChangeTwoBordersBrush(RGDBUS, DBUSBorder);
                    break;

                case DbusSource.PdSP:
                    DBUS = SP;
                    await ChangeTwoBordersBrush(SPDBUS, DBUSBorder);
                    break;

                case DbusSource.PdT:
                    DBUS = T;
                    await ChangeTwoBordersBrush(TDBUS, DBUSBorder);
                    break;

                case DbusSource.PdPC:
                    DBUS = PC;
                    await ChangeTwoBordersBrush(PCDBUS, DBUSBorder);
                    break;
                case DbusSource.PdIVR:
                    DBUS = IVR;
                    await ChangeTwoBordersBrush(IVRDBUS, DBUSBorder);
                    break;
                case DbusSource.PdADR:
                    DBUS = (short)ADR;
                    await ChangeTwoBordersBrush(ADRDBUS, DBUSBorder);
                    break;

                case DbusSource.PdMDR:
                    DBUS = MDR;
                    await ChangeTwoBordersBrush(MDRDBUS, DBUSBorder);
                    break;

                case DbusSource.PdNotMDR:
                    DBUS = Convert.ToInt16(~MDR);
                    await ChangeTwoBordersBrush(MDRDBUS, DBUSBorder);
                    break;

                case DbusSource.PDIR70:
                    SBUS = Convert.ToInt16(IR & 0b11111111);
                    await ChangeTwoBordersBrush(IRDBUS, DBUSBorder);
                    break;

                case DbusSource.PD0:
                    DBUS = 0;
                    await ChangeOneBorderColor(DBUSBorder);
                    break;

                case DbusSource.PdMinus1:
                    DBUS = -1;
                    await ChangeOneBorderColor(DBUSBorder);
                    break;
            }
        }
        private async Task ComputeALUOperation(ALUOperation aluOperation)
        {
            switch (aluOperation)
            {
                case ALUOperation.None:
                    break;

                case ALUOperation.SBUS:
                    RBUS = SBUS;
                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.DBUS:
                    RBUS = DBUS;
                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.ADD:
                    RBUS = Convert.ToInt16(DBUS + SBUS);
                    setFlags((ushort)DBUS, (ushort)SBUS);
                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.SUB:
                    RBUS = Convert.ToInt16(SBUS - DBUS);
                    setFlags((ushort)DBUS, (ushort)SBUS);
                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.AND:
                    RBUS = Convert.ToInt16(DBUS & SBUS);
                    setFlags((ushort)DBUS, (ushort)SBUS);
                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.OR:
                    RBUS = Convert.ToInt16(DBUS | SBUS);
                    setFlags((ushort)DBUS, (ushort)SBUS);
                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.XOR:
                    RBUS = Convert.ToInt16(DBUS ^ SBUS);
                    setFlags((ushort)DBUS, (ushort)SBUS);
                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.ASL:
                    RBUS = Convert.ToInt16((DBUS << 1) & 0xFFFF);
                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.ASR:
                    int bit16ASR = (DBUS >> 15) & 0b1;
                    if (bit16ASR == 0)
                    {
                        RBUS = Convert.ToInt16((DBUS >> 1));
                    }
                    else
                    {
                        RBUS = Convert.ToInt16((DBUS >> 1) | 0x8000);
                    }

                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.LSR:
                    RBUS = Convert.ToInt16((DBUS >> 1));
                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.ROL:
                    int bit16ROL = (DBUS >> 15) & 0b1;
                    if (bit16ROL == 0)
                    {
                        RBUS = Convert.ToInt16((DBUS << 1));
                    }
                    else
                    {
                        RBUS = Convert.ToInt16((DBUS << 1) ^ 0b1);
                    }

                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.ROR:
                    int bit1ROR = DBUS & 0b1;
                    if (bit1ROR == 0)
                    {
                        RBUS = Convert.ToInt16((DBUS >> 1));
                    }
                    else
                    {
                        RBUS = Convert.ToInt16((DBUS >> 1) ^ 0x8000);
                    }

                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.RLC:
                    int bit16RLC = (DBUS >> 15) & 0b1;

                    if (C == 0)
                    {
                        RBUS = Convert.ToInt16((DBUS << 1));
                    }
                    else
                    {
                        RBUS = Convert.ToInt16((DBUS << 1) ^ 0b1);
                    }

                    C = bit16RLC;
                    await ChangeRBUSBorderColor();
                    break;

                case ALUOperation.RRC:
                    int bit1RRC = DBUS & 0b1;
                    if (C == 0)
                    {
                        RBUS = Convert.ToInt16((DBUS >> 1));
                    }
                    else
                    {
                        RBUS = Convert.ToInt16((DBUS >> 1) ^ 0x8000);
                    }

                    C = bit1RRC;
                    await ChangeRBUSBorderColor();
                    break;
            }
        }
        private async Task ComputeRbusDestinationAsync(RbusDestination rbusDestination)
        {
            switch (rbusDestination)
            {
                case RbusDestination.None:
                    break;
                case RbusDestination.PmFLAG:
                    FLAG = RBUS;
                    await ChangeTwoBordersBrush(PMFLAG1Border, PMFLAG2Border);
                    FLAG_value.Content = Convert.ToString(FLAG, 2).PadLeft(16, '0');
                    break;

                case RbusDestination.PMFLAG30:
                    FLAG = Convert.ToInt16((FLAG & 0b10000) | (RBUS & 0b1111));
                    await ChangeTwoBordersBrush(PMFLAG1Border, PMFLAG2Border);
                    FLAG_value.Content = Convert.ToString(FLAG, 2).PadLeft(16, '0');
                    break;

                case RbusDestination.PmRG:
                    int indexRG = IR & 0b1111;
                    RG[indexRG] = RBUS;
                    await ChangeOneBorderColor(RGRBUS);
                    RGContentLabel();
                    break;

                case RbusDestination.PmSP:
                    SP = RBUS;
                    await ChangeOneBorderColor(SPRBUS);
                    SP_value.Content = Convert.ToString(SP, 2).PadLeft(16, '0');
                    break;

                case RbusDestination.PmT:
                    T = RBUS;
                    await ChangeOneBorderColor(TRBUS);
                    T_value.Content = Convert.ToString(T, 2).PadLeft(16, '0');
                    break;

                case RbusDestination.PmPC:
                    PC = RBUS;
                    await ChangeOneBorderColor(PCRBUS);
                    PC_value.Content = Convert.ToString(PC, 2).PadLeft(16, '0');
                    break;

                case RbusDestination.PmIVR:
                    IVR = RBUS;
                    await ChangeOneBorderColor(IVRRBUS);
                    IVR_value.Content = Convert.ToString(IVR, 2).PadLeft(16, '0');
                    break;

                case RbusDestination.PmADR:
                    ADR = (ushort)RBUS;
                    await ChangeOneBorderColor(ADRRBUS);
                    ADR_value.Content = Convert.ToString(ADR, 2).PadLeft(16, '0');
                    break;

                case RbusDestination.PmMDR:
                    MDR = RBUS;
                    await ChangeTwoBordersBrush(PMMDR1Border, PMMDR2Border);
                    MDR_value.Content = Convert.ToString(MDR, 2).PadLeft(16, '0');
                    break;
            }
        }
        private async Task ComputeOtherOperations(OtherOperations otherOperations)
        {
            switch (otherOperations)
            {
                case OtherOperations.None:
                    break;
                case OtherOperations.plus2SP:
                    SP += 1;
                    SP_value.Content = Convert.ToString(SP, 2).PadLeft(16, '0');
                    await DelayOneSecond();
                    break;

                case OtherOperations.minus2SP:
                    SP -= 1;
                    SP_value.Content = Convert.ToString(SP, 2).PadLeft(16, '0');
                    await DelayOneSecond();
                    break;

                case OtherOperations.plus2PC:
                    PC += 1;
                    PC_value.Content = Convert.ToString(PC, 2).PadLeft(16, '0');
                    await DelayOneSecond();
                    break;

                case OtherOperations.A1BE0:
                    ACLOW = 1;
                    break;

                case OtherOperations.A1BE1:
                    CIL = 1;
                    break;

                case OtherOperations.PdCONDA:
                    int tempFlagA = ((C  << 3) | (Z  << 2) | (S  << 1) | V );
                    FLAG = (short)tempFlagA;
                    break;

                case OtherOperations.CinPdCONDA:
                    CIN = 1;
                    int tempFlagACin = ((C << 3) | (Z << 2) | (S << 1) | V);
                    FLAG = (short)tempFlagACin;
                    break;

                case OtherOperations.PdCONDL:
                    int tempFlagL = ((Z << 2) | (S << 1));
                    FLAG = (short)tempFlagL;
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

        private async Task ComputeMemoryOperation(MemoryOperation memoryOperation)
        {
            switch (memoryOperation)
            {
                case MemoryOperation.None:
                    break;
                case MemoryOperation.IFCH:
                    IR = MEM[ADR];
                    IR_value.Content = Convert.ToString(IR, 2).PadLeft(16, '0');
                    await DelayOneSecond();
                    break;
                case MemoryOperation.RD:
                    MDR = MEM[ADR];
                    MDR_value.Content = Convert.ToString(MDR, 2).PadLeft(16, '0');
                    await DelayOneSecond();
                    break;
                case MemoryOperation.WR:
                    MEM[ADR] = MDR;
                    break;
            }
        }

        #endregion

        public async Task DelayOneSecond()
        {
            await Task.Delay(1000);
        }

        public async Task ChangeOneBorderColor(Border FisrtBorder)
        {
            FisrtBorder.BorderBrush = Brushes.Red;
            await DelayOneSecond();
            FisrtBorder.BorderBrush = Brushes.Black;
        }

        public async Task ChangeTwoBordersBrush(Border FisrtBorder, Border SecondBorder)
        {
            FisrtBorder.BorderBrush = Brushes.Red;
            SecondBorder.BorderBrush = Brushes.Red;
            await DelayOneSecond();
            FisrtBorder.BorderBrush = Brushes.Black;
            SecondBorder.BorderBrush = Brushes.Black;
        }

        public async Task ChangeRBUSBorderColor()
        {
            RBUS1Border.BorderBrush = Brushes.Red;
            RBUS2Border.BorderBrush = Brushes.Red;
            RBUS3Border.BorderBrush = Brushes.Red;
            await DelayOneSecond();
            RBUS1Border.BorderBrush = Brushes.Black;
            RBUS2Border.BorderBrush = Brushes.Black;
            RBUS3Border.BorderBrush = Brushes.Black;
        }



        public void RGContentLabel()
        {
            R0_value.Content = Convert.ToString(RG[0], 2).PadLeft(16, '0');
            R1_value.Content = Convert.ToString(RG[1], 2).PadLeft(16, '0');
            R2_value.Content = Convert.ToString(RG[2], 2).PadLeft(16, '0');
            R3_value.Content = Convert.ToString(RG[3], 2).PadLeft(16, '0');
            R4_value.Content = Convert.ToString(RG[4], 2).PadLeft(16, '0');
            R5_value.Content = Convert.ToString(RG[5], 2).PadLeft(16, '0');
            R6_value.Content = Convert.ToString(RG[6], 2).PadLeft(16, '0');
            R7_value.Content = Convert.ToString(RG[7], 2).PadLeft(16, '0');
            R8_value.Content = Convert.ToString(RG[8], 2).PadLeft(16, '0');
            R9_value.Content = Convert.ToString(RG[9], 2).PadLeft(16, '0');
            R10_value.Content = Convert.ToString(RG[10], 2).PadLeft(16, '0');
            R11_value.Content = Convert.ToString(RG[11], 2).PadLeft(16, '0');
            R12_value.Content = Convert.ToString(RG[12], 2).PadLeft(16, '0');
            R13_value.Content = Convert.ToString(RG[13], 2).PadLeft(16, '0');
            R14_value.Content = Convert.ToString(RG[14], 2).PadLeft(16, '0');
            R15_value.Content = Convert.ToString(RG[15], 2).PadLeft(16, '0');
        }

        private void uploadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog.InitialDirectory = "C:\\";
            openFileDialog.Title = "Select a Text File";
            
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                inputFilePath = filePath;

                try
                {
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        string fileContent = sr.ReadToEnd();
                        inputContent.Document.Blocks.Clear();
                        inputContent.AppendText(fileContent);
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Error reading the file: " + ex.Message);
                }
            }
        }

        public void Clear()
        {
            inputContent.SelectAll();

            inputContent.Selection.Text = "";
        }

        public void setFlags(ushort number1, ushort number2)
        {
            ushort result = (ushort)(number1 + number2);

            C = (result < number1 || result < number2) ? 1 : 0;

            short signedResult = (short)(result);

            V = ((signedResult < 0 && number1 > 0 && number2 > 0 )
                ||(signedResult > 0 && number1 < 0 && number2 < 0)) ? 1 : 0;

            Z = (result == 0) ? 1 : 0;

            S = ((result & 0x8000) != 0) ? 1 : 0;
        }
    }
}
