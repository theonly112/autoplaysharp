using autoplaysharp.Contracts;
using Prism.Commands;
using Prism.Mvvm;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using autoplaysharp.Core.Helper;

namespace autoplaysharp.App.UI.Repository
{
    internal class UiElementViewModel : BindableBase
    {
        private readonly IUiSubRepository _repository;
        private string _id;
        private readonly IAreaPicker _areaPicker;
        private readonly IEmulatorWindow _window;
        private readonly IGame _game;
        private ImageSource _image;
        private bool _hasImage;

        public UiElementViewModel(IUiSubRepository repository, UiElement element, IAreaPicker areaPicker, IEmulatorWindow window, IGame game)
        {
            _repository = repository;
            UiElement = element;
            Id = element.Id;
            PickAreaCommand = new DelegateCommand(PickArea);
            PickImageCommand = new DelegateCommand(PickImage);
            _areaPicker = areaPicker;
            _window = window;
            _game = game;
            Image = UiElement.Image == null ? null : ByteToImage(UiElement.Image);
            GetTextCommand = new DelegateCommand(GetText);
        }

        private void GetText()
        {
            UiElement.Text = _game.GetText(UiElement);
            RaisePropertyChanged(nameof(UiElement)); // so text is updated...
        }

        public static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            using MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.CacheOption = BitmapCacheOption.OnLoad;
            biImg.StreamSource = ms;
            biImg.EndInit();
            biImg.Freeze();
            return biImg;
        }

        private async void PickImage()
        {
            await PickAreaAsync();
            int x = (int)(UiElement.X.GetValueOrDefault() * _window.Width),
                y = (int)(UiElement.Y.GetValueOrDefault() * _window.Height),
                w = (int)(UiElement.W.GetValueOrDefault() * _window.Width),
                h = (int)(UiElement.H.GetValueOrDefault() * _window.Height);
            UiElement.Image = _window.GrabScreen(x, y, w, h).ToByteArray();
            Image = ByteToImage(UiElement.Image);
        }

        private async void PickArea()
        {
            await PickAreaAsync();
        }

        private async Task PickAreaAsync()
        {
            var vec = await _areaPicker.PickArea();
            UiElement.X = vec.Position.X;
            UiElement.Y = vec.Position.Y;
            UiElement.W = vec.Size.X;
            UiElement.H = vec.Size.Y;
            RaisePropertyChanged(nameof(UiElement));
        }

        public UiElement UiElement { get; set; }

        public bool PreviewText { get; set; }

        public string Id
        {
            get { return _id; }
            set
            {
                var oldId = _id;
                _id = value;

                if (oldId != null)
                {
                    _repository.Remove(oldId);
                    UiElement.Id = value;
                    _repository.Add(UiElement);
                }

                RaisePropertyChanged();
            }
        }

        public bool HasImage
        {
            get { return _hasImage; }
            set 
            {
                if(_hasImage == value)
                {
                    return;
                }

                _hasImage = value;
                if(!value)
                {
                    UiElement.Image = null;
                    Image = null;
                }
                RaisePropertyChanged();
            }
        }

        public ImageSource Image
        {
            get { return _image; }
            set 
            {
                _image = value;
                RaisePropertyChanged();
                HasImage = value != null;
            }
        }

        public ICommand PickAreaCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand GetTextCommand { get; }
    }
}
