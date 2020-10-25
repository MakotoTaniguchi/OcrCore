using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OCRLibrary.AzureLib.Dtos;
using OCRLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Windows.Graphics.Imaging;

namespace OCRLibrary
{
    public class AzureComputerVisionLib : BaseLib
    {
        // Add your Computer Vision subscription key and endpoint to your environment variables.
        private static string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY");

        // An endpoint should have a format like "https://westus.api.cognitive.microsoft.com"
        private static string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");

        // the Batch Read method endpoint
        private static string uriBase = endpoint + "/vision/v3.1-preview.2/read/analyze?language=ja";

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

            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey);

            string url = uriBase;

            HttpResponseMessage response;

            byte[] byteData = GetStreamAsByteArray(stream);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                var postAsyncTask = client.PostAsync(url, content);
                postAsyncTask.Wait();

                response = postAsyncTask.Result;
            }

            string operationLocation;
            if (response.IsSuccessStatusCode)
                operationLocation =
                    response.Headers.GetValues("Operation-Location").FirstOrDefault();
            else
            {
                // Display the JSON error data.
                var errorReadAsStringAsyncTask = response.Content.ReadAsStringAsync();
                errorReadAsStringAsyncTask.Wait();

                return new PageRect { LineTexts = new List<LineText>() };
            }

            var status = string.Empty;
            ApiResponse apiResponse = null;
            int retryCount = 0;
            do {
                if (retryCount++ > 0)
                {
                    System.Threading.Thread.Sleep(1000);
                }

                var getAsyncTask = client.GetAsync(operationLocation);
                getAsyncTask.Wait();
                response = getAsyncTask.Result;

                var readAsStringAsyncTask = response.Content.ReadAsStringAsync();
                readAsStringAsyncTask.Wait();

                var contentString = readAsStringAsyncTask.Result;

                apiResponse = JsonConvert.DeserializeObject<ApiResponse>(contentString);
                status = apiResponse.Status;
            }
            while (status != "succeeded");
            
            var pageRect = new PageRect
            {
                Height = softwareBitmap.PixelHeight,
                Width = softwareBitmap.PixelWidth,
                LineTexts = apiResponse.AnalyzeResult.ReadResults.SelectMany(readResult =>
                {
                    return readResult.Lines.Select(line =>
                    {
                        return new LineText
                        {
                            Text = line.Text,
                            Y = line.BoundingBox[1],
                            X = line.BoundingBox[0]
                        };
                    });
                }).ToList()
            };
            return pageRect;
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        private byte[] GetStreamAsByteArray(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }
    }
}
