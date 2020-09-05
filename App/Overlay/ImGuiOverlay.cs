﻿using autoplaysharp.Contracts;
using autoplaysharp.Overlay.Windows;
using ImGuiNET;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Veldrid;

namespace autoplaysharp.Overlay
{
    internal class ImGuiOverlay : ImGuiOverlayBase
    {
        private readonly IEmulatorWindow _window;
        private List<IOverlaySubWindow> _subWindows = new List<IOverlaySubWindow>();

        public ImGuiOverlay(ITaskExecutioner taskExecutioner, IGame game, IEmulatorWindow window, IUiRepository repository) : base(window)
        {
            _subWindows.Add(new TaskWindow(taskExecutioner, game, repository));
            _subWindows.Add(new RepositoryWindow(repository, window, game));
            _window = window;
        }

        protected override void SubmitUI(InputSnapshot snapshot)
        {
            // TODO mouse pos even interessting?
            ImGui.Begin("Debug");
            var drawList = ImGui.GetForegroundDrawList();
            var absPos = _window.VirtualMousePosition * new System.Numerics.Vector2(_window.Width, _window.Height);
            drawList.AddCircleFilled(absPos, 25, 0xff0000ff);
            if(ImGui.Button("Copy jsons back..."))
            {
                var files = Directory.GetFiles("ui", "*.json");
                var relativPath = @"..\..\..\..\Core\";
                Debug.Assert(Directory.Exists(relativPath));

                foreach (var f in files)
                {
                    File.Copy(f, Path.Combine(relativPath, f), true);
                }
                
            }
            ImGui.Text($"MousePos: {_window.VirtualMousePosition}");
            ImGui.End();

            foreach(var w in _subWindows)
            {
                w.Render();
            }
        }
    }
}
