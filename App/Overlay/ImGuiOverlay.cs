﻿using autoplaysharp.Contracts;
using autoplaysharp.Core.Game.UI;
using autoplaysharp.Game.UI;
using autoplaysharp.Overlay.Windows;
using ImGuiNET;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Veldrid;

namespace autoplaysharp.Overlay
{
    internal class ImGuiOverlay : ImGuiOverlayBase, IEmulatorOverlay
    {
        private readonly IEmulatorWindow _window;
        private List<IOverlaySubWindow> _subWindows = new List<IOverlaySubWindow>();

        public ImGuiOverlay(ITaskExecutioner taskExecutioner, IGame game, IEmulatorWindow window, IUiRepository repository) : base(window)
        {
            _subWindows.Add(new TaskWindow(taskExecutioner, game, repository));
            _subWindows.Add(new RepositoryWindow(repository, window, game));
            _window = window;
        }

        private abstract class OverlayElement
        {
            private int _duration;

            public OverlayElement(int duration = 3000)
            {
                _duration = duration;
            }
            public bool CanBeRemoved()
            {
                return _duration <= 0;
            }

            public virtual void Render(int delta)
            {
                _duration -= delta;
            }
        }

        private class UiElementOverlayElement : OverlayElement
        {
            protected readonly UIElement UIElement;
            protected readonly IEmulatorWindow EmulatorWindow;
            protected uint Color = 0xff00FFFF;

            public UiElementOverlayElement(UIElement uIElement, IEmulatorWindow emulatorWindow, int duration = 3000) : base(duration)
            {
                UIElement = uIElement;
                EmulatorWindow = emulatorWindow;
            }

            public override void Render(int delta)
            {
                base.Render(delta);
                var drawList = ImGui.GetBackgroundDrawList();
                drawList.AddRect(UIElement.GetDenormalizedLocation(EmulatorWindow), UIElement.GetDenormalizedLocationBottomRight(EmulatorWindow), Color);
                drawList.AddText(ImGui.GetFont(), 16, UIElement.GetDenormalizedLocation(EmulatorWindow), Color, UIElement.Id);
            }
        }

        private class IsVisibleUiElement : UiElementOverlayElement
        {
            public IsVisibleUiElement(UIElement element, IEmulatorWindow window, bool isVisible) : base(element, window, isVisible ? 5000 : 3000)
            {
                Color = isVisible ? 0xff00ff00 : 0xff0000ff; // Green for found, red for not found.
            }
        }

        private object _lock = new object();
        private readonly List<OverlayElement> _elementsToRenders = new List<OverlayElement>();

        public void ShowGetText(UIElement uIElement)
        {
            lock (_lock)
            {
                _elementsToRenders.Add(new UiElementOverlayElement(uIElement, NoxWindow));
            }
        }

        public void ShowIsVisibile(UIElement uIElement, bool isVisible)
        {
            lock (_lock)
            {
                _elementsToRenders.Add(new IsVisibleUiElement(uIElement, NoxWindow, isVisible));
            }
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

            lock (_lock)
            {
                foreach (var element in _elementsToRenders)
                {
                    // todo: pass actuall time passed...
                    element.Render(1000 / 60);
                }
                _elementsToRenders.RemoveAll(e => e.CanBeRemoved());
            }

            foreach (var w in _subWindows)
            {
                w.Render();
            }
        }
    }
}
