using OCRLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Data.Pdf;

namespace OCRLibrary
{
    public abstract class BaseLib
    {
        public abstract PageRect OcrAnalyze(Stream stream);

        public List<PageRect> ConvertPdfToImage(Stream stream)
        {
            return ConvertPdfToStream(stream).Select(convertStream =>
            {
                return OcrAnalyze(convertStream);
            }).ToList();
        }

        protected List<Stream> ConvertPdfToStream(Stream stream)
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

                    return convertStream.AsStream();
                }
            }).ToList();
        }
    }
}
