using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using autoplaysharp.Contracts;
using PInvoke;

namespace autoplaysharp.Emulators
{
    public abstract class BaseEmulatorWindow : IEmulatorWindow
    {
        private readonly Random _random = new Random();

        public Vector2 VirtualMousePosition { get; set; } = Vector2.Zero;


        public int Width
        {
            get
            {
                User32.GetWindowRect(GameAreaHwnd, out var rect);
                return rect.right - rect.left;
            }
        }

        public int Height
        {
            get
            {
                User32.GetWindowRect(GameAreaHwnd, out var rect);
                return rect.bottom - rect.top;
            }
        }

        public int X
        {
            get
            {
                User32.GetWindowRect(GameAreaHwnd, out var rect);
                return rect.left;
            }
        }

        public int Y
        {
            get
            {
                User32.GetWindowRect(GameAreaHwnd, out var rect);
                return rect.top;
            }
        }

        protected abstract IntPtr GameAreaHwnd { get; }

        public void ClickAt(float x, float y)
        {
            int x_relative = (int)(x * Width);
            int y_realtive = (int)(y * Height);
            var lparam = MakeLong(x_relative, y_realtive);
            VirtualMousePosition = new Vector2(x, y);
            ClickAt(GameAreaHwnd, x_relative, y_realtive);
        }

        protected void ClickAt(IntPtr hwnd, int x, int y)
        {
            var lparam = MakeLong(x, y);
            User32.SendMessage(hwnd, User32.WindowMessage.WM_MOUSEMOVE, IntPtr.Zero, new IntPtr(lparam));
            Thread.Sleep(_random.Next(10, 50));
            User32.SendMessage(hwnd, User32.WindowMessage.WM_LBUTTONDOWN, new IntPtr(1), new IntPtr(lparam));
            Thread.Sleep(_random.Next(10, 50));
            User32.SendMessage(hwnd, User32.WindowMessage.WM_LBUTTONUP, new IntPtr(0), new IntPtr(lparam));
        }

        public Vector2 Denormalize(Vector2 normalized)
        {
            return normalized * new Vector2(Width, Height);
        }

        public abstract void RestartGame();

        public void Drag(Vector2 vectorStart, Vector2 vectorEnd)
        {
            int x_start = (int)(vectorStart.X * Width);
            int y_start = (int)(vectorStart.Y * Height);
            var lparam = MakeLong(x_start, y_start);
            VirtualMousePosition = new Vector2(vectorStart.X, vectorStart.Y);
            User32.PostMessage(GameAreaHwnd, User32.WindowMessage.WM_MOUSEMOVE, IntPtr.Zero, new IntPtr(lparam));
            User32.PostMessage(GameAreaHwnd, User32.WindowMessage.WM_LBUTTONDOWN, IntPtr.Zero, new IntPtr(lparam));
            int duration = 500;
            int steps = 10;
            for (int i = 0; i < steps; i++)
            {
                var delta = (vectorEnd - vectorStart) / steps;
                x_start += (int)(delta.X * Width);
                y_start += (int)(delta.Y * Height);
                lparam = MakeLong(x_start, y_start);

                VirtualMousePosition = new Vector2(x_start / (float)Width, y_start / (float)Height);
                var wParam = new IntPtr(1); // Left button down.
                User32.PostMessage(GameAreaHwnd, User32.WindowMessage.WM_MOUSEMOVE, wParam, new IntPtr(lparam));
                Thread.Sleep(duration / steps);
            }
            User32.PostMessage(GameAreaHwnd, User32.WindowMessage.WM_LBUTTONUP, new IntPtr(0), new IntPtr(lparam));
        }

        private int MakeLong(int lo, int hi)
        {
            return hi << 16 | lo & 0xFFFF;
        }

        protected abstract IntPtr ScreenshotHwnd { get; }

        public Bitmap GrabScreen(int x, int y, int w, int h)
        {
            User32.GetWindowRect(ScreenshotHwnd, out var mainWindowRect);

            var x_src = X - mainWindowRect.left + x;
            var y_src = Y - mainWindowRect.top + y;

            return GrabScreen(ScreenshotHwnd, x_src, y_src, w, h);
        }

        protected Bitmap GrabScreen(IntPtr hwnd, int x, int y, int w, int h)
        {
            using var hdcSource = User32.GetWindowDC(ScreenshotHwnd);
            using var hdcMemory = Gdi32.CreateCompatibleDC(hdcSource);
            IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(hdcSource, w, h);
            IntPtr hBitmapOld = Gdi32.SelectObject(hdcMemory, hBitmap);
            var operation = /*SRCCOPY*/0x00CC0020; // | /*CAPTUREBLT   */0x40000000;

            Gdi32.BitBlt(hdcMemory.DangerousGetHandle(), 0, 0, w, h, hdcSource.DangerousGetHandle(), x, y, operation);
            var hBitmap2 = Gdi32.SelectObject(hdcMemory, hBitmapOld);
            var bitmap = Image.FromHbitmap(hBitmap2);

            var res = Gdi32.DeleteObject(hBitmap);
            Gdi32.DeleteDC(hdcMemory);
            return bitmap;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, User32.WNDENUMPROC lpEnumFunc, IntPtr lParam);
    }
}
