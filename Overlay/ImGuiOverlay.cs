﻿using autoplaysharp.Contracts;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using System.Linq;
using System;

namespace autoplaysharp.Overlay
{
    public class ImGuiOverlay : ImGuiOverlayBase, IEmulatorOverlay
    {
        private readonly IGame _game;
        private readonly IEmulatorWindow _window;
        private readonly IUiRepository _repository;
        private readonly object _lock = new object();
        private readonly List<OverlayElement> _elementsToRenders = new List<OverlayElement>();

        public ImGuiOverlay(IGame game, IEmulatorWindow window, IUiRepository repository) : base(window)
        {
            _game = game;
            _window = window;
            _repository = repository;
        }

        public UiElement SelectedUiElement { get; set; }
        public (Vector2 Position, Vector2 Size) SelectionBox { get; set; }
        public bool PreviewElementText { get; set; }

        public void ShowGetText(UiElement uIElement)
        {
            lock (_lock)
            {
                var toRemove = _elementsToRenders.OfType<UiElementOverlayElement>().Where(x => x.UiElement == uIElement);
                _elementsToRenders.RemoveAll(x => toRemove.Contains(x));
                _elementsToRenders.Add(new UiElementOverlayElement(uIElement, NoxWindow));
            }
        }

        public void ShowIsVisibile(UiElement uIElement, bool isVisible)
        {
            lock (_lock)
            {
                var toRemove = _elementsToRenders.OfType<UiElementOverlayElement>().Where(x => x.UiElement == uIElement);
                _elementsToRenders.RemoveAll(x => toRemove.Contains(x));
                _elementsToRenders.Add(new IsVisibleUiElement(uIElement, NoxWindow, isVisible));
            }
        }

        protected override void SubmitUi(InputSnapshot snapshot)
        {
            var drawList = ImGui.GetForegroundDrawList();
            var absPos = _window.VirtualMousePosition * new Vector2(_window.Width, _window.Height);
            drawList.AddCircleFilled(absPos, 25, 0xff0000ff);
            
            lock (_lock)
            {
                foreach (var element in _elementsToRenders)
                {
                    // todo: pass actuall time passed...
                    element.Render(1000 / 60);
                }
                _elementsToRenders.RemoveAll(e => e.CanBeRemoved());
            }

            if(SelectedUiElement != null)
            {
                DrawSelectedElement(SelectedUiElement);
            }

            if (SelectionBox.Position != Vector2.Zero && SelectionBox.Size != Vector2.Zero)
            {
                var windowSize = new Vector2(_window.Width, _window.Height);
                drawList.AddRect(SelectionBox.Position * windowSize, (SelectionBox.Position + SelectionBox.Size) * windowSize, 0xFF00FF00);
            }
        }

        private Vector2 GetAbsoluteSize(UiElement dynUiElement)
        {
            return new Vector2(dynUiElement.W.GetValueOrDefault(), dynUiElement.H.GetValueOrDefault()) * new Vector2(_window.Width, _window.Height);
        }

        private Vector2 GetAbsoluteLocation(UiElement dynUiElement)
        {
            return new Vector2(dynUiElement.X.GetValueOrDefault(), dynUiElement.Y.GetValueOrDefault()) * new Vector2(_window.Width, _window.Height);
        }

        private void DrawElement(ImDrawListPtr drawList, UiElement uiElement)
        {
            Vector2 size = GetAbsoluteSize(uiElement);
            Vector2 loc = GetAbsoluteLocation(uiElement);

            drawList.AddRect(loc, loc + size, 0xff00ff00);

            if (PreviewElementText)
            {
                if (uiElement.Image != null)
                {
                    _game.IsVisible(uiElement);
                }
                else
                {
                    var fontSize = 18;
                    var text = _game.GetText(uiElement);

                    string textToRender;
                    if (!string.IsNullOrWhiteSpace(uiElement.Text))
                    {
                        textToRender = $"Found Text: {text} \nMatches expected text: {text == uiElement.Text}";
                    }
                    else
                    {
                        textToRender = $"Found Text: {text}";
                    }

                    var textSize = ImGui.CalcTextSize(textToRender);
                    var scale = fontSize / ImGui.GetFontSize();
                    textSize = textSize * scale;
                    var textLoc = Vector2.Max(new Vector2(loc.X, 0), (loc - textSize));
                    drawList.AddText(ImGui.GetFont(), fontSize, textLoc, 0xFF0000FF, textToRender);

                }

            }
        }

        private void DrawSelectedElement(UiElement element)
        {
            var drawList = ImGui.GetBackgroundDrawList();
            if(!element.X.HasValue || !element.Y.HasValue)
            {
                return;
            }

            var absoluteLoc = GetAbsoluteLocation(element);

            if (element.H.HasValue && element.W.HasValue)
            {
                DrawElement(drawList, element);
            }
            else
            {
                drawList.AddCircleFilled(absoluteLoc, 15, 0xff0000ff);
            }

            if ((element.XOffset.HasValue && element.XOffset.Value > 0)
             || (element.YOffset.HasValue && element.YOffset.Value > 0))
            {
                DrawElementGrid(drawList, element);
            }
        }

        private void DrawElementGrid(ImDrawListPtr drawList, UiElement element)
        {
            var xCount = element.XOffset.HasValue ? Math.Ceiling((1f - element.X.GetValueOrDefault()) / element.XOffset.Value) : 1;
            var yCount = element.YOffset.HasValue ? Math.Ceiling((1f - element.Y.GetValueOrDefault()) / element.YOffset.Value) : 1;
            int y;
            for (y = 0; y < yCount; y++)
            {
                int x;
                for (x = 0; x < xCount; x++)
                {
                    var dynUiElement = _repository[element.Id, x, y];
                    DrawElement(drawList, dynUiElement);
                }
            }
        }

        public void ShowIsVisibile(UiElement uIElement, bool isVisible, double certainty)
        {
            lock (_lock)
            {
                var toRemove = _elementsToRenders.OfType<UiElementOverlayElement>().Where(x => x.UiElement == uIElement);
                _elementsToRenders.RemoveAll(x => toRemove.Contains(x));
                _elementsToRenders.Add(new IsVisibleUiElement(uIElement, NoxWindow, isVisible, certainty));
            }
        }
    }
}
