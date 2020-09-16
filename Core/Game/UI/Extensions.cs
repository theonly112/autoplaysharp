using autoplaysharp.Contracts;
using autoplaysharp.Game.UI;
using Newtonsoft.Json;
using System.Numerics;

namespace autoplaysharp.Core.Game.UI
{
    public static class Extensions
    {
        public static Vector2 GetDenormalizedLocation(this UIElement element, IEmulatorWindow window)
        {
            return window.Denormalize(new Vector2(element.X.Value, element.Y.Value));
        }

        public static Vector2 GetDenormalizedLocationBottomRight(this UIElement element, IEmulatorWindow window)
        {
            return GetDenormalizedLocation(element, window) + window.Denormalize(new Vector2(element.W.Value, element.H.Value));
        }

        public static UIElement Clone(this UIElement element)
        {
            return JsonConvert.DeserializeObject<UIElement>(JsonConvert.SerializeObject(element));
        }
    }
}
