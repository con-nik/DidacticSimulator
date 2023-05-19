using DidacticSimulator.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
    public partial class Interface : Window, INotifyPropertyChanged
    {
        Border border;
        Simulator simulator;
        Assembler assembler;
        
        private int _PC, _T, _IR, _SP, _IVR, _ADR, _MDR, _FLAG,
                    _R0, _R1, _R2, _R3, _R4, _R5, _R6, _R7, _R8, _R9, _R10,
                    _R11, _R12, _R13, _R14, _R15;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            assembler.ChangeInMachineCode(@"..\..\Shared\Files\InputFile.txt");
            simulator.LoadMicroprogram(@"..\..\Shared\Files\Microprogram.txt");
            simulator.LoadProgram(@"..\..\Shared\Files\Program.txt");
            simulator.Simulate();
        }

        public int PC
        {
            get { return _PC; }
            set
            {
                if (value != _PC)
                {
                    _PC = value;
                    OnPropertyChanged("PC");
                }
            }
        }
        public int T
        {
            get { return _T; }
            set
            {
                if (value != _T)
                {
                    _T = value;
                    OnPropertyChanged("T");
                }
            }
        }
        public int IR
        {
            get { return _IR; }
            set
            {
                if (value != _IR)
                {
                    _IR = value;
                    OnPropertyChanged("IR");
                }
            }
        }
        public int SP
        {
            get { return _SP; }
            set
            {
                if (value != _SP)
                {
                    _SP = value;
                    OnPropertyChanged("SP");
                }
            }
        }
        public int IVR
        {
            get { return _IVR; }
            set
            {
                if (value != _IVR)
                {
                    _IVR = value;
                    OnPropertyChanged("IVR");
                }
            }
        }
        public int ADR
        {
            get { return _ADR; }
            set
            {
                if (value != _ADR)
                {
                    _ADR = value;
                    OnPropertyChanged("ADR");
                }
            }
        }
        public int MDR
        {
            get { return _MDR; }
            set
            {
                if (value != _MDR)
                {
                    _MDR = value;
                    OnPropertyChanged("MDR");
                }
            }
        }
        public int FLAG
        {
            get { return _FLAG; }
            set
            {
                if (value != _FLAG)
                {
                    _FLAG = value;
                    OnPropertyChanged("FLAG");
                }
            }
        }
        public int R0
        {
            get { return _R0; }
            set
            {
                if (value != _R0)
                {
                    _R0 = value;
                    OnPropertyChanged("R0");
                }
            }
        }
        public int R1
        {
            get { return _R1; }
            set
            {
                if (value != _R1)
                {
                    _R1 = value;
                    OnPropertyChanged("R1");
                }
            }
        }
        public int R2
        {
            get { return _R2; }
            set
            {
                if (value != _R2)
                {
                    _R2 = value;
                    OnPropertyChanged("R2");
                }
            }
        }
        public int R3
        {
            get { return _R3; }
            set
            {
                if (value != _R3)
                {
                    _R3 = value;
                    OnPropertyChanged("R3");
                }
            }
        }
        public int R4
        {
            get { return _R4; }
            set
            {
                if (value != _R4)
                {
                    _R4 = value;
                    OnPropertyChanged("R4");
                }
            }
        }
        public int R5
        {
            get { return _R5; }
            set
            {
                if (value != _R5)
                {
                    _R5 = value;
                    OnPropertyChanged("R5");
                }
            }
        }
        public int R6
        {
            get { return _R6; }
            set
            {
                if (value != _R6)
                {
                    _R6 = value;
                    OnPropertyChanged("R6");
                }
            }
        }
        public int R7
        {
            get { return _R7; }
            set
            {
                if (value != _R7)
                {
                    _R7 = value;
                    OnPropertyChanged("R7");
                }
            }
        }
        public int R8
        {
            get { return _R8; }
            set
            {
                if (value != _R8)
                {
                    _R8 = value;
                    OnPropertyChanged("R8");
                }
            }
        }
        public int R9
        {
            get { return _R9; }
            set
            {
                if (value != _R9)
                {
                    _R9 = value;
                    OnPropertyChanged("R9");
                }
            }
        }
        public int R10
        {
            get { return _R10; }
            set
            {
                if (value != _R10)
                {
                    _R10 = value;
                    OnPropertyChanged("R10");
                }
            }
        }
        public int R11
        {
            get { return _R11; }
            set
            {
                if (value != _R11)
                {
                    _R11 = value;
                    OnPropertyChanged("R11");
                }
            }
        }
        public int R12
        {
            get { return _R12; }
            set
            {
                if (value != _R12)
                {
                    _R12 = value;
                    OnPropertyChanged("R12");
                }
            }
        }
        public int R13
        {
            get { return _R13; }
            set
            {
                if (value != _R13)
                {
                    _R13 = value;
                    OnPropertyChanged("R13");
                }
            }
        }
        public int R14
        {
            get { return _R14; }
            set
            {
                if (value != _R14)
                {
                    _R14 = value;
                    OnPropertyChanged("R14");
                }
            }
        }
        public int R15
        {
            get { return _R15; }
            set
            {
                if (value != _R15)
                {
                    _R15 = value;
                    OnPropertyChanged("R15");
                }
            }
        }

        public Interface()
        {
            InitializeComponent();
            assembler = new Assembler();
            simulator = new Simulator();

            _PC = simulator.PC;
            _T = simulator.T;
            _IR = simulator.IR;
            _SP = simulator.SP;
            _IVR = simulator.IVR;
            _ADR = simulator.ADR;
            _MDR = simulator.MDR;
            _FLAG = simulator.FLAG;

            _R0 = simulator.RG[0];
            _R1 = simulator.RG[1];
            _R2 = simulator.RG[2];
            _R3 = simulator.RG[3];
            _R4 = simulator.RG[4];
            _R5 = simulator.RG[5];
            _R6 = simulator.RG[6];
            _R7 = simulator.RG[7];
            _R8 = simulator.RG[8];
            _R9 = simulator.RG[9];
            _R10 = simulator.RG[10];
            _R11 = simulator.RG[11];
            _R12 = simulator.RG[12];
            _R13 = simulator.RG[13];
            _R14 = simulator.RG[14];
            _R15 = simulator.RG[15];


            PC_value.SetBinding(ContentProperty, new Binding("PC"));
            T_value.SetBinding(ContentProperty, new Binding("T"));
            IR_value.SetBinding(ContentProperty, new Binding("IR"));
            SP_value.SetBinding(ContentProperty, new Binding("SP"));
            IVR_value.SetBinding(ContentProperty, new Binding("IVR"));
            ADR_value.SetBinding(ContentProperty, new Binding("ADR"));
            MDR_value.SetBinding(ContentProperty, new Binding("MDR"));
            FLAG_value.SetBinding(ContentProperty, new Binding("FLAG"));

            R0_value.SetBinding(ContentProperty, new Binding("R0"));
            R1_value.SetBinding(ContentProperty, new Binding("R1"));
            R2_value.SetBinding(ContentProperty, new Binding("R2"));
            R3_value.SetBinding(ContentProperty, new Binding("R3"));
            R4_value.SetBinding(ContentProperty, new Binding("R4"));
            R5_value.SetBinding(ContentProperty, new Binding("R5"));
            R6_value.SetBinding(ContentProperty, new Binding("R6"));
            R7_value.SetBinding(ContentProperty, new Binding("R7"));
            R8_value.SetBinding(ContentProperty, new Binding("R8"));
            R9_value.SetBinding(ContentProperty, new Binding("R9"));
            R10_value.SetBinding(ContentProperty, new Binding("R10"));
            R11_value.SetBinding(ContentProperty, new Binding("R11"));
            R12_value.SetBinding(ContentProperty, new Binding("R12"));
            R13_value.SetBinding(ContentProperty, new Binding("R13"));
            R14_value.SetBinding(ContentProperty, new Binding("R14"));
            R15_value.SetBinding(ContentProperty, new Binding("R15"));

            DataContext = this;

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            var handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        /*private void OnPropertyChanged([CallerMemberName] string propertyName = null)
         {
             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
         }*/


    }
}
