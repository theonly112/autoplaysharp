using System.Drawing;
using autoplaysharp.Game.UI;

namespace autoplaysharp.Contracts
{
    public interface ITextRecognition
    {
        string GetText(Bitmap section, UIElement element);
        Point LocateText(Bitmap image, string text);
    }
}
