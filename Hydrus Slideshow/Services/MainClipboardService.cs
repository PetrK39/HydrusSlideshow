using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

namespace Hydrus_Slideshow.Services
{
    internal class MainClipboardService : IClipboardService
    {
        private readonly HashSet<string> lines;
        public MainClipboardService()
        {
            lines = new HashSet<string>();
        }
        public int AddLine(string line)
        {
            lines.Add(line);
            try
            {
                var data = new DataObject(string.Join(Environment.NewLine, lines));
                Clipboard.SetDataObject(data, true);
                return lines.Count;
            }
            catch { return 0; }
        }
    }
}
