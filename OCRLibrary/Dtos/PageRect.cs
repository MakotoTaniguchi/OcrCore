using System;
using System.Collections.Generic;
using System.Text;

namespace OCRLibrary.Dtos
{
    public class PageRect
    {
        public int Height { get; set; }

        public int Width { get; set; }

        public List<LineText> LineTexts { get; set; }
    }
}
