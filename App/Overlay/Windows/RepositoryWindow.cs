using autoplaysharp.Contracts;
using autoplaysharp.Core;
using autoplaysharp.Game.UI;
using ImGuiNET;
using OpenCvSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace autoplaysharp.Overlay.Windows
{
    class RepositoryWindow : IOverlaySubWindow
    {
        private int _selectedRepo = 0;
        private int _selectedUiElement = 0;
        private bool _previewText = false;
        private readonly IUiRepository _repository;
        private readonly IGame _game;
        private readonly ImGuiOverlay _imguiOverlay;
        private IEmulatorWindow _noxWindow;
        private string _id;

        public RepositoryWindow(IUiRepository repository, IEmulatorWindow window, IGame game, ImGuiOverlay imGuiOverlay)
        {
            _repository = repository;
            _noxWindow = window;
            _game = game;
            _imguiOverlay = imGuiOverlay;
        }

        public void Render()
        {
            ShowRepository();
        }

        private void ShowRepository()
        {
            if (!ImGui.Begin("Repository"))
            {
                ImGui.End();
                return;
            }

            var repos = _repository.SubRepositories.ToList();
            var subRepo = repos[_selectedRepo];
            var items = subRepo.Ids.ToArray();
            _selectedUiElement = Math.Min(_selectedUiElement, items.Length - 1);

            ImGui.Combo("Repository", ref _selectedRepo, repos.Select(x => x.Name).ToArray(), repos.Count, 30);

            if (ImGui.Button("Add"))
            {
                subRepo.Add(_id);
            }
            ImGui.SameLine();
            _id = "new_id";
            ImGui.InputText("Id", ref _id, 64);

            ImGui.Combo("UIElements", ref _selectedUiElement, items, items.Length, 30);
            if (ImGui.Button("Remove"))
            {
                subRepo.Remove(items[_selectedUiElement]);
                items = subRepo.Ids.ToArray();
                _selectedUiElement = Math.Min(_selectedUiElement, items.Length - 1);
            }

            var element = _repository[items[_selectedUiElement]];
            ShowElementProperties(element);


            // Draw selected.
            if (element.X.HasValue && element.Y.HasValue)
            {
                DrawSelectedElement(element);
            }

            if (element.Image != null)
            {
                ImGui.Text($"Is Visible: {_game.IsVisible(items[_selectedUiElement])}");
            }

            if (ImGui.Button("Reload Repo"))
            {
                _repository.Load();
            }

            if (ImGui.Button("Save Repo"))
            {
                _repository.Save();
            }

            if (ImGui.Button("Copy jsons back..."))
            {
                var files = Directory.GetFiles("ui", "*.json");
                var relativPath = @"..\..\..\..\Core\";
                Debug.Assert(Directory.Exists(relativPath));

                foreach (var f in files)
                {
                    File.Copy(f, Path.Combine(relativPath, f), true);
                }
            }

            ImGui.Checkbox("Save raw images", ref Settings.SaveRawImages);
            ImGui.SameLine();
            ImGui.Checkbox("Save image?", ref Settings.SaveImages);
            ImGui.End();
        }

        private void ShowElementProperties(UIElement element)
        {
            ShowFloatProperty(() => element.X, x => element.X = x, "X");
            ShowFloatProperty(() => element.Y, y => element.Y = y, "Y");
            ShowFloatProperty(() => element.W, w => element.W = w, "W");
            ShowFloatProperty(() => element.H, h => element.H = h, "H");

            var hasThreshold = element.Threshold.HasValue;
            ImGui.Checkbox("Treshhold", ref hasThreshold);
            if (hasThreshold)
            {
                ImGui.SameLine();
                if (!element.Threshold.HasValue)
                {
                    element.Threshold = 128;
                }

                var thresh = element.Threshold.Value;
                ImGui.SliderInt("Threshold", ref thresh, 0, 255);
                element.Threshold = thresh;
            }
            else
            {
                element.Threshold = null;
            }

            var hasText = element.Text != null;
            ImGui.Checkbox("Text", ref hasText);
            if (hasText)
            {
                ImGui.SameLine();
                if (element.Text == null)
                {
                    if (element.X.HasValue && element.Y.HasValue &&
                        element.W.HasValue && element.H.HasValue)
                    {
                        element.Text = _game.GetText(element);
                    }
                    else
                    {
                        element.Text = "ABC";
                    }
                }
                var text = element.Text;
                ImGui.InputText("Text", ref text, 64);
                element.Text = text;
            }
            else
            {
                element.Text = null;
            }
            ImGui.Checkbox("Preview Text", ref _previewText);

            if(element.Image != null)
            {
                using var mat = Cv2.ImDecode(element.Image, ImreadModes.AnyColor);
                using var rgba = new Mat();
                Cv2.CvtColor(mat, rgba, ColorConversionCodes.BGRA2RGBA);

                var gd = _imguiOverlay.GraphicsDevice;
                using var texture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)rgba.Cols, (uint)rgba.Height, 1, 1,
                    Veldrid.PixelFormat.R8_G8_B8_A8_UInt, TextureUsage.Sampled));
                
                var id = _imguiOverlay.Controller.GetOrCreateImGuiBinding(gd.ResourceFactory, texture);

                unsafe
                {
                    gd.UpdateTexture(texture, (IntPtr)rgba.DataPointer, (uint)(rgba.Cols * 4 * rgba.Height), 0, 0, 0, (uint)rgba.Cols, (uint)rgba.Height, 1, 0, 0);
                }

                var viewDesc = new TextureViewDescription(texture, Veldrid.PixelFormat.R8_G8_B8_A8_UNorm);
                using var textureView = gd.ResourceFactory.CreateTextureView(viewDesc);
                var id2 = _imguiOverlay.Controller.GetOrCreateImGuiBinding(gd.ResourceFactory, textureView);
                ImGui.Image(id2, new Vector2(rgba.Cols, rgba.Height));
            }

            if (element.PSM.HasValue)
            {
                var psm = element.PSM.Value;
                string[] mode = Enumerable.Range(0, 14).Select(x => x.ToString()).ToArray();
                var selectedIndex = Array.IndexOf(mode, psm.ToString());
                ImGui.Combo("PSM", ref selectedIndex, mode, mode.Length);
                element.PSM = int.Parse(mode[selectedIndex]);
            }
        }

        private static void ShowFloatProperty(Func<float?> getter, Action<float?> setter, string name)
        {
            var property = getter();
            var hasValue = property.HasValue;
            ImGui.Checkbox(name, ref hasValue);
            if (hasValue)
            {
                if (!property.HasValue)
                {
                    property = 0;
                }
                ImGui.SameLine();
                var value = property.Value;
                ImGui.SliderFloat(name, ref value, 0, 1);
                setter(value);
            }
            else
            {
                setter(null);
            }
        }

        private void DrawSelectedElement(UIElement element)
        {
            var drawList = ImGui.GetBackgroundDrawList();

            var absoluteLoc = GetAbsoluteLocation(element);

            if (element.H.HasValue && element.W.HasValue)
            {
                DrawElement(drawList, element);

                ImGui.BeginGroup();

                if (element.XOffset.HasValue)
                {
                    var xOffset = element.XOffset.Value;
                    ImGui.SliderFloat("XOffset", ref xOffset, 0, 1);
                    if(xOffset != 0)
                    {
                        element.XOffset = xOffset;
                    }
                }

                if (element.YOffset.HasValue)
                {
                    var yOffset = element.YOffset.Value;
                    ImGui.SliderFloat("YOffset", ref yOffset, 0, 1);
                    if(yOffset != 0)
                    {
                        element.YOffset = yOffset;
                    }
                }

                if((element.XOffset.HasValue && element.XOffset.Value > 0) 
                    || (element.YOffset.HasValue && element.YOffset.Value > 0))
                {
                    DrawElementGrid(drawList, element);
                }

                ImGui.EndGroup();
         

            }
            else
            {
                drawList.AddCircleFilled(absoluteLoc, 15, 0xff0000ff);
            }
        }

        private void DrawElementGrid(ImDrawListPtr drawList, UIElement element)
        {
            int x = 0;
            int y = 0;
            while (true)
            {
                var dynUiElement = _repository[element.Id, x, y];
                if (!IsVisible(dynUiElement))
                    break;

                while (true)
                {
                    dynUiElement = _repository[element.Id, x, y];
                    if (!IsVisible(dynUiElement))
                    {
                        x = 0;
                        break;
                    }

                    DrawElement(drawList, dynUiElement);

                    if (element.XOffset.HasValue && element.XOffset.Value > 0)
                    {
                        x++;
                    }
                }

                if (element.YOffset.HasValue && element.YOffset.Value > 0)
                {
                    y++;
                }
            }
        }

        private void DrawElement(ImDrawListPtr drawList, UIElement uiElement)
        {
            Vector2 size = GetAbsoluteSize(uiElement);
            Vector2 loc = GetAbsoluteLocation(uiElement);

            drawList.AddRect(loc, loc + size, 0xff00ff00);

            if (_previewText)
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
                var scale = fontSize / (float)ImGui.GetFontSize();
                textSize = textSize * scale;
                var textLoc = Vector2.Max(new Vector2(loc.X, 0), (loc - textSize));
                drawList.AddText(ImGui.GetFont(), fontSize, textLoc, 0xFF0000FF, textToRender);
            }
        }

        private bool IsVisible(UIElement element)
        {
            return element.Y + element.H <= 1 && element.X + element.W <= 1;
        }

        private Vector2 GetAbsoluteSize(UIElement dynUiElement)
        {
            return new Vector2(dynUiElement.W.Value, dynUiElement.H.Value) * new Vector2(_noxWindow.Width, _noxWindow.Height);
        }

        private Vector2 GetAbsoluteLocation(UIElement dynUiElement)
        {
            return new Vector2(dynUiElement.X.Value, dynUiElement.Y.Value) * new Vector2(_noxWindow.Width, _noxWindow.Height);
        }
    }
}
