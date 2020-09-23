using autoplaysharp.Contracts;
using autoplaysharp.Game;
using PInvoke;
using System;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace autoplaysharp.Overlay
{
    public abstract class ImGuiOverlayBase : IDisposable
    {
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;
        private static ImGuiController _controller;
        protected readonly IEmulatorWindow NoxWindow;

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public ImGuiOverlayBase(IEmulatorWindow window)
        {
            NoxWindow = window;
            Setup();
        }

        private void Setup()
        {
            _window = VeldridStartup.CreateWindow(new WindowCreateInfo(NoxWindow.X, NoxWindow.Y, NoxWindow.Width, NoxWindow.Height, WindowState.Normal, "autoplay# Overlay"));

            var handle = _window.Handle;
            User32.SetWindowLong(handle, User32.WindowLongIndexFlags.GWL_EXSTYLE, User32.SetWindowLongFlags.WS_EX_LAYERED | User32.SetWindowLongFlags.WS_EX_TOPMOST | User32.SetWindowLongFlags.WS_EX_TOOLWINDOW);
            SetLayeredWindowAttributes(handle, 0xFF000000, 255, 1);
            User32.SetWindowPos(handle, new IntPtr(-1), 0, 0, 0, 0, User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOSIZE | User32.SetWindowPosFlags.SWP_SHOWWINDOW);

            SetWindowClasses();

            _gd = VeldridStartup.CreateGraphicsDevice(_window,
                new GraphicsDeviceOptions(),
                   GraphicsBackend.OpenGL);
     

            _cl = _gd.ResourceFactory.CreateCommandList();
            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);
            _window.Resized += () =>
            {
                _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                _controller.WindowResized(_window.Width, _window.Height);
            };
        }

        private bool SetIfChanged(int old, int newValue, Action<int> setter)
        {
            var changed = old != newValue;
            if(changed)
            {
                setter(newValue);
            }
            return changed;
        }

        public void Update()
        {
            UpdatePosition();
            InputSnapshot snapshot = _window.PumpEvents();
            if (!_window.Exists) { return; }

            _controller.Update(1f / 60f, snapshot);

            SubmitUI(snapshot);

            _cl.Begin();
            _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
            var transparent = System.Drawing.Color.Transparent;
            _cl.ClearColorTarget(0, new RgbaFloat(0, 0, 0, 255));
            _controller.Render(_gd, _cl);
            _cl.End();

            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_gd.MainSwapchain);
        }

        private void UpdatePosition()
        {
            var changed = SetIfChanged(_window.Width, NoxWindow.Width, x => _window.Width = x);
            changed |= SetIfChanged(_window.Height, NoxWindow.Height, x => _window.Height = x);
            changed |= SetIfChanged(_window.X, NoxWindow.X, x => _window.X = x);
            changed |= SetIfChanged(_window.Y, NoxWindow.Y, x => _window.Y = x);
            if (changed)
            {
                User32.SetWindowPos(_window.Handle, new IntPtr(-1), 0, 0, 0, 0, User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOSIZE | User32.SetWindowPosFlags.SWP_SHOWWINDOW);
                SetWindowClasses();
            }
        }

        private static void SetWindowClasses()
        {
            var classes = User32.GetWindowLong(_window.Handle, User32.WindowLongIndexFlags.GWL_STYLE);
            classes &= ~(int)User32.SetWindowLongFlags.WS_DLGFRAME;
            classes &= ~(int)User32.SetWindowLongFlags.WS_THICKFRAME;
            classes &= ~(int)User32.SetWindowLongFlags.WS_BORDER;
            User32.SetWindowLong(_window.Handle, User32.WindowLongIndexFlags.GWL_STYLE, (User32.SetWindowLongFlags)classes);
        }

        protected abstract void SubmitUI(InputSnapshot snapshot);

        public void Dispose()
        {
            // Clean up Veldrid resources
            _gd.WaitForIdle();
            _cl.Dispose();
            _gd.Dispose();
        }
    }
}
