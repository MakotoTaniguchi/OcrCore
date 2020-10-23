using Google.Cloud.Vision.V1;
using OCRLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Graphics.Imaging;

namespace OCRLibrary
{
    public class GoogleCloudVisionLib : BaseLib
    {
        public override PageRect OcrAnalyze(Stream stream)
        {
            var imageStream = new MemoryStream();
            stream.CopyTo(imageStream);
            imageStream.Position = 0;
            stream.Position = 0;

            var createAsyncTask = BitmapDecoder.CreateAsync(imageStream.AsRandomAccessStream()).AsTask();
            createAsyncTask.Wait();
            var bitmapDecoder = createAsyncTask.Result;

            var getSoftwareBitmapAsyncTask = bitmapDecoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8,
                                                BitmapAlphaMode.Premultiplied).AsTask();
            getSoftwareBitmapAsyncTask.Wait();
            SoftwareBitmap softwareBitmap = getSoftwareBitmapAsyncTask.Result;

            // Instantiates a client
            var client = ImageAnnotatorClient.Create();
            // Load the image file into memory
            var image = Image.FromStream(stream);
            // Performs label detection on the image file
            var response = client.DetectText(image);

            var pageRect = new PageRect
            {
                Height = softwareBitmap.PixelHeight,
                Width = softwareBitmap.PixelWidth,
                LineTexts = response.Where(annotation =>
                {
                    return string.IsNullOrEmpty(annotation.Locale);
                }).Select(annotation =>
                {
                    return new LineText
                    {
                        Text = annotation.Description,
                        X = annotation.BoundingPoly.Vertices.First().X,
                        Y = annotation.BoundingPoly.Vertices.First().Y,
                    };
                }).ToList()
            };
            return pageRect;
        }
    }
}
