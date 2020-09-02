using Tesseract;

namespace autoplaysharp.OCR
{
    class TextRecognition
    {
        private static object _lock = new object();
        private static TesseractEngine _engine;

        static TextRecognition()
        {
            _engine = new TesseractEngine(@"./tessdata", "eng");
            _engine.SetVariable("debug_file", "/dev/null");
        }

        public static string GetText(Pix pix, int psm = 3)
        {
            lock (_lock)
            {
                using (var page = _engine.Process(pix, (PageSegMode)psm))
                {
                    return page.GetText();
                }
            }
        }
    }
}
