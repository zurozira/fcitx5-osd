using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using Avalonia.Threading;

namespace osd_app;

public sealed class SocketListener
{
    private readonly string _socketPath;

    public SocketListener()
    {
        _socketPath = Path.Combine(
                Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") ?? "/tmp",
                "fcitx5-osd.sock"); // real socket, not test one
    }

    public void Start()
    {
        var thread = new Thread(Listen) { IsBackground = true, Name = "fcitx5-osd-listener" };
        thread.Start();
    }

    private void Listen()
    {
        if (File.Exists(_socketPath))
        {
            File.Delete(_socketPath);
        }

        using var socket = new Socket(AddressFamily.Unix, SocketType.Dgram, ProtocolType.Unspecified);
        socket.Bind(new UnixDomainSocketEndPoint(_socketPath));

        Console.WriteLine($"Listening on {_socketPath}");
        var buffer = new byte[4096];
        while (true)
        {
            var received = socket.Receive(buffer);
            var text = Encoding.UTF8.GetString(buffer, 0, received);

            foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<ImSwitchPayload>(line);
                    //Console.WriteLine($"Parsed: {payload?.nativeName}({payload?.uniqueName})");

                    if (payload is not null)
                    {
                        Dispatcher.UIThread.Post(() => OsdWindow.ShowFor(payload));
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Bad JSON: {ex.Message}");
                }
            }
        }
    }
}
