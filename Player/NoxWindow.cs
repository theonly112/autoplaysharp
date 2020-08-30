using autoplaysharp.Helper;
using PInvoke;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace autoplaysharp.Game
{
    class NoxWindow
    {
        private Random _random = new Random();
        private IntPtr _noxMainWindow;
        private IntPtr _noxGameAreaHwnd;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, User32.WNDENUMPROC lpEnumFunc, IntPtr lParam);

        internal void ClickAt(double x, double y)
        {
            int x_relative = (int)(x * Width);
            int y_realtive = (int)(y * Height);
            var lparam = MakeLong(x_relative, y_realtive);
            Thread.Sleep(_random.Next(10, 50));
            User32.SendMessage(_noxGameAreaHwnd, User32.WindowMessage.WM_MOUSEMOVE, IntPtr.Zero, new IntPtr(lparam));
            User32.SendMessage(_noxGameAreaHwnd, User32.WindowMessage.WM_LBUTTONDOWN, new IntPtr(1), new IntPtr(lparam));
            Thread.Sleep(_random.Next(10, 50));
            User32.SendMessage(_noxGameAreaHwnd, User32.WindowMessage.WM_LBUTTONUP, new IntPtr(0), new IntPtr(lparam));

            User32.SetCursorPos(X + x_relative, Y + y_realtive);
        }

        internal void Drag(Vector2 vectorStart, Vector2 vectorEnd)
        {
            int x_start = (int)(vectorStart.X * Width);
            int y_start = (int)(vectorStart.Y * Height);
            var lparam = MakeLong(x_start, y_start);
            User32.SendMessage(_noxGameAreaHwnd, User32.WindowMessage.WM_LBUTTONDOWN, new IntPtr(1), new IntPtr(lparam));
            int duration = 500;
            int steps = 10;
            for(int i = 0; i < steps; i++)
            {
                var delta = (vectorEnd - vectorStart) / steps;
                x_start += (int)(delta.X * Width);
                y_start += (int)(delta.Y * Height);
                lparam = MakeLong(x_start, y_start);
                User32.SendMessage(_noxGameAreaHwnd, User32.WindowMessage.WM_MOUSEMOVE, new IntPtr(1), new IntPtr(lparam));
                Thread.Sleep(duration / steps);
            }
            User32.SendMessage(_noxGameAreaHwnd, User32.WindowMessage.WM_LBUTTONUP, new IntPtr(0), new IntPtr(lparam));
        }

        private int MakeLong(int lo, int hi)
        {
            return (hi << 16) | (lo & 0xFFFF);
        }

        public NoxWindow()
        {
            _noxMainWindow = IntPtr.Zero;
            _noxGameAreaHwnd = IntPtr.Zero;

            var ptr = new User32.WNDENUMPROC((hwnd, param) =>
            {
                char[] text = new char[32];
                User32.GetWindowText(hwnd, text, text.Length);
                var name = new string(text).TrimEnd('\0');
                if (_noxMainWindow == IntPtr.Zero && name == "NoxPlayer")
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

        }

        public int Width
        {
            get
            {
                User32.GetWindowRect(_noxGameAreaHwnd, out var rect);
                return rect.right - rect.left;
            }
        }

        public int Height
        {
            get
            {
                User32.GetWindowRect(_noxGameAreaHwnd, out var rect);
                return rect.bottom - rect.top;
            }
        }

        public int X
        {
            get
            {
                User32.GetWindowRect(_noxGameAreaHwnd, out var rect);
                return rect.left;
            }
        }

        public int Y
        {
            get
            {
                User32.GetWindowRect(_noxGameAreaHwnd, out var rect);
                return rect.top;
            }
        }

        public Bitmap GrabScreen()
        {
            User32.GetWindowRect(_noxMainWindow, out var mainWindowRect);
            var width = mainWindowRect.right - mainWindowRect.left;
            var height = mainWindowRect.bottom - mainWindowRect.top;
            
            using var bitmap = new Bitmap(width, height);
            
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                var res = User32.PrintWindow(_noxMainWindow, g.GetHdc(), User32.PrintWindowFlags.PW_FULLWINDOW);
            }

            var x = X - mainWindowRect.left;
            var y = Y - mainWindowRect.top;
            return bitmap.Crop(x, y, Width, Height);
        }


        public Bitmap GrabScreen(int x, int y, int w, int h)
        {
            User32.GetWindowRect(_noxMainWindow, out var mainWindowRect);

            using var hdcSource = User32.GetWindowDC(_noxMainWindow);
            using var hdcMemory = Gdi32.CreateCompatibleDC(hdcSource);

            IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(hdcSource, w, h);
            IntPtr hBitmapOld = Gdi32.SelectObject(hdcMemory, hBitmap);

            var x_src = X - mainWindowRect.left + x;
            var y_src = Y - mainWindowRect.top + y;
            var operation = /*SRCCOPY*/0x00CC0020; // | /*CAPTUREBLT   */0x40000000;

            Gdi32.BitBlt(hdcMemory.DangerousGetHandle(), 0, 0, w, h, hdcSource.DangerousGetHandle(), x_src, y_src, operation);
            var hBitmap2 = Gdi32.SelectObject(hdcMemory, hBitmapOld);
            var bitmap = Image.FromHbitmap(hBitmap2);

            var res = Gdi32.DeleteObject(hBitmap);
            Gdi32.DeleteDC(hdcMemory);

            return bitmap;
        }
    }
}
