using autoplaysharp.Game.UI;
using System.Collections.Generic;

namespace autoplaysharp.Contracts
{
    public interface IUiRepository
    {
        void Load();
        UIElement this[string id] { get; }
        UIElement this[string id, int column, int row] { get; }
        IEnumerable<string> AllIds { get; }
        IEnumerable<IUiSubRepository> SubRepositories { get; }
        UIElement GetGridElement(string id, int column, int row);
        void Save();
    }
}
