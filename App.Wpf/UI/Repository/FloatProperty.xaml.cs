using System.Windows;

namespace autoplaysharp.App.UI.Repository
{
    /// <summary>
    /// Interaction logic for FloatProperty.xaml
    /// </summary>
    public partial class FloatProperty
    {
        public FloatProperty()
        {
            InitializeComponent();
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(FloatProperty), new PropertyMetadata(""));

        public float? Value
        {
            get { return (float?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(float?), typeof(FloatProperty),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public bool HasValue
        {
            get { return (bool)GetValue(HasValueProperty); }
            set { SetValue(HasValueProperty, value); }
        }

        public static readonly DependencyProperty HasValueProperty =
            DependencyProperty.Register("HasValue", typeof(bool), typeof(FloatProperty), new PropertyMetadata(false, OnHasValueChanged));

        public float DefaultValue
        {
            get { return (float)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(float), typeof(FloatProperty), new PropertyMetadata(1f));


        public float MinValue
        {
            get { return (float)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(float), typeof(FloatProperty), new PropertyMetadata(0f));


        public float MaxValue
        {
            get { return (float)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(float), typeof(FloatProperty), new PropertyMetadata(1f));

            
        public static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (FloatProperty)d;
            obj.SetValue(HasValueProperty, e.NewValue != null);
        }

        public static void OnHasValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (FloatProperty)d;
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
