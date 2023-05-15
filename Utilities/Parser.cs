using DidacticSimulator.Shared.Enums;
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
    public class Parser
    {
        private string labelMatch = @"[A-Za-z0-9]+:";
        private string onlyUpperCaseMatch = @"\b[A-Z]+\b(?![^ ]*[^A-Za-z\s])";
        private string operandsMatch = @"(R\d+)|(\d+\(R\d+\))|(\(R\d+\))|(\d+)";

        private string adMatch = @"(R\d+)";
        private string amMatch = @"(\d+)";
        private string aiMatch = @"(\(R\d+\))";
        private string axMatch = @"(\d+\(R\d+\))";

        private Dictionary<int, string> _instructions;
        private Dictionary<string, int> _labels;
        private Dictionary<int, List<string>> _operands;
        private Dictionary<int, string> _lines;


        public Parser()
        {
            _instructions = new Dictionary<int, string>();
            _labels = new Dictionary<string, int>();
            _operands = new Dictionary<int, List<string>>();
            _lines = new Dictionary<int, string>();
        }

        public void ParseFile(string pathFile)
        {
            List<string> fileLines = new List<string>();

            using (StreamReader sr = new StreamReader(pathFile))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    fileLines.Add(line);
                }
            }

            for (int index = 0; index < fileLines.Count; index++)
            {
                _lines.Add(index, fileLines[index]);

                Match match = Regex.Match(fileLines[index], onlyUpperCaseMatch);
                if (match.Success)
                    _instructions.Add(index, match.Value);

                Match matchLabels = Regex.Match(fileLines[index], labelMatch);
                if (matchLabels.Success)
                    _labels.Add(matchLabels.Value.Substring(0, matchLabels.Value.Length - 1), index);

                MatchCollection matchOperands = Regex.Matches(fileLines[index], operandsMatch);
                List<string> operands = new List<string>();
                foreach (Match matchOperand in matchOperands)
                {
                    operands.Add(getAddressingMode(matchOperand.Value));
                    operands.Add(matchOperand.Value);
                }
                _operands.Add(index, operands);
            }
        }

        private string getAddressingMode(string operand) {
            Match matchAx = Regex.Match(operand, axMatch);
            if (matchAx.Success)
                return "11";

            Match matchAi = Regex.Match(operand, aiMatch);
            if (matchAi.Success)
                return "10";

            Match matchAd = Regex.Match(operand, adMatch);
            if (matchAd.Success)
                return "01";

            Match matchAm = Regex.Match(operand, amMatch);
            if (matchAm.Success)
                return "00";

            return "";
        }

        public Dictionary<int, string> Instructions => _instructions;
        public Dictionary<string, int> Labels => _labels;
        public Dictionary<int, List<string>> Operands => _operands;
        public Dictionary<int, string> Lines => _lines;
    }
}
