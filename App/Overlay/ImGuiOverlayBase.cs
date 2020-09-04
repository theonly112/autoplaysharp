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
    abstract class ImGuiOverlayBase : IDisposable
    {
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;
        private static ImGuiController _controller;
        protected readonly IEmulatorWindow NoxWindow;

        [DllImport("dwmapi.dll")]
        static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref int[] pMargins);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public ImGuiOverlayBase(IEmulatorWindow window)
        {
            NoxWindow = window;
            Setup();
        }

        private void Setup()
        {
            _window = VeldridStartup.CreateWindow(new WindowCreateInfo(NoxWindow.X, NoxWindow.Y, NoxWindow.Width, NoxWindow.Height, WindowState.Normal, "autoplaysharp"));
            

            var handle = _window.Handle;
            User32.SetWindowLong(handle, User32.WindowLongIndexFlags.GWL_EXSTYLE, User32.SetWindowLongFlags.WS_EX_LAYERED | User32.SetWindowLongFlags.WS_EX_TOPMOST);
            SetLayeredWindowAttributes(handle, 0xFF000000, 255, 1);
            //_window.X = NoxWindow.X;
            //_window.Y = NoxWindow.Y;
            User32.SetWindowPos(handle, new IntPtr(-1), 0, 0, 0, 0, User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOSIZE | User32.SetWindowPosFlags.SWP_SHOWWINDOW);
            //User32.SetWindowLong(handle, User32.WindowLongIndexFlags.GWL_STYLE, User32.SetWindowLongFlags.WS_VISIBLE | User32.SetWindowLongFlags.WS_POPUP);


            //int[] marg = new[] { 0, 0, _window.Width, _window.Height };
            //DwmExtendFrameIntoClientArea(_window.Handle, ref marg);


            _gd = VeldridStartup.CreateGraphicsDevice(_window,
                new GraphicsDeviceOptions(),
                   GraphicsBackend.Direct3D11);
            _window.Resized += () =>
            {
                _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
            };

            _cl = _gd.ResourceFactory.CreateCommandList();
            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);
        }

        public void Update()
        {
            InputSnapshot snapshot = _window.PumpEvents();
            if (!_window.Exists) { return; }

            _controller.Update(1000 / 60, snapshot);

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
