using System.Windows;
using System.Windows.Controls;

namespace autoplaysharp.App.UI.Repository
{
    /// <summary>
    /// Interaction logic for FloatProperty.xaml
    /// </summary>
    public partial class TextProperty : UserControl
    {
        public TextProperty()
        {
            InitializeComponent();
        }


        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(TextProperty), new PropertyMetadata(""));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(TextProperty),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public bool HasValue
        {
            get { return (bool)GetValue(HasValueProperty); }
            set { SetValue(HasValueProperty, value); }
        }

        public static readonly DependencyProperty HasValueProperty =
            DependencyProperty.Register("HasValue", typeof(bool), typeof(TextProperty), new PropertyMetadata(false, OnHasValueChanged));

        public string DefaultValue
        {
            get { return (string)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(string), typeof(TextProperty), new PropertyMetadata("ID_1"));

        public static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (TextProperty)d;
            obj.SetValue(HasValueProperty, e.NewValue != null);
        }

        public static void OnHasValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (TextProperty)d;
            if((bool)e.NewValue && obj.GetValue(ValueProperty) == null)
            {
                obj.SetValue(ValueProperty, obj.GetValue(DefaultValueProperty));
            }
            else if(!(bool)e.NewValue)
            {
                obj.SetValue(ValueProperty, null);
            }
        }
    }
}
