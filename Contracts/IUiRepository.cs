using System.Collections.Generic;

namespace autoplaysharp.Contracts
{
    public interface IUiRepository
    {
        void Load();
        UiElement this[string id] { get; }
        UiElement this[string id, int column, int row] { get; }
        IEnumerable<string> AllIds { get; }
        IEnumerable<IUiSubRepository> SubRepositories { get; }
        UiElement GetGridElement(string id, int column, int row);
        void Save();
    }
}
