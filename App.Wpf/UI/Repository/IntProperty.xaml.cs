using System.Windows;
using System.Windows.Controls;

namespace autoplaysharp.App.UI.Repository
{
    /// <summary>
    /// Interaction logic for FloatProperty.xaml
    /// </summary>
    public partial class IntProperty : UserControl
    {
        public IntProperty()
        {
            InitializeComponent();
        }


        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(IntProperty), new PropertyMetadata(""));

        public int? Value
        {
            get { return (int?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int?), typeof(IntProperty),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public bool HasValue
        {
            get { return (bool)GetValue(HasValueProperty); }
            set { SetValue(HasValueProperty, value); }
        }

        public static readonly DependencyProperty HasValueProperty =
            DependencyProperty.Register("HasValue", typeof(bool), typeof(IntProperty), new PropertyMetadata(false, OnHasValueChanged));

        public int DefaultValue
        {
            get { return (int)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(int), typeof(IntProperty), new PropertyMetadata(0));


        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(int), typeof(IntProperty), new PropertyMetadata(0));


        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(IntProperty), new PropertyMetadata(255));

            
        public static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (IntProperty)d;
            obj.SetValue(HasValueProperty, e.NewValue != null);
        }

        public static void OnHasValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (IntProperty)d;
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
