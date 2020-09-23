using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Errors;
using autoplaysharp.Core;
using autoplaysharp.Core.OCR;
using autoplaysharp.Game.UI;
using autoplaysharp.Helper;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;

namespace autoplaysharp.Game
{
    public class GameImpl : IGame
    {
        private readonly IEmulatorWindow _window;
        private readonly IUiRepository _repository;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly Random _random = new Random();

        public IEmulatorOverlay Overlay { get; set; }

        public GameImpl(IEmulatorWindow window, IUiRepository repository, ILoggerFactory loggerFactory)
        {
            _window = window;
            _repository = repository;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger(GetType());
        }

        public void Click(string id)
        {
            Click(_repository[id]);
        }

        private float RandomizeWithinRange(float min, float max)
        {
            // using 20%-80% range to avoid clicking on borders.
            var scale = _random.Next(20, 80) * 0.01f;
            return min + ((max - min) * scale);
        }

        public void Click(UIElement element)
        {
            var x = element.X.Value;
            var y = element.Y.Value;

            if (element.W.HasValue)
            {
                x = RandomizeWithinRange(x, x + element.W.Value);
            }

            if(element.H.HasValue)
            {
                y = RandomizeWithinRange(y, y + element.H.Value);
            }

            _window.ClickAt(x, y);
        }

        public void Drag(string idStart, string IdEnd)
        {
            var start = _repository[idStart];
            var end = _repository[IdEnd];
            _window.Drag(new Vector2(start.X.Value, start.Y.Value), new Vector2(end.X.Value, end.Y.Value));
        }

