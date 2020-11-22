using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using autoplaysharp.Contracts;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Tesseract;
using Point = System.Drawing.Point;

namespace autoplaysharp.UiAutomation.OCR
{
    public class TextRecognition : ITextRecognition
    {
        private static readonly object _lock = new object();
        private static readonly TesseractEngine _engine;


        public string GetText(Bitmap section, UIElement element)
        {
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

            if (element.Threshold.HasValue)
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
            using var pix = ToPix(inverted_bitmap);

            // TODO: debugging
            // for debugging ...
            //if (Settings.SaveImages)
            //{
            //    SaveImage(element.Id, pix);
            //}


            var result = GetText(pix, element.PSM.HasValue ? element.PSM.Value : 3);

            Debug.WriteLine($"{element.Id} - {result.Confidence}");

            // TryHard strategy?....
            if (string.IsNullOrEmpty(result.Text) && element.TryHard.GetValueOrDefault())
            {
                var results = new List<string>();
                for (var threshold = 50; threshold < 200; threshold += 3)
                {
                    Cv2.Threshold(grayscale_mat, tresholded_mat, threshold, 255, ThresholdTypes.Binary);
                    using var invertedMat2 = (~tresholded_mat).ToMat();
                    using var invertedBitmap2 = invertedMat2.ToBitmap();
                    using var pix2 = ToPix(invertedBitmap2);
                    var result2 = GetText(pix2, element.PSM.HasValue ? element.PSM.Value : 3);
                    if (result.Text != null) results.Add(result2.Text.Trim());
                }

                return results
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .GroupBy(x => x)
                    .OrderByDescending(x => x.Count())
                    .First()
                    .Key;
            }

            // TODO: overlay...
            //Overlay?.ShowGetText(element);

            return result.Text.Trim();
        }

        public Point LocateText(Bitmap image, string text)
        {
            lock (_lock)
            {

                //page.

                using var section_mat = image.ToMat();

                using var scaled_section_mat = new Mat();
                using var grayscale_mat = new Mat();
                Cv2.CvtColor(section_mat, grayscale_mat, ColorConversionCodes.BGR2GRAY);
                using var tresholded_mat = new Mat();

                Cv2.Threshold(grayscale_mat, tresholded_mat, 200, 255, ThresholdTypes.Binary);

                using var inverted_mat = (~tresholded_mat).ToMat();
                using var inverted_bitmap = inverted_mat.ToBitmap();
                using var pix = ToPix(inverted_bitmap);

                using var page = _engine.Process(pix);
                using (var iter = page.GetIterator())
                {
                    iter.Begin();
                    do
                    {
                        if (!iter.TryGetBoundingBox(PageIteratorLevel.Word, out var rect)) continue;
                        if (iter.GetText(PageIteratorLevel.Word) != "Future") continue;
                        
                        iter.Next(PageIteratorLevel.Word);
                        if (iter.GetText(PageIteratorLevel.Word) == "Fight")
                        {
                            return new Point(rect.X1 + rect.Width / 2, rect.Y1);
                        }
                    } while (iter.Next(PageIteratorLevel.Word));
                }

                return new Point(0, 0);
            }
        }

        private static Pix ToPix(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Tiff);
                return Pix.LoadTiffFromMemory(stream.ToArray());
            }
        }

        static TextRecognition()
        {
            _engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.LstmOnly);
            _engine.SetVariable("debug_file", "/dev/null");
        }

        public static TextRecognitionResult GetText(Pix pix, int psm = 3)
        {
            lock (_lock)
            {
                using var page = _engine.Process(pix, (PageSegMode)psm);
                return new TextRecognitionResult(page.GetMeanConfidence(), page.GetText());
            }
        }
    }
}
