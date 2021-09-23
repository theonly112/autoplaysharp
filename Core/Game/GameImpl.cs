using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Errors;
using autoplaysharp.Core.Game.Tasks;
using autoplaysharp.Core.Helper;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace autoplaysharp.Core.Game
{
    public class GameImpl : IGame
    {
        private readonly IEmulatorWindow _window;
        private readonly IVideoProvider _videoProvider;
        private readonly IUiRepository _repository;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ITextRecognition _recognition;
        private readonly ISettings _settings;
        private readonly ILogger _logger;
        private readonly Random _random = new Random();

        public IEmulatorOverlay Overlay { get; set; }

        public GameImpl(IEmulatorWindow window, IVideoProvider videoProvider, IUiRepository repository,
            ILoggerFactory loggerFactory, ITextRecognition recognition, ISettings settings)
        {
            _window = window;
            _videoProvider = videoProvider;
            _repository = repository;
            _loggerFactory = loggerFactory;
            _recognition = recognition;
            _settings = settings;
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

        public void Click(UiElement element)
        {
            var x = element.X.GetValueOrDefault();
            var y = element.Y.GetValueOrDefault();

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

        public string GetText(UiElement element)
        {
            using Bitmap section = GrabElement(element);
            return _recognition.GetText(section, element);
        }

        public void Drag(string idStart, string idEnd)
        {
            var start = _repository[idStart];
            var end = _repository[idEnd];
            _window.Drag(new Vector2(start.X.GetValueOrDefault(), start.Y.GetValueOrDefault()), new Vector2(end.X.GetValueOrDefault(), end.Y.GetValueOrDefault()));
        }

        private Bitmap GrabElement(UiElement element)
        {
            var x = element.X.GetValueOrDefault() * _window.Width;
            var y = element.Y.GetValueOrDefault() * _window.Height;
            var w = element.W.GetValueOrDefault() * _window.Width;
            var h = element.H.GetValueOrDefault() * _window.Height;
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

        //private static void SaveImage(string id, Tesseract.Pix pix)
        //{
        //    try
        //    {
        //        if(!Directory.Exists("logs"))
        //        {
        //            Directory.CreateDirectory("logs");
        //        }
        //        pix.Save($"logs\\{id}.bmp");
        //    }
        //    catch
        //    {
        //        // dont care...
        //    }
        //}

        public bool IsVisible(UiElement element)
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

        private bool IsImageVisible(UiElement element, float confidence = 0.80f)
        {
            using var uielement = GrabElement(element);

            if(Settings.SaveImages)
            {
                Directory.CreateDirectory("logs");
                uielement.Save($"logs\\{element.Id}.bmp");
            }

            using var uiElementMat = uielement.ToMat();
            using var templateMat = Cv2.ImDecode(element.Image, ImreadModes.AnyColor);
            using var uiElementMatGray = new Mat();
            using var templateMatGray = new Mat();


            var size = templateMat.Size();
            var targetSize = new OpenCvSharp.Size(size.Height * 2, size.Width * 2);

            Cv2.CvtColor(uiElementMat, uiElementMatGray, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(templateMat, templateMatGray, ColorConversionCodes.BGR2GRAY);
            using var uiElementMatGrayScaled = new Mat();
            Cv2.Resize(uiElementMatGray, uiElementMatGrayScaled, targetSize);
            using var scaledTemplateMat = new Mat();
            Cv2.Resize(templateMatGray, scaledTemplateMat, targetSize);

            using var result = new Mat();
            Cv2.MatchTemplate(uiElementMatGrayScaled, scaledTemplateMat, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out var _, out var maxVal, out var _, out _);

            Overlay?.ShowIsVisibile(element, maxVal > confidence, maxVal);

            return maxVal >= confidence;
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
            if (_settings.RestartOnError)
            {
                RestartGame();
            }
            switch (taskError)
            {
                case ElementNotFoundError elementNotFoundError:
                    {
                        using var screen = _videoProvider.GetCurrentFrame().Crop(0, 0, _window.Width, _window.Height);
                        using var screenMat = screen.ToMat();

                        var missingElement = elementNotFoundError.MissingElement;
                        var x = (int)(missingElement.X.GetValueOrDefault() * _window.Width);
                        var y = (int)(missingElement.Y.GetValueOrDefault() * _window.Height);
                        var w = (int)(missingElement.W.GetValueOrDefault() * _window.Width);
                        var h = (int)(missingElement.H.GetValueOrDefault() * _window.Height);


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
                            var foundText = _recognition.GetText(cropped, missingElement);
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
                        //Cv2.ImShow("Error", screenMat);
                        //Cv2.WaitKey();
#endif
                        screenMat.SaveImage(screenFileName);
                    }
                    break;

            }

            throw new Exception("Cancelling task by throwing exception?");


            void DrawElementGrid(Mat screenMat, UiElement element)
            {
                var xCount = element.XOffset.HasValue ? Math.Ceiling((1f - element.X.GetValueOrDefault()) / element.XOffset.Value) : 1;
                var yCount = element.YOffset.HasValue ? Math.Ceiling((1f - element.Y.GetValueOrDefault()) / element.YOffset.Value) : 1;
                int y;
                for (y = 0; y < yCount; y++)
                {
                    int x;
                    for (x = 0; x < xCount; x++)
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

            void DrawElement(Mat screenMat, UiElement uiElement)
            {
                var x = (int)(uiElement.X.GetValueOrDefault() * _window.Width);
                var y = (int)(uiElement.Y.GetValueOrDefault() * _window.Height);
                var w = (int)(uiElement.W.GetValueOrDefault() * _window.Width);
                var h = (int)(uiElement.H.GetValueOrDefault() * _window.Height);

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

        private void RestartGame()
        {
            _logger.LogError($"Restarting game:\n{Environment.StackTrace}");
            _window.RestartGame();
            var restart = new RestartGame(this, _repository, _settings);
            restart.Run(CancellationToken.None).Wait();
        }
    }
}