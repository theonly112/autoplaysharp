using autoplaysharp.Contracts;
using autoplaysharp.Core;
using autoplaysharp.Game;
using autoplaysharp.Game.UI;
using autoplaysharp.Helper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Drawing;
using autoplaysharp.Core.Game;
using autoplaysharp.UiAutomation.OCR;

namespace autoplaysharp.Tests
{
    public abstract class SingleFrameTest
    {
        protected GameImpl Game;
        private IEmulatorWindow _window;

        public void Setup(string fileName)
        {
            var repo = new Repository();
            repo.Load();
            _window = Substitute.For<IEmulatorWindow>();
            var recognition = new TextRecognition();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            Game = new GameImpl(_window, repo, loggerFactory, recognition);
#if DEBUG
            Settings.SaveImages = true;
            Settings.SaveRawImages = true;
#endif

            using var bitmap = new Bitmap(fileName);
            _window.Width.Returns(bitmap.Width);
            _window.Height.Returns(bitmap.Height);
            _window.GrabScreen(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(info =>
                {
                    using var bitmap = new Bitmap(fileName);
                    return bitmap.Crop(info.ArgAt<int>(0), info.ArgAt<int>(1), info.ArgAt<int>(2), info.ArgAt<int>(3));
                });

        }
    }
}
