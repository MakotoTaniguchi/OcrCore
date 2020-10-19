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
            var pageRects = files.SelectMany(file =>
            {
                var stream = file.OpenReadStream();
                if (file.FileName.ToLower().Contains(".pdf"))
                {
                    return ConvertPdfToImage(stream);
                }

                List<PageRect> pageRects = new List<PageRect>
                {
                    OcrAnalyze(stream)
                };
                return pageRects;
            }).ToList();

            var viewModel = new AnalyzeViewModel
            {
                PageRects = pageRects
            };

            return View(viewModel);
        }

        private List<PageRect> ConvertPdfToImage(Stream stream)
        {
            var loadFromStreamAsyncTask = PdfDocument.LoadFromStreamAsync(stream.AsRandomAccessStream()).AsTask();
            loadFromStreamAsyncTask.Wait();
            var pdfDocument = loadFromStreamAsyncTask.Result;

            List<string> result = new List<string>();

            return Enumerable.Range(0, (int)pdfDocument.PageCount).Select((index) =>
            {
                using (PdfPage page = pdfDocument.GetPage((uint)index))
                using (var convertStream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                {
                    var renderToStreamAsyncTask = page.RenderToStreamAsync(convertStream).AsTask();
                    renderToStreamAsyncTask.Wait();

                    return OcrAnalyze(convertStream.AsStream());
                }
            }).ToList();
        }

        private PageRect OcrAnalyze(Stream stream)
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

            var lineRects =  ocrResult.Lines.SelectMany(ocrLine =>
            {
                return ocrLine.Words.Select(word =>
                {
                    return new LineText
                    {
                        Height = (int)word.BoundingRect.Height,
                        Width = (int)word.BoundingRect.Width,
                        Top = (int)word.BoundingRect.Top,
                        Left = (int)word.BoundingRect.Left,
                        X = (int)word.BoundingRect.X,
                        Y = (int)word.BoundingRect.Y,
                        Text = word.Text,
                        FontSize = word.BoundingRect.Height >= word.BoundingRect.Width ? (int)word.BoundingRect.Height : (int)word.BoundingRect.Width
                    };
                });
            }).ToList();

            return new PageRect
            {
                Height = softwareBitmap.PixelHeight,
                Width = softwareBitmap.PixelWidth,
                LineTexts = lineRects
            };
        }
    }
}
