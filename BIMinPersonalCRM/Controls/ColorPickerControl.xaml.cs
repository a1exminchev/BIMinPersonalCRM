using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BIMinPersonalCRM.Controls
{
    public partial class ColorPickerControl : UserControl
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Brush), typeof(ColorPickerControl),
                new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

        public static readonly DependencyProperty SelectedColorTextProperty =
            DependencyProperty.Register(nameof(SelectedColorText), typeof(string), typeof(ColorPickerControl),
                new FrameworkPropertyMetadata("#FFFFFF", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorTextChanged));

        public Brush SelectedColor
        {
            get => (Brush)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public string SelectedColorText
        {
            get => (string)GetValue(SelectedColorTextProperty);
            set => SetValue(SelectedColorTextProperty, value);
        }

        public ColorPickerControl()
        {
            InitializeComponent();
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPickerControl control && e.NewValue is SolidColorBrush brush)
            {
                control.SelectedColorText = brush.Color.ToString();
            }
        }

        private static void OnSelectedColorTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPickerControl control && e.NewValue is string colorString && !string.IsNullOrWhiteSpace(colorString))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorString);
                    var newBrush = new SolidColorBrush(color);
                    // Avoid infinite loop: only set if different
                    if (control.SelectedColor is SolidColorBrush existing)
                    {
                        if (existing.Color != color)
                        {
                            control.SelectedColor = newBrush;
                        }
                    }
                    else
                    {
                        control.SelectedColor = newBrush;
                    }
                }
                catch
                {
                    // Ignore invalid color strings
                }
            }
        }

        private void SelectColor_Click(object sender, RoutedEventArgs e)
        {
            ColorPopup.IsOpen = true;
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string colorString)
            {
                var color = (Color)ColorConverter.ConvertFromString(colorString);
                SelectedColor = new SolidColorBrush(color);
                SelectedColorText = colorString;
                ColorPopup.IsOpen = false;
            }
        }
    }
}
