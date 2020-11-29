using System;
using System.Collections.Generic;
using PInvoke;

namespace autoplaysharp.Emulators
{
    public class NoxWindow : BaseEmulatorWindow
    {
        private readonly string _windowName;
        private IntPtr _noxMainWindow;
        private IntPtr _noxGameAreaHwnd;
        public NoxWindow(string windowName)
        {
            _windowName = windowName;
            _noxMainWindow = IntPtr.Zero;
            _noxGameAreaHwnd = IntPtr.Zero;


        }

        protected override IntPtr GameAreaHwnd => _noxGameAreaHwnd;

        public override void RestartGame()
        {
            throw new NotImplementedException();
        }

        public override void Initialize()
        {
            var tuple = FindWindow(_windowName);
            _noxMainWindow = tuple.MainHwnd;
            _noxGameAreaHwnd = tuple.GameArea;
            User32.SetForegroundWindow(_noxMainWindow);
        }


        private (IntPtr MainHwnd, IntPtr GameArea) FindWindow(string settingsWindowName)
        {
            var possible = FindPossibleHwnds();
            foreach (var valueTuple in possible)
            {
                if (User32.GetWindowText(valueTuple.MainHwnd) == settingsWindowName)
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
                if (!className.Contains("Qt5QWindowIcon"))
                {
                    return true;
                }

                var childHwnd = User32.FindWindowEx(hwnd, IntPtr.Zero, null, "ScreenBoardClassWindow");
                if (childHwnd != IntPtr.Zero)
                {
                    windowList.Add((hwnd, childHwnd));
                    return false;
                }

                return true;
            });

            User32.EnumWindows(ptr, IntPtr.Zero);
            return windowList;
        }
        /*
         *
            var ptr = new User32.WNDENUMPROC((hwnd, param) =>
            {
                char[] text = new char[32];
                User32.GetWindowText(hwnd, text, text.Length);
                var name = new string(text).TrimEnd('\0');
                if (_noxMainWindow == IntPtr.Zero && name == windowName)
                {
                    _noxMainWindow = hwnd;
                    return false;
                }

                if (_noxMainWindow != IntPtr.Zero && name == "ScreenBoardClassWindow")
                {
                    _noxGameAreaHwnd = hwnd;
                    return false;
                }
                return true;
            });

            while (User32.EnumWindows(ptr, IntPtr.Zero))
            {

            }

            while (EnumChildWindows(_noxMainWindow, ptr, IntPtr.Zero))
            {

            }

            User32.SetForegroundWindow(_noxMainWindow);
         *
         */

        protected override IntPtr ScreenshotHwnd => _noxMainWindow;
    }
}
