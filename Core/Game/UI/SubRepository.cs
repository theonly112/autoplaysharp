using autoplaysharp.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace autoplaysharp.Core.Game.UI
{
    internal class SubRepository : IUiSubRepository
    {
        private readonly string _path;
        private Dictionary<string, UiElement> _repository = new Dictionary<string, UiElement>();

        public SubRepository(string path)
        {
            _path = path;
        }

        internal void Save()
        {
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            var json = JsonConvert.SerializeObject(_repository.Values, Formatting.Indented, settings);
            File.WriteAllText(_path, json);
        }

        internal void Load()
        {
            var elements = JsonConvert.DeserializeObject<List<UiElement>>(File.ReadAllText(_path));
            foreach (var e in elements)
            {
                _repository.Add(e.Id, e);
            }
        }

        public IEnumerable<string> Ids => _repository.Keys;

        public string Name => Path.GetFileNameWithoutExtension(_path);

        public UiElement this[string id]
        {
            get => _repository[id];
            set => _repository[id] = value;
        }

        public void Add(string id)
        {
            _repository.Add(id, new UiElement() { Id = id });
        }

        public void Add(UiElement element)
        {
            if(string.IsNullOrWhiteSpace(element.Id))
            {
                throw new ArgumentException(nameof(element.Id));
            }
            _repository.Add(element.Id, element);
        }

        public void Remove(string id)
        {
            _repository.Remove(id);
        }
    }
}
