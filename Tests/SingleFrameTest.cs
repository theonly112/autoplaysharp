using autoplaysharp.Contracts;
using autoplaysharp.Core;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Drawing;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Core.Game;
using autoplaysharp.Core.Game.UI;
using autoplaysharp.Core.Helper;
using autoplaysharp.UiAutomation.OCR;

namespace autoplaysharp.Tests
{
    public abstract class SingleFrameTest
    {
        protected GameImpl Game;
        private IEmulatorWindow _window;
        protected Repository Repository;
        private IVideoProvider _videoProvider;
        public void Setup(string fileName)
        {
            Repository = new Repository();
            Repository.Load();
            _window = Substitute.For<IEmulatorWindow>();
            var settings = Substitute.For<ISettings>();
            var recognition = new TextRecognition();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            _videoProvider = Substitute.For<IVideoProvider>();
            Game = new GameImpl(_window, _videoProvider, Repository, loggerFactory, recognition, settings);
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
                    using var bitmapLocal = new Bitmap(fileName);
                    return bitmapLocal.Crop(info.ArgAt<int>(0), info.ArgAt<int>(1), info.ArgAt<int>(2), info.ArgAt<int>(3));
                });
            _videoProvider.GetCurrentFrame().Returns(info => bitmap);
        }
    }
}
