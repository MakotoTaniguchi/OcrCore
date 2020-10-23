using Amazon;
using Amazon.Textract;
using OCRLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Graphics.Imaging;

namespace OCRLibrary
{
    public class AmazonTextractLib: BaseLib
    {
        private static string AWSAccessKeyId = Environment.GetEnvironmentVariable("AWSAccessKeyId");

        // An endpoint should have a format like "https://westus.api.cognitive.microsoft.com"
        private static string AWSSecretKey = Environment.GetEnvironmentVariable("AWSSecretKey");

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

            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            stream.Position = 0;
            memoryStream.Position = 0;

            var client = new AmazonTextractClient(AWSAccessKeyId, AWSSecretKey, RegionEndpoint.USEast1);
            var detectDocumentTextTask = client.DetectDocumentTextAsync(new Amazon.Textract.Model.DetectDocumentTextRequest
            {
                Document = new Amazon.Textract.Model.Document {
                    Bytes = memoryStream
                }
            });
            detectDocumentTextTask.Wait();

            var pageRect = new PageRect {
                Height = softwareBitmap.PixelHeight,
                Width = softwareBitmap.PixelWidth,
                LineTexts = detectDocumentTextTask.Result.Blocks.Where(block => block.BlockType == BlockType.LINE).Select(block =>
                {
                    return new LineText
                    {
                        Text = block.Text,
                        X = (int)(softwareBitmap.PixelWidth * block.Geometry.BoundingBox.Left),
                        Y = (int)(softwareBitmap.PixelHeight * block.Geometry.BoundingBox.Top)
                    };
                }).ToList()
            };
            return pageRect;
        }
    }
}
