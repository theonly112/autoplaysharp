using System.Collections.Generic;
using System.IO;
using System.Linq;
using autoplaysharp.Contracts;
using Newtonsoft.Json;

namespace autoplaysharp.Core.Game.UI
{
    public class Repository : IUiRepository
    {
        private List<SubRepository> _subRepositories = new List<SubRepository>();
        public IEnumerable<string> AllIds => _subRepositories.SelectMany(x => x.Ids).ToList();

        public IEnumerable<IUiSubRepository> SubRepositories => _subRepositories;

        public void Load()
        {
            _subRepositories.Clear();

            var files = Directory.GetFiles("ui", "*.json");
            foreach (var f in files)
            {
                var subRepo = new SubRepository(f);
                subRepo.Load();
                _subRepositories.Add(subRepo);
            }
      
        }

        public UIElement this[string id]
        {
            get { return _subRepositories.First(x => x.Ids.Contains(id))[id]; }
            set 
            { 
                var repo = _subRepositories.First(x => x.Ids.Contains(id));
                repo[id] = value;
            }
        }

        public UIElement this[string id, int column, int row] => GetGridElement(id, column, row);

        public UIElement GetGridElement(string id, int column, int row)
        {
            var element = this[id];
            var cloned = JsonConvert.DeserializeObject<UIElement>(JsonConvert.SerializeObject(element));
            cloned.X += cloned.XOffset.HasValue ? cloned.XOffset.Value *  column : 0;
            cloned.Y += cloned.YOffset.HasValue ? cloned.YOffset.Value * row : 0;
            cloned.Id += $"_{column}_{row}";
            return cloned;
        }

        public void Save()
        {
            foreach (var repository in _subRepositories)
            {
                repository.Save();
            }
        }
    }
}
