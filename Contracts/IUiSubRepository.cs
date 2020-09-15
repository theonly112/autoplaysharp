﻿using autoplaysharp.Game.UI;
using System.Collections.Generic;

namespace autoplaysharp.Contracts
{
    public interface IUiSubRepository
    {
        string Name { get; }
        IEnumerable<string> Ids { get; }
        void Add(string id);
        void Add(UIElement element);
        void Remove(string id);
        UIElement this[string id] { get; }
    }
}