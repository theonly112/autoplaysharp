using autoplaysharp.Contracts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace autoplaysharp.Game.UI
{
    public class Repository : IUiRepository
    {
        private Dictionary<string, UIElement> _repository = new Dictionary<string, UIElement>();
        private Dictionary<string, string> _fileMapping = new Dictionary<string, string>();

        public IEnumerable<string> Ids => _repository.Keys.ToList();

        public void Load()
        {
            _repository.Clear();
            _fileMapping.Clear();

            var files = Directory.GetFiles("ui", "*.json");
            foreach (var f in files)
            {
                var elements = JsonConvert.DeserializeObject<List<UIElement>>(File.ReadAllText(f));
                foreach (var e in elements)
                {
                    _fileMapping.Add(e.Id, f);
                    _repository.Add(e.Id, e);
                }
            }
      
        }

        public UIElement this[string id]
        {
            get { return _repository[id]; }
            set { _repository[id] = value; }
        }

        public UIElement this[string id, int column, int row] => GetGridElement(id, column, row);

        public UIElement GetGridElement(string id, int column, int row)
        {
            var element = this[id];
            var cloned = JsonConvert.DeserializeObject<UIElement>(JsonConvert.SerializeObject(element));
            cloned.X += cloned.XOffset * column;
            cloned.Y += cloned.YOffset * row;
            cloned.Id += $"_{column}_{row}";
            return cloned;
        }

        public void Save()
        {
            var lists = _repository.Values.GroupBy(x => _fileMapping[x.Id]).ToList();
            foreach(var l in lists)
            {
                var settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                var json = JsonConvert.SerializeObject(l.ToArray(), Formatting.Indented, settings);
                File.WriteAllText(l.Key, json);
            }
        }
    }
}
