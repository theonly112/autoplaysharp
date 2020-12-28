namespace autoplaysharp.Contracts
{
    public class UiElement
    {
        public string Id { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }

        /// <summary>
        /// Width
        /// </summary>
        public float? W { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        public float? H { get; set; }

        /// <summary>
        /// Page Segmentation Mode for OCR (default is 3)
        ///  tesseract --help-psm
        ///       Page segmentation modes:
        /// 0    Orientation and script detection(OSD) only.
        /// 1    Automatic page segmentation with OSD.
        /// 2    Automatic page segmentation, but no OSD, or OCR. (not implemented)
        /// 3    Fully automatic page segmentation, but no OSD. (Default)
        /// 4    Assume a single column of text of variable sizes.
        /// 5    Assume a single uniform block of vertically aligned text.
        /// 6    Assume a single uniform block of text.
        /// 7    Treat the image as a single text line.
        /// 8    Treat the image as a single word.
        /// 9    Treat the image as a single word in a circle.
        /// 10    Treat the image as a single character.
        /// 11    Sparse text. Find as much text as possible in no particular order.
        /// 12    Sparse text with OSD.
        /// 13    Raw line. Treat the image as a single text line,
        ///      bypassing hacks that are Tesseract-specific.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public int? PSM { get; set; }

        /// <summary>
        /// For grayscale tresholding. (removing noise)
        /// </summary>
        public int? Threshold { get; set; }

        /// <summary>
        /// Text expected to be displayed on the UI element
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Allows specification of X offset for elements that appear in columns at the same distance.
        /// </summary>
        public float? XOffset { get; set; }

        /// <summary>
        /// Allows specification of Y offset for elements that appear in rows at the same distance.
        /// </summary>
        public float? YOffset { get; set; }

        /// <summary>
        /// Scale image for better OCR
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// Raw image data.
        /// </summary>
        public byte[] Image { get; set; }

        /// <summary>
        /// Temporary workaround for thresholding issue with certain UIElements,
        /// where neither hardcoded or dynamic thresholding works.
        /// This attempts multiple threshold values to try and get better results. 
        /// </summary>
        public bool? TryHard { get; set; }
    }
}
