using System;
using System.Collections.Generic;

namespace ScreenScraper
{
    internal class Scraper
    {
        private string url;
        Dictionary<string, Dictionary<string, string>> _history = new Dictionary<string, Dictionary<string, string>>();
        internal Scraper(string url, string previousHistoryFile = null)
        {
            this.url = url;

            // If _history != null
            bool firstLine = true;
            string[] keys = null;
            foreach(var line in System.IO.File.ReadAllLines(previousHistoryFile))
            {
                string trimmedLine = line.Substring(1, line.Length - 2);
                var values = trimmedLine.Split(new string[] { "\",\"" }, StringSplitOptions.None);
                if (firstLine)
                {
                    keys = values;
                    firstLine = false;
                    continue;
                }

                Dictionary<string, string> record = new Dictionary<string, string>(); 
                for (int i = 0; i < values.Length && i < keys.Length; i ++)
                {
                    record[keys[i]] = values[i];
                }

                if(!_history.ContainsKey(record["url"]))
                    _history.Add(record["url"], record);
            }
        }

        internal IEnumerable<Dictionary<string, string>> GetResults()
        {
            // Enumerate the page links

            // first use the demo link
            //string test = "https://www.gumtree.com/p/office-space-offered/flexible-office-space-rental-docklands-serviced-offices-e14-/1162356040";
            //yield return ScrapePageData(new Uri(test));

            foreach (var page in EnumeratePages(url))
            {
                if (this._history.ContainsKey(page.AbsolutePath))
                    continue;

                var results = ScrapePageData(page);

                this._history.Add(page.AbsolutePath, results);

                yield return results;
            }
        }

        protected virtual IEnumerable<Uri> EnumeratePages(string rootUrl)
        {
            throw new NotImplementedException();
        }

        protected virtual Dictionary<string, string> ScrapePageData(Uri page)
        {
            throw new NotImplementedException();
        }

    }
}