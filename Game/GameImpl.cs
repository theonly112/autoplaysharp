using autoplaysharp.Contracts;
using autoplaysharp.Game.UI;
using autoplaysharp.Helper;
using autoplaysharp.OCR;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace autoplaysharp.Game
{
    class GameImpl : IGame
    {
        private readonly NoxWindow _window;
        private readonly Repository _repository;

        public GameImpl(NoxWindow window, Repository repository)
        {
            _window = window;
            _repository = repository;
        }

        public void Click(string id)
        {
            Click(_repository[id]);
        }

        public void Click(UIElement element)
        {
            _window.ClickAt(element.X.Value, element.Y.Value);
        }

        public void Drag(string idStart, string IdEnd)
        {
            var start = _repository[idStart];
            var end = _repository[IdEnd];
            _window.Drag(new Vector2(start.X.Value, start.Y.Value), new Vector2(end.X.Value, end.Y.Value));
        }

        public void Update()
        {
        }

        public string GetText(UIElement element)
        {
            var x = element.X.Value * _window.Width;
            var y = element.Y.Value * _window.Height;
            var w = element.W.Value * _window.Width;
            var h = element.H.Value * _window.Height;

            using var section = _window.GrabScreen((int)x, (int)y, (int)w, (int)h);


            // Preprocessing for OCR.
            using var section_mat = section.ToMat();

            using var scaled_section_mat = new Mat();
            if (element.Scale.HasValue)
            {
                var scale = element.Scale.Value;
                Cv2.Resize(section_mat, scaled_section_mat, new Size(section.Width * scale, section.Height * scale));
            }

            using var grayscale_mat = new Mat();
            Cv2.CvtColor(element.Scale.HasValue ? scaled_section_mat : section_mat, grayscale_mat, ColorConversionCodes.BGR2GRAY);
            using var tresholded_mat = new Mat();

            var threshold = element.Threshold.HasValue ? element.Threshold.Value : 128;

            Cv2.Threshold(grayscale_mat, tresholded_mat, threshold, 255, ThresholdTypes.Binary);
            using var inverted_mat = (~tresholded_mat).ToMat();
            using var inverted_bitmap = inverted_mat.ToBitmap();
            using var pix = inverted_bitmap.ToPix();

            // for debugging ...
            if (Program.SaveImages)
            {
                SaveImage(element.Id, pix);
            }

        

            var result = TextRecognition.GetText(pix, element.PSM.HasValue ? element.PSM.Value : 3);
            return result.TrimStart().TrimEnd();
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
            Debug.Assert(!string.IsNullOrWhiteSpace(element.Text));
            var text = GetText(element).TrimStart().TrimEnd();
            return text == element.Text;
        }

        public bool IsVisible(string id)
        {
            return IsVisible(_repository[id]);
        }


    }
}