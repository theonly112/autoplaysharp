using autoplaysharp.Contracts;
using autoplaysharp.Game;
using autoplaysharp.Game.UI;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;

namespace autoplaysharp.Overlay.Windows
{
    class RepositoryWindow : IOverlaySubWindow
    {
        private int _selectedUiElement = 0;
        private bool _previewText = false;
        private readonly Repository _repository;
        private readonly IGame _game;
        private NoxWindow _noxWindow;

        public RepositoryWindow(Repository repository, NoxWindow window, IGame game)
        {
            _repository = repository;
            _noxWindow = window;
            _game = game;
        }

        public void Render()
        {
            ShowRepository();
        }

        private void ShowRepository()
        {
            ImGui.Begin("Repository");

            var items = _repository.Ids.ToArray();

            var element = _repository[items[_selectedUiElement]];

            var x = element.X.Value;
            ImGui.SliderFloat("X", ref x, 0, 1);
            element.X = x;

            var y = element.Y.Value;
            ImGui.SliderFloat("Y", ref y, 0, 1);
            element.Y = y;

            if (element.W.HasValue)
            {
                var w = element.W.Value;
                ImGui.SliderFloat("W", ref w, 0, 1);
                element.W = w;
            }

            if (element.H.HasValue)
            {
                var h = element.H.Value;
                ImGui.SliderFloat("H", ref h, 0, 1);
                element.H = h;
            }

            // Draw selected.
            DrawSelectedElement(items, element, x, y);

            if (element.PSM.HasValue)
            {
                var psm = element.PSM.Value;
                string[] mode = Enumerable.Range(0, 14).Select(x => x.ToString()).ToArray();
                var selectedIndex = Array.IndexOf(mode, psm.ToString());
                ImGui.Combo("PSM", ref selectedIndex, mode, mode.Length);
                element.PSM = int.Parse(mode[selectedIndex]);
            }


            ImGui.Combo("UIElements", ref _selectedUiElement, items, items.Length, 30);


            if (ImGui.Button("Reload Repo"))
            {
                _repository.Load();
            }

            if (ImGui.Button("Save Repo"))
            {
                _repository.Save();
            }

            ImGui.Checkbox("Preview Text", ref _previewText);

            ImGui.End();
        }

        private void DrawSelectedElement(string[] items, UIElement element, float x, float y)
        {
            var drawList = ImGui.GetBackgroundDrawList();

            var absoluteLoc = new Vector2(x, y) * new Vector2(_noxWindow.Width, _noxWindow.Height);

            if (element.H.HasValue && element.W.HasValue)
            {
                if (_previewText)
                {
                    ImGui.Text($"Current Text: {_game.GetText(items[_selectedUiElement])}");
                }

                var absoluteSize = new Vector2(element.W.Value, element.H.Value) * new Vector2(_noxWindow.Width, _noxWindow.Height);
                drawList.AddRect(absoluteLoc, absoluteLoc + absoluteSize, 0xff00ff00);

                ImGui.BeginGroup();

                if (element.XOffset.HasValue)
                {
                    var xOffset = element.XOffset.Value;
                    ImGui.SliderFloat("XOffset", ref xOffset, 0, 1);
                    element.XOffset = xOffset;

                    int i = 0;
                    while (true && element.XOffset.Value > 0)
                    {
                        var loc = absoluteLoc + new Vector2(element.XOffset.Value * i, 0) * new Vector2(_noxWindow.Width, _noxWindow.Height);
                        if (loc.X + absoluteSize.X > _noxWindow.Width)
                            break;
                        drawList.AddRect(loc, loc + absoluteSize, 0xff00ff00);
                        i++;
                    }
                }

                if (element.YOffset.HasValue)
                {
                    var yOffset = element.YOffset.Value;
                    ImGui.SliderFloat("YOffset", ref yOffset, 0, 1);
                    element.YOffset = yOffset;
                    int i = 0;
                    while (true && element.YOffset.Value > 0)
                    {
                        var loc = absoluteLoc + new Vector2(0, element.YOffset.Value * i) * new Vector2(_noxWindow.Width, _noxWindow.Height);
                        if (loc.Y + absoluteSize.Y > _noxWindow.Height)
                            break;
                        drawList.AddRect(loc, loc + absoluteSize, 0xff00ff00);
                        i++;
                    }
                }
                ImGui.EndGroup();


                ImGui.Checkbox("Save image?", ref Program.SaveImages);

                if (element.Threshold.HasValue)
                {
                    var thresh = element.Threshold.Value;
                    ImGui.SliderInt("Threshold", ref thresh, 0, 255);
                    element.Threshold = thresh;
                }
            }
            else
            {
                drawList.AddCircleFilled(absoluteLoc, 15, 0xff0000ff);
            }
        }
    }
}
