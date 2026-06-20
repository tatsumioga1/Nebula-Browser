using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

namespace Nebula.Controls
{
    public sealed partial class DockButton : UserControl
    {
        public DockButton()
        {
            InitializeComponent();
        }

        public event TappedEventHandler Click;

        public string Glyph
        {
            get => (string)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.Register(
                nameof(Glyph),
                typeof(string),
                typeof(DockButton),
                new PropertyMetadata("", OnGlyphChanged));

        private static void OnGlyphChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is DockButton button)
            {
                button.Icon.Glyph =
                    e.NewValue?.ToString() ?? "";
            }
        }

        private void AnimateScale(double scale)
        {
            Storyboard storyboard = new();

            DoubleAnimation scaleX = new()
            {
                To = scale,
                Duration = new Duration(
                    System.TimeSpan.FromMilliseconds(120))
            };

            DoubleAnimation scaleY = new()
            {
                To = scale,
                Duration = new Duration(
                    System.TimeSpan.FromMilliseconds(120))
            };

            Storyboard.SetTarget(scaleX, ScaleTransform);
            Storyboard.SetTarget(scaleY, ScaleTransform);

            Storyboard.SetTargetProperty(
                scaleX,
                "ScaleX");

            Storyboard.SetTargetProperty(
                scaleY,
                "ScaleY");

            storyboard.Children.Add(scaleX);
            storyboard.Children.Add(scaleY);

            storyboard.Begin();
        }

        private void ButtonSurface_PointerEntered(
            object sender,
            PointerRoutedEventArgs e)
        {
            AnimateScale(1.12);

            ButtonSurface.Background =
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(
                        45, 255, 255, 255));
        }

        private void ButtonSurface_PointerExited(
            object sender,
            PointerRoutedEventArgs e)
        {
            AnimateScale(1);

            ButtonSurface.Background =
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(
                        18, 255, 255, 255));
        }

        private void ButtonSurface_PointerPressed(
            object sender,
            PointerRoutedEventArgs e)
        {
            AnimateScale(0.95);
        }

        private void ButtonSurface_PointerReleased(
            object sender,
            PointerRoutedEventArgs e)
        {
            AnimateScale(1.12);
        }

        private void ButtonSurface_Tapped(
            object sender,
            TappedRoutedEventArgs e)
        {
            Click?.Invoke(sender, e);
        }
    }
}