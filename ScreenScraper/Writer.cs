using System;
using System.Collections.Generic;

namespace ScreenScraper
{
    internal class Writer
    {
        internal virtual void Write(Dictionary<string, string> result)
        {
            throw new NotImplementedException();
        }
    }
}