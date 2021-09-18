using autoplaysharp.Contracts;
using PInvoke;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace autoplaysharp.App.UI.Repository
{
    internal class AreaPicker : IAreaPicker
    {
        private User32.SafeHookHandle _keyboardHookId;
        private User32.SafeHookHandle _mouseHookId;
        private User32.WindowsHookDelegate _keyboardHook;
        private User32.WindowsHookDelegate _mouseHook;
        private readonly IEmulatorWindow _emulatorWindow;
        private Vector2 _posStart = Vector2.Zero;

        private TaskCompletionSource<(bool Cancelled, Vector2 Position, Vector2 Size)> _taskCompletionSource;
        private readonly IEmulatorOverlay _overlay;

        public AreaPicker(IEmulatorWindow emulatorWindow, IEmulatorOverlay overlay)
        {
            _emulatorWindow = emulatorWindow;
            _overlay = overlay;
        }

        public Task<(bool Cancelled, Vector2 Position, Vector2 Size)> PickArea()
        {
            if(_taskCompletionSource != null)
            {
                throw new InvalidOperationException("Already picking");
            }

            _taskCompletionSource = new TaskCompletionSource<(bool Cancelled, Vector2 Position, Vector2 Size)>();
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            if (curModule == null)
            {
                throw new InvalidOperationException();
            }
            
            var modHandle = Kernel32.GetModuleHandle(curModule.ModuleName);

            _keyboardHook = KeyboardHook;
            _mouseHook = MouseHook;
            _keyboardHookId = User32.SetWindowsHookEx(User32.WindowsHookType.WH_KEYBOARD_LL, _keyboardHook, modHandle, 0);
            _mouseHookId = User32.SetWindowsHookEx(User32.WindowsHookType.WH_MOUSE_LL, _mouseHook, modHandle, 0);

            return _taskCompletionSource.Task;
        }

        private int KeyboardHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var msg = (User32.WindowMessage)wParam;
                if (msg == User32.WindowMessage.WM_KEYDOWN)
                {
                    var args = Marshal.PtrToStructure<KeyboardLowLevelHookStruct>(lParam);
                    if (args.vkCode == User32.VirtualKey.VK_ESCAPE)
                    {
                        Debug.WriteLine("Escape");
                        CleanUp();
                        _taskCompletionSource?.SetResult((true, Vector2.Zero, Vector2.Zero));
                    }

                    if (args.vkCode == User32.VirtualKey.VK_LCONTROL && _posStart == Vector2.Zero)
                    {
                        Debug.WriteLine("Button down");
                        var cursorPos = User32.GetCursorPos();
                        _posStart = new Vector2(
                            Math.Max(0, cursorPos.x - _emulatorWindow.X) / (float)_emulatorWindow.Width,
                            Math.Max(0, cursorPos.y - _emulatorWindow.Y) / (float)_emulatorWindow.Height);
                        _overlay.SelectionBox = (_posStart, Vector2.Zero);
                    }

                }
                else if(msg == User32.WindowMessage.WM_KEYUP)
                {
                    var args = Marshal.PtrToStructure<KeyboardLowLevelHookStruct>(lParam);
                    if (args.vkCode == User32.VirtualKey.VK_LCONTROL)
                    {
                        Debug.WriteLine("Button up");
                        var cursorPos = User32.GetCursorPos();
                        var posEnd = new Vector2(
                            Math.Max(0, cursorPos.x - _emulatorWindow.X) / (float)_emulatorWindow.Width,
                            Math.Max(0, cursorPos.y - _emulatorWindow.Y) / (float)_emulatorWindow.Height);
                        _overlay.SelectionBox = (Vector2.Zero, Vector2.Zero);
                        _taskCompletionSource.SetResult((false, _posStart, posEnd - _posStart));
                        _posStart = Vector2.Zero;
                        CleanUp();
                    }
                }

            }

            return User32.CallNextHookEx(_keyboardHookId.DangerousGetHandle(), nCode, wParam, lParam);
        }

        private void CleanUp()
        {
            if (_mouseHookId != null)
            {
                UnhookWindowsHookEx(_mouseHookId.DangerousGetHandle());
                _mouseHookId.Dispose();
            }
            if (_keyboardHookId != null)
            {
                UnhookWindowsHookEx(_keyboardHookId.DangerousGetHandle());
                _keyboardHookId.Dispose();
            }
            _taskCompletionSource = null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        int MouseHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var msg = (User32.WindowMessage)wParam;
                if (msg == User32.WindowMessage.WM_MOUSEMOVE)
                {
                    var args = Marshal.PtrToStructure<MouseLowLevelHookStruct>(lParam);
                    var posEnd = new Vector2(
                        Math.Max(0, args.pt.x - _emulatorWindow.X) / (float)_emulatorWindow.Width,
                        Math.Max(0, args.pt.y - _emulatorWindow.Y) / (float)_emulatorWindow.Height);
                    _overlay.SelectionBox = (_posStart, posEnd - _posStart);
                }
            }

            return User32.CallNextHookEx(_mouseHookId.DangerousGetHandle(), nCode, wParam, lParam);
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [StructLayout(LayoutKind.Sequential)]
        internal struct KeyboardLowLevelHookStruct
        {
            public User32.VirtualKey vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseLowLevelHookStruct
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
    }
}
