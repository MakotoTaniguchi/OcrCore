using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace OcrService.Models
{
    public class AnalyzeViewModel
    {
        public List<string> AnalyzeTexts { get; set; }

        public List<PageRect> PageRects { get; set; }
    }

    public class PageRect
    {
        public int Height { get; set; }

        public int Width { get; set; }

        public List<LineText> LineTexts { get; set; }
    }

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
