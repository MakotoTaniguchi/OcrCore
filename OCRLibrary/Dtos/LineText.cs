using System;
using System.Collections.Generic;
using System.Text;

namespace OCRLibrary.Dtos
{
    public class LineText
    {
        public int Height { get; set; }

        public int Width { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Top { get; set; }

        public int Left { get; set; }

        public string Text { get; set; }

        public int FontSize { get; set; }
    }
}
