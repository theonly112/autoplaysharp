using autoplaysharp.Contracts;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace autoplaysharp.App.UI.Repository
{
    internal class RepositoryBrowserViewModel : BindableBase
    {
        private readonly IUiRepository _repository;
        private readonly IEmulatorOverlay _overlay;
        private readonly IAreaPicker _areaPicker;
        private string _selectedSubRepository;
        private UiElementViewModel _selectedElement;
        private readonly IEmulatorWindow _window;

        public RepositoryBrowserViewModel(IUiRepository repository, IEmulatorOverlay overlay, IAreaPicker picker, IEmulatorWindow window)
        {
            _repository = repository;
            _overlay = overlay;
            _areaPicker = picker;
            SubRepositories = new ObservableCollection<string>(_repository.SubRepositories.Select(x => x.Name));
            SelectedSubRepository = SubRepositories.First();
            _window = window;
            SaveRepo = new DelegateCommand(Save);
            ReloadRepo = new DelegateCommand(Reload);
            AddNew = new DelegateCommand(Add);
        }

        private void Add()
        {
            if (string.IsNullOrWhiteSpace(NewId))
                return;
            _repository.SubRepositories.FirstOrDefault(x => x.Name == SelectedSubRepository)?.Add(NewId);
            ReloadSubRepoItems();
        }

        private void Reload()
        {
            _repository.Load();
            SubRepositories.Clear();
            var repos = _repository.SubRepositories.Select(x => x.Name);
            foreach (var item in repos)
            {
                SubRepositories.Add(item);
            }
            SelectedSubRepository = SubRepositories.First();
        }

        private void Save()
        {
            _repository.Save();
        }

        public ObservableCollection<UiElementViewModel> UIElements { get; } = new ObservableCollection<UiElementViewModel>();

        public ObservableCollection<string> SubRepositories { get; }
        
        public string SelectedSubRepository
        {
            get { return _selectedSubRepository; }
            set
            {
                _selectedSubRepository = value;
                if (value == null)
                {
                    return;
                }

                UIElements.Clear();
                ReloadSubRepoItems();
                SelectedElement = UIElements.First();
                RaisePropertyChanged();
            }
        }

        private void ReloadSubRepoItems()
        {
            var subRepo = _repository.SubRepositories.First(x => x.Name == _selectedSubRepository);
            var items = subRepo.Ids.Select(x => new UiElementViewModel(subRepo, subRepo[x], _areaPicker, _window));
            foreach (var item in items)
            {
                UIElements.Add(item);
            }
        }

        public UiElementViewModel SelectedElement
        {
            get { return _selectedElement; }
            set
            {
                _selectedElement = value;
                _overlay.SelectedUiElement = _selectedElement?.UIElement;
                RaisePropertyChanged();
            }
        }

        public bool PreviewText
        {
            get { return _overlay.PreviewElementText; }
            set
            {
                _overlay.PreviewElementText = value; 
            }
        }

        public string NewId { get; set; }
        public ICommand AddNew { get; }
        public ICommand SaveRepo { get; }
        public ICommand ReloadRepo { get; }
    }
}
