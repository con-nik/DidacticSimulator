using DidacticSimulator.Shared.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace DidacticSimulator.Utilities
{
    public class Parser
    {
        private string labelMatch = @"[A-Za-z]+:";
        private string onlyUpperCaseMatch = @"\b[A-Z]+\b(?![^ ]*[^A-Za-z\s])";

        private Dictionary<int, string> _instructions;
        private Dictionary<string, int> _labels;


        public Parser(string pathFile)
        {
            List<string> fileLines = new List<string>();

            _instructions = new Dictionary<int, string>();
            _labels = new Dictionary<string, int>();

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
                Match match = Regex.Match(fileLines[index], onlyUpperCaseMatch);
                if (match.Success)
                    _instructions.Add(index, match.Value);

                Match matchLabels = Regex.Match(fileLines[index], labelMatch);
                if (matchLabels.Success)
                    _labels.Add(matchLabels.Value, index);
            }

        }

        public Dictionary<int, string> Instructions => _instructions;
        public Dictionary<string, int> Labels => _labels;

        /* public void parseInputFile(string pathFile)
         {
             for (int index = 0; index < fileLines.Count; index++)
             {
                 Match match = Regex.Match(fileLines[index], onlyUpperCaseMatch);
                 if (match.Success)
                     instructions.Add(index, match.Value);

                 Match matchLabels = Regex.Match(fileLines[index], labelMatch);
                 if (matchLabels.Success)
                     labels.Add(matchLabels.Value, index);
             }


             using (StreamWriter writer = new StreamWriter(@"..\..\Shared\Files\OutputFile.txt"))
             {
                 foreach (var kvp in instructions)
                 {
                     writer.Write($"{kvp.Key} # {kvp.Value} \n");
                 }

                 foreach (var kvp in labels)
                 {
                     writer.Write($"{kvp.Key} # {kvp.Value} \n");
                 }
             }
         }*/
    }
}
