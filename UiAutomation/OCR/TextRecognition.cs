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
        private static readonly object Lock = new object();
        private static readonly TesseractEngine Engine;


        public string GetText(Bitmap section, UiElement element)
        {
            // Preprocessing for OCR.
            using var sectionMat = section.ToMat();

            using var scaledSectionMat = new Mat();
            if (element.Scale.HasValue)
            {
                var scale = element.Scale.Value;
                Cv2.Resize(sectionMat, scaledSectionMat, new OpenCvSharp.Size(section.Width * scale, section.Height * scale));
            }

            using var grayscaleMat = new Mat();
            Cv2.CvtColor(element.Scale.HasValue ? scaledSectionMat : sectionMat, grayscaleMat, ColorConversionCodes.BGR2GRAY);
            using var tresholdedMat = new Mat();

            if (element.Threshold.HasValue)
            {
                var threshold = element.Threshold.Value;
                Cv2.Threshold(grayscaleMat, tresholdedMat, threshold, 255, ThresholdTypes.Binary);
            }
            else
            {
                Cv2.Threshold(grayscaleMat, tresholdedMat, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            }

            using var invertedMat = (~tresholdedMat).ToMat();
            using var invertedBitmap = invertedMat.ToBitmap();
            using var pix = ToPix(invertedBitmap);

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
                    Cv2.Threshold(grayscaleMat, tresholdedMat, threshold, 255, ThresholdTypes.Binary);
                    using var invertedMat2 = (~tresholdedMat).ToMat();
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

            return result.Text?.Trim();
        }

        public Point LocateText(Bitmap image, string text)
        {
            lock (Lock)
            {

                //page.

                using var sectionMat = image.ToMat();

                using var scaledSectionMat = new Mat();
                using var grayscaleMat = new Mat();
                Cv2.CvtColor(sectionMat, grayscaleMat, ColorConversionCodes.BGR2GRAY);
                using var tresholdedMat = new Mat();

                Cv2.Threshold(grayscaleMat, tresholdedMat, 200, 255, ThresholdTypes.Binary);

                using var invertedMat = (~tresholdedMat).ToMat();
                using var invertedBitmap = invertedMat.ToBitmap();
                using var pix = ToPix(invertedBitmap);

                using var page = Engine.Process(pix);
                using (var iterate = page.GetIterator())
                {
                    iterate.Begin();
                    do
                    {
                        if (!iterate.TryGetBoundingBox(PageIteratorLevel.Word, out var rect)) continue;
                        if (iterate.GetText(PageIteratorLevel.Word) != "Future") continue;
                        
                        iterate.Next(PageIteratorLevel.Word);
                        if (iterate.GetText(PageIteratorLevel.Word) == "Fight")
                        {
                            return new Point(rect.X1 + rect.Width / 2, rect.Y1);
                        }
                    } while (iterate.Next(PageIteratorLevel.Word));
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
            Engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.LstmOnly);
            Engine.SetVariable("debug_file", "/dev/null");
        }

        public static TextRecognitionResult GetText(Pix pix, int psm = 3)
        {
            lock (Lock)
            {
                using var page = Engine.Process(pix, (PageSegMode)psm);
                return new TextRecognitionResult(page.GetMeanConfidence(), page.GetText());
            }
        }
    }
}
