using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OcrService.Models;
using Windows.Data.Pdf;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

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
                List<string> lines;
                if (file.FileName.ToLower().Contains(".pdf"))
                {
                    lines = ConvertPdfToImage(stream);
                }
                else
                {
                    lines = OcrAnalyze(stream);
                }

                return lines;
            }).ToList();

            var viewModel = new AnalyzeViewModel
            {
                AnalyzeTexts = result
            };

            return View(viewModel);
        }

        private List<string> ConvertPdfToImage(Stream stream)
        {
            var loadFromStreamAsyncTask = PdfDocument.LoadFromStreamAsync(stream.AsRandomAccessStream()).AsTask();
            loadFromStreamAsyncTask.Wait();
            var pdfDocument = loadFromStreamAsyncTask.Result;

            List<string> result = new List<string>();
            for (var pageIndex = 0; pageIndex < pdfDocument.PageCount; pageIndex++)
            {
                using (PdfPage page = pdfDocument.GetPage((uint)pageIndex))
                {
                    using (var convertStream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                    {
                        var renderToStreamAsyncTask = page.RenderToStreamAsync(convertStream).AsTask();
                        renderToStreamAsyncTask.Wait();

                        var texts = OcrAnalyze(convertStream.AsStream());

                        result.AddRange(texts);
                    }
                }
            }

            return result;
        }

        private List<string> OcrAnalyze(Stream stream)
        {
            var createAsyncTask = BitmapDecoder.CreateAsync(stream.AsRandomAccessStream()).AsTask();
            createAsyncTask.Wait();
            var bitmapDecoder = createAsyncTask.Result;

            var getSoftwareBitmapAsyncTask = bitmapDecoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8,
                                                BitmapAlphaMode.Premultiplied).AsTask();
            getSoftwareBitmapAsyncTask.Wait();
            SoftwareBitmap softwareBitmap = getSoftwareBitmapAsyncTask.Result;

            IReadOnlyList<Language> langList = OcrEngine.AvailableRecognizerLanguages;
            var ocrEngine = OcrEngine.TryCreateFromLanguage(langList[0]);

            var recognizeAsyncTask = ocrEngine.RecognizeAsync(softwareBitmap).AsTask();
            recognizeAsyncTask.Wait();

            var ocrResult = recognizeAsyncTask.Result;

            var lines = ocrResult.Lines.Select(ocrLine =>
            {
                return ocrLine.Text;
            }).ToList();
            
            return lines;
        }
    }
}
