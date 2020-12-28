using autoplaysharp.Contracts;
using Newtonsoft.Json;
using System.Numerics;

namespace autoplaysharp.Core.Game.UI
{
    public static class Extensions
    {
        public static Vector2 GetDenormalizedLocation(this UiElement element, IEmulatorWindow window)
        {
            return window.Denormalize(new Vector2(element.X.GetValueOrDefault(), element.Y.GetValueOrDefault()));
        }

        public static Vector2 GetDenormalizedLocationBottomRight(this UiElement element, IEmulatorWindow window)
        {
            return GetDenormalizedLocation(element, window) + window.Denormalize(new Vector2(element.W.GetValueOrDefault(), element.H.GetValueOrDefault()));
        }

        public static UiElement Clone(this UiElement element)
        {
            return JsonConvert.DeserializeObject<UiElement>(JsonConvert.SerializeObject(element));
        }
    }
}
