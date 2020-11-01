using System;
using System.Drawing;
using autoplaysharp.Contracts;
using FlaUI.Core.Input;
using FlaUI.UIA3;
using Microsoft.Extensions.Logging;
using PInvoke;

namespace autoplaysharp.Emulators
{
    public class BluestacksWindow : BaseEmulatorWindow
    {
        private readonly ILogger _logger;
        private readonly ITextRecognition _recognition;
        private IntPtr _blueStacksMain;
        private IntPtr _blueStacksGameArea;

        public BluestacksWindow(ILogger logger, ITextRecognition recognition)
        {
            _logger = logger;
            _recognition = recognition;
            _blueStacksMain = IntPtr.Zero;
            _blueStacksGameArea = IntPtr.Zero;

            var ptr = new User32.WNDENUMPROC((hwnd, param) =>
            {
                char[] text = new char[64];
                User32.GetWindowText(hwnd, text, text.Length);
                var name = new string(text).TrimEnd('\0');
                if (_blueStacksMain == IntPtr.Zero && name == "BlueStacks")
                {
                    _blueStacksMain = hwnd;
                    return false;
                }

                if (_blueStacksMain != IntPtr.Zero && name == "BlueStacks Android PluginAndroid")
                {
                    _blueStacksGameArea = hwnd;
                    return false;
                }
                return true;
            });

            while (User32.EnumWindows(ptr, IntPtr.Zero))
            {

            }

            while (EnumChildWindows(_blueStacksMain, ptr, IntPtr.Zero))
            {

            }

            User32.SetForegroundWindow(_blueStacksMain);

        }

        protected override IntPtr GameAreaHwnd => _blueStacksGameArea;

        public override void RestartGame()
        {
            User32.GetWindowThreadProcessId(_blueStacksMain, out var procId);
            using var app = FlaUI.Core.Application.Attach(procId);
            using var automation = new UIA3Automation();
            var mainWindow = app.GetMainWindow(automation);
            var mffTab = mainWindow.FindFirstDescendant(cf => cf.ByText("Future Fight"));
            var close = mffTab?.Parent?.FindFirstDescendant(cf => cf.ByAutomationId("CloseTabButtonLandScape"));
            if (close != null)
            {
                close.Click();
            }
            else
            {
                _logger.LogError("Failed to close game");
                // TODO: restart emulator?
            }

            var screen = GrabScreen(0, 0, Width, Height);
            var futureFightLocation = _recognition.LocateText(screen, "Future Fight");

            Mouse.Position = futureFightLocation + new Size(X, Y);
            Mouse.Click();
        }

        protected override IntPtr ScreenshotHwnd => _blueStacksMain;
    }
}
