using System.Drawing;
using System.IO;
using Tesseract;

namespace autoplaysharp.Helper
{
    public static class BitmapExtensions
    {
        public static Pix ToPix(this Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Tiff);
                return Pix.LoadTiffFromMemory(stream.ToArray());
            }
        }

        public static Bitmap Crop(this Bitmap bitmap, int x, int y, int width, int height)
        {
            var format = bitmap.PixelFormat;
            var rect = new Rectangle(x, y, width, height);
            return bitmap.Clone(rect, format);
        }
    }
}
