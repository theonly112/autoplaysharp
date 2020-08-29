using autoplaysharp.Contracts;
using autoplaysharp.Game;
using autoplaysharp.Game.UI;
using autoplaysharp.Overlay.Windows;
using ImGuiNET;
using System.Collections.Generic;
using Veldrid;

namespace autoplaysharp.Overlay
{
    internal class ImGuiOverlay : ImGuiOverlayBase
    {

        private List<IOverlaySubWindow> _subWindows = new List<IOverlaySubWindow>();

        public ImGuiOverlay(ITaskExecutioner taskExecutioner, IGame game, NoxWindow window, Repository repository) : base(window)
        {
            _subWindows.Add(new TaskWindow(taskExecutioner, game, repository));
            _subWindows.Add(new RepositoryWindow(repository, window, game));
        }

        protected override void SubmitUI(InputSnapshot snapshot)
        {
            // TODO mouse pos even interessting?
            ImGui.Begin("Debug");
            ImGui.Text($"MousePos: {snapshot.MousePosition}");
            ImGui.End();

            foreach(var w in _subWindows)
            {
                w.Render();
            }
        }
    }
}
