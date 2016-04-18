using System.Collections.Generic;
using System.Linq;

namespace ScreenScraper
{
    internal class CSVWriter : Writer
    {
        private string output;

        public CSVWriter(string output)
        {
            this.output = output;
        }

        private object __locker = new object();
        internal override void Write(Dictionary<string, string> result)
        {
            lock(__locker)
            {
                if(System.IO.File.ReadAllText(output).Length == 0)
                    System.IO.File.AppendAllLines(output, new string[] { string.Join(",", result.Select(r => string.Format("\"{0}\"", r.Key))) });
                System.IO.File.AppendAllLines(output, new string[] { string.Join(",", result.Select(r => string.Format("\"{0}\"", r.Value))) });
            }
        }
    }
}