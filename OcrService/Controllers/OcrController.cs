using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCRLibrary;
using OCRLibrary.Dtos;
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

        [ActionName("WindowsSDKInput")]
        public IActionResult Input()
        {
            var viewModel = new InputViewModel
            {
                RequestUrl = @"/Ocr/WindowsSDKAnalyze"
            };
            return View("Input", viewModel);
        }

        [ActionName("GcpInput")]
        public IActionResult GcpInput()
        {
            var viewModel = new InputViewModel
            {
                RequestUrl = @"/Ocr/GcpAnalyze"
            };
            return View("Input", viewModel);
        }

        [ActionName("WindowsSDKAnalyze")]
        public IActionResult WindowsSDKAnalyze(List<IFormFile> files)
        {
            var windowsSdkOcrLib = new WindowsSdkOcrLib();
            var pageRects = files.SelectMany(file =>
            {
                var stream = file.OpenReadStream();
                if (file.FileName.ToLower().Contains(".pdf"))
                {
                    return windowsSdkOcrLib.ConvertPdfToImage(stream);
                }

                List<PageRect> pageRects = new List<PageRect>
                {
                    windowsSdkOcrLib.OcrAnalyze(stream)
                };
                return pageRects;
            }).ToList();

            var viewModel = new AnalyzeViewModel
            {
                PageRects = pageRects
            };

            return View("Analyze", viewModel);
        }

        [ActionName("GcpAnalyze")]
        public IActionResult GcpAnalyze(List<IFormFile> files)
        {
            var googleCloudVisionLib = new GoogleCloudVisionLib();

            var pageRects = files.SelectMany(file =>
            {
                var stream = file.OpenReadStream();
                if (file.FileName.ToLower().Contains(".pdf"))
                {
                    return googleCloudVisionLib.ConvertPdfToImage(stream);
                }

                List<PageRect> pageRects = new List<PageRect>
                {
                    googleCloudVisionLib.OcrAnalyze(stream)
                };

                return pageRects;
            }).ToList();

            var viewModel = new AnalyzeViewModel
            {
                PageRects = pageRects
            };

            return View("Analyze", viewModel);
        }
    }
}
