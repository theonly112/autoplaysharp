using System.Collections.Generic;

namespace autoplaysharp.Contracts
{
    public interface IUiSubRepository
    {
        string Name { get; }
        IEnumerable<string> Ids { get; }
        void Add(string id);
        void Add(UiElement element);
        void Remove(string id);
        UiElement this[string id] { get; }
    }
}
