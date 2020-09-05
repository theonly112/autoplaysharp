using System.Drawing;
using System.Numerics;

namespace autoplaysharp.Contracts
{
    public interface IEmulatorWindow
    {
        int Width { get; }
        int Height { get; }
        int X { get; }
        int Y { get; }
        Vector2 VirtualMousePosition { get; }

        void ClickAt(float x, float y);
        void Drag(Vector2 from, Vector2 to);
        Bitmap GrabScreen(int x, int y, int w, int h);
        Vector2 Denormalize(Vector2 normalized);
    }
}
