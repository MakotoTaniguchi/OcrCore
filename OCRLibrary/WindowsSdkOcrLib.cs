using OCRLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Data.Pdf;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace OCRLibrary
{
    public class WindowsSdkOcrLib : BaseLib
    {
        public override PageRect OcrAnalyze(Stream stream)
        {
            var createAsyncTask = BitmapDecoder.CreateAsync(stream.AsRandomAccessStream()).AsTask();
            createAsyncTask.Wait();
            var bitmapDecoder = createAsyncTask.Result;

            var getSoftwareBitmapAsyncTask = bitmapDecoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8,
                                                BitmapAlphaMode.Premultiplied).AsTask();
            getSoftwareBitmapAsyncTask.Wait();
            SoftwareBitmap softwareBitmap = getSoftwareBitmapAsyncTask.Result;

            //IReadOnlyList<Language> langList = OcrEngine.AvailableRecognizerLanguages;
            Windows.Globalization.Language language = new Windows.Globalization.Language("ja");
            var ocrEngine = OcrEngine.TryCreateFromLanguage(language);

            var recognizeAsyncTask = ocrEngine.RecognizeAsync(softwareBitmap).AsTask();
            recognizeAsyncTask.Wait();

            var ocrResult = recognizeAsyncTask.Result;

            var lineRects = ocrResult.Lines.SelectMany(ocrLine =>
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
