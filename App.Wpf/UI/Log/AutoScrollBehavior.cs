using System.Windows;
using System.Windows.Controls;

namespace autoplaysharp.App.UI.Log
{
    internal class AutoScrollBehavior
    {

        public static bool GetEnableAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableAutoScrollProperty);
        }

        public static void SetEnableAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableAutoScrollProperty, value);
        }

        public static readonly DependencyProperty EnableAutoScrollProperty =
        DependencyProperty.RegisterAttached("EnableAutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, AutoScrollToEndPropertyChanged));

        private static void AutoScrollToEndPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textbox && e.NewValue is bool mustAutoScroll && mustAutoScroll)
            {
                textbox.TextChanged += (s, ee) => textbox.ScrollToEnd();
            }
        }
    }
}
