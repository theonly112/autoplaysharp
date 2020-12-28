using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
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
        private readonly string _settingsWindowName;
        private IntPtr _blueStacksMain;
        private IntPtr _blueStacksGameArea;

        public BluestacksWindow(ILogger logger, ITextRecognition recognition, string settingsWindowName)
        {
            _logger = logger;
            _recognition = recognition;
            _settingsWindowName = settingsWindowName;
            _blueStacksMain = IntPtr.Zero;
            _blueStacksGameArea = IntPtr.Zero;

        }

        private (IntPtr MainHwnd, IntPtr GameArea) FindWindow(string settingsWindowName)
        {
            var possible = FindPossibleHwnds();
            foreach (var valueTuple in possible)
            {
                if(User32.GetWindowText(valueTuple.MainHwnd) == settingsWindowName)
                {
                    return valueTuple;
                }
            }

            throw new FailedToFindWindowException($"Failed to find BlueStacks Window: {settingsWindowName}");
        }

        protected override IEnumerable<(IntPtr MainHwnd, IntPtr GameArea)> FindPossibleHwnds()
        {
            var windowList = new List<(IntPtr, IntPtr)>();
            var ptr = new User32.WNDENUMPROC((hwnd, _) =>
            {
                var className = User32.GetClassName(hwnd);
                if (!className.Contains("Bluestacks", StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
                
                var childHwnd = User32.FindWindowEx(hwnd, IntPtr.Zero, null, "BlueStacks Android PluginAndroid");
                if(childHwnd != IntPtr.Zero)
                {
                    windowList.Add((hwnd, childHwnd));
                    return false;
                }

                return true;
            });
           
            User32.EnumWindows(ptr, IntPtr.Zero);
            return windowList;
        }


        protected override IntPtr GameAreaHwnd => _blueStacksGameArea;

        public override void RestartGame()
        {
            var originalForeground = User32.GetForegroundWindow();
            var originalPos = Mouse.Position;
            User32.GetWindowThreadProcessId(_blueStacksMain, out var procId);
            using var app = FlaUI.Core.Application.Attach(procId);
            using var automation = new UIA3Automation();
            var mainWindow = app.GetMainWindow(automation);
            var mffTab = mainWindow.FindFirstDescendant(cf => cf.ByText("Future Fight"));
            var close = mffTab?.Parent?.FindFirstDescendant(cf => cf.ByAutomationId("CloseTabButtonLandScape"));
            User32.SetForegroundWindow(_blueStacksMain);

            int attempts = 0;
            Point futureFightLocation;
            do
            {
                if (close != null)
                {

                    Mouse.Position = close.GetClickablePoint();
                    close.Click();
                    Mouse.Position = originalPos;
                }
                else
                {
                    _logger.LogError("Failed to close game");
                    // TODO: restart emulator?
                }

                Thread.Sleep(100);

                var screen = GrabScreen(0, 0, Width, Height);
                futureFightLocation = _recognition.LocateText(screen, "Future Fight");
                attempts++;
            } while (futureFightLocation == Point.Empty && attempts < 10);

            
            Mouse.Position = futureFightLocation + new Size(X, Y);
            Mouse.Click();
            Thread.Sleep(100);
            Mouse.Position = originalPos;
            User32.SetForegroundWindow(originalForeground);
        }

        public override void Initialize()
        {
            var tuple = FindWindow(_settingsWindowName);
            _blueStacksMain = tuple.MainHwnd;
            _blueStacksGameArea = tuple.GameArea;
            User32.SetForegroundWindow(_blueStacksMain);
        }

        protected override IntPtr ScreenshotHwnd => _blueStacksMain;
    }
}
