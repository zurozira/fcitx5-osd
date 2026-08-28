using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace osd_app;

public partial class OsdWindow : Window
{
    private static OsdWindow? _current;

    private Border? _card;
    private TextBlock? _nativeNameText;
    private TextBlock? _nameText;

    public OsdWindow()
    {
        InitializeComponent();
        _card = this.FindControl<Border>("Card");
        _nativeNameText = this.FindControl<TextBlock>("NativeNameText");
        _nameText = this.FindControl<TextBlock>("NameText");
    }


    public static void ShowFor(ImSwitchPayload payload)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ShowFor called for {payload.uniqueName}");
        _current?.Close();

        var window = new OsdWindow();
        window.Populate(payload);
        _current = window;

        var workingArea = window.Screens.Primary?.WorkingArea ?? new PixelRect(0, 0, 1920, 1080);
        window.Position = new PixelPoint(
            workingArea.X + (workingArea.Width - (int)window.Width) / 2,
            workingArea.Y + workingArea.Height - (int)window.Height - 80);

        window.Show();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] window.Show() called");
        _ = window.AnimateInThenOutAsync();
    }


    private void Populate(ImSwitchPayload payload)
    {
        var display = string.IsNullOrEmpty(payload.nativeName) ? payload.name : payload.nativeName;
        if (_nativeNameText is not null) _nativeNameText.Text = display;
        if (_nameText is not null) _nameText.Text = payload.uniqueName;
    }

    private async Task AnimateInThenOutAsync()
    {
        if (_card is null || _card.RenderTransform is not ScaleTransform scale)
        {
            return;
        }

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] fade-in starting");

        var fadeIn = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(280),
            Easing = new CubicEaseOut(),
            Children =
        {
            new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(Visual.OpacityProperty, 0d) } },
            new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(Visual.OpacityProperty, 1d) } },
        },
        };
        await fadeIn.RunAsync(_card);
        scale.ScaleX = 1;
        scale.ScaleY = 1;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] fade-in done, holding for 6s");

        await Task.Delay(3000);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] hold finished, starting fade-out");

        var fadeOut = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(400),
            Easing = new CubicEaseIn(),
            Children =
        {
            new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(Visual.OpacityProperty, 1d) } },
            new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(Visual.OpacityProperty, 0d) } },
        },
        };
        await fadeOut.RunAsync(_card);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] fade-out done, closing");
        Close();
    }
}
