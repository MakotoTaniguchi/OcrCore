using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OcrService.Models;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace OcrService.Controllers
{
    public class OcrController : Controller
    {
        [ActionName("Index")]
        public IActionResult Index()
        {
            return new RedirectResult("/Ocr/Input");
        }

        [ActionName("Input")]
        public IActionResult Input()
        {
            return View();
        }

        [ActionName("Analyze")]
        public IActionResult Analyze(List<IFormFile> files)
        {
            var result = files.SelectMany(file =>
            {
                var stream = file.OpenReadStream();

                var lines = OcrAnalyze(stream);

                lines.Wait();

                return lines.Result;
            }).ToList();

            var viewModel = new AnalyzeViewModel
            {
                AnalyzeTexts = result
            };

            return View(viewModel);
        }

        private async Task<List<string>> OcrAnalyze(Stream stream)
        {
            var bitmapDecoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());

            SoftwareBitmap softwareBitmap = await bitmapDecoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8,
                                                BitmapAlphaMode.Premultiplied);

            IReadOnlyList<Language> langList = OcrEngine.AvailableRecognizerLanguages;
            var ocrEngine = OcrEngine.TryCreateFromLanguage(langList[0]);

            var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);

            var lines = ocrResult.Lines.Select(ocrLine =>
            {
                return ocrLine.Text;
            }).ToList();
            
            return lines;
        }
    }
}
