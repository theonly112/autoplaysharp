using autoplaysharp.Game.UI;
using System.Collections.Generic;

namespace autoplaysharp.Contracts
{
    interface IUiRepository
    {
        void Load();
        UIElement this[string id] { get; }
        UIElement this[string id, int column, int row] { get; }
        IEnumerable<string> Ids { get; }

        UIElement GetGridElement(string id, int column, int row);
        void Save();
    }
}
