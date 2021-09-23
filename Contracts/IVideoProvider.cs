using System;
using System.Drawing;

namespace autoplaysharp.Contracts
{
    public interface IVideoProvider
    {
        Bitmap GetCurrentFrame();
        event Action<Bitmap> OnNewFrame;
    }
}
