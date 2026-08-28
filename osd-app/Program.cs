using Avalonia;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace osd_app;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        new SocketListener().Start();

        // TestSocketListener(); // temporary — proves the socket read works before wiring Avalonia in

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void TestSocketListener()
    {
        var socketPath = Path.Combine(
            Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") ?? "/tmp",
            "fcitx5-osd-test.sock");

        if (File.Exists(socketPath))
        {
            File.Delete(socketPath);
        }

        using var socket = new Socket(AddressFamily.Unix, SocketType.Dgram, ProtocolType.Unspecified);
        socket.Bind(new UnixDomainSocketEndPoint(socketPath));

        Console.WriteLine($"Listening on {socketPath}");
        var buffer = new byte[4096];
        while (true)
        {
            var received = socket.Receive(buffer);
            Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, received));
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
