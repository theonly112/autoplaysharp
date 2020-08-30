using autoplaysharp.Contracts;
using autoplaysharp.Game.UI;
using autoplaysharp.Helper;
using autoplaysharp.OCR;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using System.Drawing;
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
            var x = element.X.Value;
            var y = element.Y.Value;

            if (element.W.HasValue)
            {
                x += element.W.Value / 2f;
            }

            if(element.H.HasValue)
            {
                y += element.H.Value / 2f;
            }

            _window.ClickAt(x, y);
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

        private Bitmap GrabElement(UIElement element)
        {
            var x = element.X.Value * _window.Width;
            var y = element.Y.Value * _window.Height;
            var w = element.W.Value * _window.Width;
            var h = element.H.Value * _window.Height;
            var section = _window.GrabScreen((int)x, (int)y, (int)w, (int)h);
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
            return text == element.Text;
        }

        private bool IsImageVisible(UIElement element, float confidence = 0.90f)
        {
            using var uielement = GrabElement(element);

            if(Program.SaveImages)
            {
                Directory.CreateDirectory($"logs\\{Path.GetDirectoryName(element.Image)}");
                uielement.Save($"logs\\{element.Image}");
            }

            using var uielement_mat = uielement.ToMat();
            using var template_mat = Cv2.ImRead(element.Image);
            using var uielement_mat_gray = new Mat();
            using var tempalte_mat_gray = new Mat();
            Cv2.CvtColor(uielement_mat, uielement_mat_gray, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(template_mat, tempalte_mat_gray, ColorConversionCodes.BGR2GRAY);
            using var uielement_mat_gray_scaled = new Mat();
            Cv2.Resize(uielement_mat_gray, uielement_mat_gray_scaled, template_mat.Size());

            using var result = new Mat();
            Cv2.MatchTemplate(uielement_mat_gray_scaled, tempalte_mat_gray, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out var _, out var maxval, out var _, out var maxloc);

            if (maxval > confidence)
                return true;
            return false;
        }

        public bool IsVisible(string id)
        {
            return IsVisible(_repository[id]);
        }
    }
}