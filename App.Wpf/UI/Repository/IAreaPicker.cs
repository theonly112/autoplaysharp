using System.Numerics;
using System.Threading.Tasks;

namespace autoplaysharp.App.UI.Repository
{
    internal interface IAreaPicker
    {
        Task<(bool Cancelled, Vector2 Position, Vector2 Size)> PickArea();
    }
}
