using Tesseract;

namespace autoplaysharp.Core.OCR
{
    public static class TextRecognition
    {
        private static readonly object _lock = new object();
        private static readonly TesseractEngine _engine;

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
