using PInvoke;
using System;

namespace autoplaysharp.Core.Emulator
{
    public class BluestacksWindow : BaseEmulatorWindow
    {
        private IntPtr _blueStacksMain;
        private IntPtr _blueStacksGameArea;

        public BluestacksWindow()
        {
            _blueStacksMain = IntPtr.Zero;
            _blueStacksGameArea = IntPtr.Zero;

            var ptr = new User32.WNDENUMPROC((hwnd, param) =>
            {
                char[] text = new char[32];
                User32.GetWindowText(hwnd, text, text.Length);
                var name = new string(text).TrimEnd('\0');
                if (_blueStacksMain == IntPtr.Zero && name == "BlueStacks")
                {
                    _blueStacksMain = hwnd;
                    return false;
                }

                if (_blueStacksMain != IntPtr.Zero && name == "_ctl.Window")
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

        protected override IntPtr ScreenshotHwnd => _blueStacksMain;
    }
}
