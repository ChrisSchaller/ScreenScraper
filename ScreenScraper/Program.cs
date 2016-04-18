using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenScraper
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var rand = new System.Random();

            // 1. Parse parameters
            string url = "https://www.gumtree.com/flats-houses";

            // 2. Select scraper engine
            Scraper engine = new GumTreeMobileNumberScraper(url, @"C:\Users\ChrisSchaller\Desktop\tmp4FB7.csv");

            // 3. Define output path
            string output = System.IO.Path.GetTempFileName();

            // 3. select the output formatter
            Writer writer = new CSVWriter(output);

            // 3. Get the results
            foreach (var result in engine.GetResults())
            {
                writer.Write(result);

                // Pause for a bit, before going to the next page
                Thread.Sleep(100);
                Application.DoEvents();
                Thread.Sleep(rand.Next(100, 1000));

            }

        }
    }
}
