using PInvoke;
using System;

namespace autoplaysharp.Core.Emulator
{
    public class NoxWindow : BaseEmulatorWindow
    {
        private IntPtr _noxMainWindow;
        private IntPtr _noxGameAreaHwnd;
        public NoxWindow(string windowName)
        {
            _noxMainWindow = IntPtr.Zero;
            _noxGameAreaHwnd = IntPtr.Zero;

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

        }

        protected override IntPtr GameAreaHwnd => _noxGameAreaHwnd;

        protected override IntPtr ScreenshotHwnd => _noxMainWindow;
    }
}
