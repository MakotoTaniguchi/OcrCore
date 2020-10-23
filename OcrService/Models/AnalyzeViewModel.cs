using OCRLibrary.Dtos;
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
}