        public string GetText(UIElement element)
        {
            using Bitmap section = GrabElement(element);

            // Preprocessing for OCR.
            using var section_mat = section.ToMat();

            using var scaled_section_mat = new Mat();
            if (element.Scale.HasValue)
            {
                var scale = element.Scale.Value;
                Cv2.Resize(section_mat, scaled_section_mat, new OpenCvSharp.Size(section.Width * scale, section.Height * scale));
            }

            using var grayscale_mat = new Mat();
            Cv2.CvtColor(element.Scale.HasValue ? scaled_section_mat : section_mat, grayscale_mat, ColorConversionCodes.BGR2GRAY);
            using var tresholded_mat = new Mat();

            if(element.Threshold.HasValue)
            {
                var threshold = element.Threshold.Value;
                Cv2.Threshold(grayscale_mat, tresholded_mat, threshold, 255, ThresholdTypes.Binary);
            }
            else
            {
                Cv2.Threshold(grayscale_mat, tresholded_mat, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            }

            using var inverted_mat = (~tresholded_mat).ToMat();
            using var inverted_bitmap = inverted_mat.ToBitmap();
            using var pix = inverted_bitmap.ToPix();

            // for debugging ...
            if (Settings.SaveImages)
            {
                SaveImage(element.Id, pix);
            }


            var result = TextRecognition.GetText(pix, element.PSM.HasValue ? element.PSM.Value : 3);
            //_logger.LogDebug($"Detected text {result.Text} with confidence: {result.Confidence}");

            Overlay?.ShowGetText(element);
            //_logger.LogDebug($"{element.Id} Text: {result}");
            
            return result.Text.TrimStart().TrimEnd();
        }

        private Bitmap GrabElement(UIElement element)
        {
            var x = element.X.Value * _window.Width;
            var y = element.Y.Value * _window.Height;
            var w = element.W.Value * _window.Width;
            var h = element.H.Value * _window.Height;
            var section = _window.GrabScreen((int)x, (int)y, (int)w, (int)h);
            if (Settings.SaveRawImages)
            {
                var dir = Path.Combine("logs", "raw");
                Directory.CreateDirectory(dir);
                section.Save(Path.Combine(dir, $"{element.Id}.bmp"), System.Drawing.Imaging.ImageFormat.Bmp);
            }
            return section;
        }

        public string GetText(string id)
        {
            var element = _repository[id];
            return GetText(element);
        }

        private static void SaveImage(string id, Tesseract.Pix pix)
        {
            try
            {
                if(!Directory.Exists("logs"))
                {
                    Directory.CreateDirectory("logs");
                }
                pix.Save($"logs\\{id}.bmp");
            }
            catch
            {
                // dont care...
            }
        }

        public bool IsVisible(UIElement element)
        {
            if(element.Image != null)
            {
                return IsImageVisible(element);
            }

            Debug.Assert(!string.IsNullOrWhiteSpace(element.Text));
            var text = GetText(element).TrimStart().TrimEnd();
            var isVisible = text == element.Text;
            Overlay?.ShowIsVisibile(element, isVisible);
            //_logger.LogDebug($"{element.Id} IsVisible: {isVisible}");
            return isVisible;
        }

        private bool IsImageVisible(UIElement element, float confidence = 0.90f)
        {
            using var uielement = GrabElement(element);

            if(Settings.SaveImages)
            {
                Directory.CreateDirectory("logs");
                uielement.Save($"logs\\{element.Id}.bmp");
            }

            using var uielement_mat = uielement.ToMat();
            using var template_mat = Cv2.ImDecode(element.Image, ImreadModes.AnyColor);
            using var uielement_mat_gray = new Mat();
            using var tempalte_mat_gray = new Mat();
            Cv2.CvtColor(uielement_mat, uielement_mat_gray, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(template_mat, tempalte_mat_gray, ColorConversionCodes.BGR2GRAY);
            using var uielement_mat_gray_scaled = new Mat();
            Cv2.Resize(uielement_mat_gray, uielement_mat_gray_scaled, template_mat.Size());

            using var result = new Mat();
            Cv2.MatchTemplate(uielement_mat_gray_scaled, tempalte_mat_gray, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out var _, out var maxval, out var _, out var maxloc);

            Overlay?.ShowIsVisibile(element, maxval > confidence, maxval);

            if (maxval >= confidence)
                return true;
            return false;
        }

        public bool IsVisible(string id)
        {
            return IsVisible(_repository[id]);
        }

        public ILogger CreateLogger(Type t)
        {
            return _loggerFactory.CreateLogger(t);
        }

        
        public void OnError(TaskError taskError)
        {
            switch (taskError)
            {
                case ElementNotFoundError elementNotFoundError when taskError is ElementNotFoundError:
                    {
                        using var screen = _window.GrabScreen(0, 0, _window.Width, _window.Height);
                        using var screenMat = screen.ToMat();

                        var missingElement = elementNotFoundError.MissingElement;
                        var x = (int)(missingElement.X * _window.Width);
                        var y = (int)(missingElement.Y * _window.Height);
                        var w = (int)(missingElement.W * _window.Width);
                        var h = (int)(missingElement.H * _window.Height);


                        var timeStamp = $"{DateTime.Now:yyyyMMddTHHmmss}";
                        var screenFileName = Path.Combine("logs", $"ElementNotFound {timeStamp} - Screen.bmp");

                        if (missingElement.XOffset.HasValue || missingElement.YOffset.HasValue)
                        {
                            DrawElementGrid(screenMat, missingElement);
                        }
                        else
                        {
                            DrawElement(screenMat, missingElement);
                        }


                        using var cropped = screen.Crop(x, y, w, h);


                        if (missingElement.Image == null)
                        {
                            using var pix = cropped.ToPix();
                            var foundText = TextRecognition.GetText(pix, missingElement.PSM.HasValue ? missingElement.PSM.Value : 3);
                            var text = $"Found Text: {foundText}";
                            var size = GetTextSize(text);
                            DrawText(screenMat, x, y + size.Height, text);
                        }
                        else
                        {
                            var missingElementFileName = Path.Combine("logs", $"ElementNotFound {timeStamp} - MissingElement.bmp");
                            cropped.Save(missingElementFileName);
                        }
#if DEBUG
                        Cv2.ImShow("Error", screenMat);
                        Cv2.WaitKey();
#endif
                        screenMat.SaveImage(screenFileName);
                    }
                    break;
            }


            void DrawElementGrid(Mat screenMat, UIElement element)
            {
                var x_count = element.XOffset.HasValue ? Math.Ceiling((1f - element.X.Value) / element.XOffset.Value) : 1;
                var y_count = element.YOffset.HasValue ? Math.Ceiling((1f - element.Y.Value) / element.YOffset.Value) : 1;
                int y = 0, x = 0;
                for (y = 0; y < y_count; y++)
                {
                    for (x = 0; x < x_count; x++)
                    {
                        var dynUiElement = _repository[element.Id, x, y];
                        DrawElement(screenMat, dynUiElement);
                    }
                }
            }

            OpenCvSharp.Size GetTextSize(string text)
            {
                var fontScale = 0.75;
                var font = HersheyFonts.HersheyComplex;
                var size = Cv2.GetTextSize(text, font, fontScale, 1, out var _);
                return size;
            }

            void DrawElement(Mat screenMat, UIElement uiElement)
            {
                var x = (int)(uiElement.X * _window.Width);
                var y = (int)(uiElement.Y * _window.Height);
                var w = (int)(uiElement.W * _window.Width);
                var h = (int)(uiElement.H * _window.Height);

                Cv2.Rectangle(screenMat, new Rect(x, y, w, h), new Scalar(0, 0, 255));
            }


            void DrawText(Mat screenMat, int x, int y, string text)
            {
                var fontScale = 0.75;
                var font = HersheyFonts.HersheyComplex;
                var size = Cv2.GetTextSize(text, font, fontScale, 1, out var _);
                var textX = x;
                if (textX + size.Width > _window.Width)
                {
                    // Move text so it fits onto the screenshot.
                    textX -= (textX + size.Width) - _window.Width;
                }
                Cv2.PutText(screenMat, text, new OpenCvSharp.Point(textX, y), font, fontScale, new Scalar(0, 0, 255));
            }
        }
    }
}