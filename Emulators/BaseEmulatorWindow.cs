using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using autoplaysharp.Contracts;
using PInvoke;

namespace autoplaysharp.Emulators
{
    public abstract class BaseEmulatorWindow : IEmulatorWindow
    {
        private readonly Random _random = new();

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
            int xRelative = (int)(x * Width);
            int yRelative = (int)(y * Height);
            VirtualMousePosition = new Vector2(x, y);
            ClickAt(GameAreaHwnd, xRelative, yRelative);
        }

        private void ClickAt(IntPtr hwnd, int x, int y)
        {
            var param = MakeLong(x, y);
            User32.SendMessage(hwnd, User32.WindowMessage.WM_MOUSEMOVE, IntPtr.Zero, new IntPtr(param));
            Thread.Sleep(_random.Next(10, 50));
            User32.SendMessage(hwnd, User32.WindowMessage.WM_LBUTTONDOWN, new IntPtr(1), new IntPtr(param));
            Thread.Sleep(_random.Next(10, 50));
            User32.SendMessage(hwnd, User32.WindowMessage.WM_LBUTTONUP, new IntPtr(0), new IntPtr(param));
        }

        public Vector2 Denormalize(Vector2 normalized)
        {
            return normalized * new Vector2(Width, Height);
        }

        public abstract void RestartGame();
        public abstract void Initialize();

        protected abstract IEnumerable<(IntPtr MainHwnd, IntPtr GameArea)> FindPossibleHwnds();
        
        public IEnumerable<string> FindPossibleWindows()
        {
            var hwnds = FindPossibleHwnds();
            return hwnds.Select(x => $"{User32.GetWindowText(x.MainHwnd)}");
        }

        public void Drag(Vector2 vectorStart, Vector2 vectorEnd)
        {
            User32.SetForegroundWindow(ScreenshotHwnd);
            int xStart = (int)(vectorStart.X * Width);
            int yStart = (int)(vectorStart.Y * Height);
            VirtualMousePosition = new Vector2(vectorStart.X, vectorStart.Y);

            var bounds = Screen.FromHandle(GameAreaHwnd).Bounds;
            User32.SetCursorPos(X + xStart, Y + yStart);
            User32.mouse_event(User32.mouse_eventFlags.MOUSEEVENTF_LEFTDOWN |
                User32.mouse_eventFlags.MOUSEEVENTF_ABSOLUTE,
                (int)((X + xStart) / (float)bounds.Width * 65535),
                (int)((Y + yStart) / (float)bounds.Height * 65535),
                0,
                IntPtr.Zero);
            int duration = 500;
            int steps = 10;
            for (int i = 0; i < steps; i++)
            {
                var delta = (vectorEnd - vectorStart) / steps;
                xStart += (int)(delta.X * Width);
                yStart += (int)(delta.Y * Height);

                VirtualMousePosition = new Vector2(xStart / (float)Width, yStart / (float)Height);
                User32.mouse_event(User32.mouse_eventFlags.MOUSEEVENTF_MOVE |
                    User32.mouse_eventFlags.MOUSEEVENTF_ABSOLUTE,
                    (int)((X + xStart) / (float)bounds.Width * 65535), 
                    (int)((Y + yStart) / (float)bounds.Height * 65535),
                    0,
                    IntPtr.Zero);
                Thread.Sleep(duration / steps);
            }

            User32.mouse_event(User32.mouse_eventFlags.MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
        }

        private int MakeLong(int lo, int hi)
        {
            return hi << 16 | lo & 0xFFFF;
        }

        protected abstract IntPtr ScreenshotHwnd { get; }

        public Bitmap GrabScreen(int x, int y, int w, int h)
        {
            User32.GetWindowRect(ScreenshotHwnd, out var mainWindowRect);

            var xSrc = X - mainWindowRect.left + x;
            var ySrc = Y - mainWindowRect.top + y;

            return GrabScreenInt(xSrc, ySrc, w, h);
        }

        private Bitmap GrabScreenInt(int x, int y, int w, int h)
        {
            using var hdcSource = User32.GetWindowDC(ScreenshotHwnd);
            using var hdcMemory = Gdi32.CreateCompatibleDC(hdcSource);
            IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(hdcSource, w, h);
            IntPtr hBitmapOld = Gdi32.SelectObject(hdcMemory, hBitmap);
            var operation = /*SRCCOPY*/0x00CC0020; // | /*CAPTUREBLT   */0x40000000;

            Gdi32.BitBlt(hdcMemory.DangerousGetHandle(), 0, 0, w, h, hdcSource.DangerousGetHandle(), x, y, operation);
            var hBitmap2 = Gdi32.SelectObject(hdcMemory, hBitmapOld);
            var bitmap = Image.FromHbitmap(hBitmap2);

            Gdi32.DeleteObject(hBitmap);
            Gdi32.DeleteDC(hdcMemory);
            return bitmap;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, User32.WNDENUMPROC lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

    }
}
