using autoplaysharp.Contracts;
using autoplaysharp.Game.UI;
using autoplaysharp.Helper;
using Prism.Commands;
using Prism.Mvvm;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace autoplaysharp.App.UI.Repository
{
    internal class UiElementViewModel : BindableBase
    {
        private readonly IUiSubRepository _repository;
        private string _id;
        private readonly IAreaPicker _areaPicker;
        private readonly IEmulatorWindow _window;
        private ImageSource _image;
        private bool _hasImage;

        public UiElementViewModel(IUiSubRepository repository, UIElement element, IAreaPicker areaPicker, IEmulatorWindow window)
        {
            _repository = repository;
            UIElement = element;
            Id = element.Id;
            PickAreaCommand = new DelegateCommand(PickArea);
            PickImageCommand = new DelegateCommand(PickImage);
            _areaPicker = areaPicker;
            _window = window;
            Image = UIElement.Image == null ? null : ByteToImage(UIElement.Image);
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
            int x = (int)(UIElement.X * _window.Width), 
                y = (int)(UIElement.Y * _window.Height), 
                w = (int)(UIElement.W * _window.Width), 
                h = (int)(UIElement.H * _window.Height);
            UIElement.Image = _window.GrabScreen(x, y, w, h).ToByteArray();
            Image = ByteToImage(UIElement.Image);
        }

        private async void PickArea()
        {
            await PickAreaAsync();
        }

        private async Task PickAreaAsync()
        {
            var vec = await _areaPicker.PickArea();
            UIElement.X = vec.Position.X;
            UIElement.Y = vec.Position.Y;
            UIElement.W = vec.Size.X;
            UIElement.H = vec.Size.Y;
            RaisePropertyChanged(nameof(UIElement));
        }

        public UIElement UIElement { get; set; }

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
                    UIElement.Id = value;
                    _repository.Add(UIElement);
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
                    UIElement.Image = null;
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
    }
}
