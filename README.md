# fcitx5-osd

An animated on-screen display for fcitx6 input method switches. Basically: you hit your IM toggle key, and instead of guessing whether it actually switched, a little overlay pops up and tells you.

As someone who's constantly switching input methods on my keyboard, this felt like a non-negotiable to have, but fcitx6's default OSD looks pretty outdated next to the rest of a modern desktop UI.

## Current issues

Not gonna pretend this is polished ~ the popup window itself is being stubborn about actually staying visible.

Here's the exact broken behavior I'm chasing right now: the window fades in like it's supposed to, then just disappears during what should be the "hold" period where it's meant to sit steady on screen. Then, right as the fade-out animation is supposed to start, it flickers back into view for a split second, before vanishing for real once `Close()` gets called.

## How it works

- fcitx5 addon (C++) detects an input method switch and writes the event to a Unix socket
- The Avalonia app (C#) listens on that socket, receives the event, and triggers the overlay animation
- Overlay is meant to fade in, hold briefly so you can actually read it, then fade out — when it's working, that is

## Install

Heads up: given the current visibility bug above, expect the overlay to be flaky right now. The socket bridge itself works fine.

### Prerequisites

- fcitx5 installed and running
- CMake + a C++ compiler (build the addon)
- [.NET SDK](https://dotnet.microsoft.com/download) (build the Avalonia app)

### Build the C++ addon

```bash
git clone https://github.com/zurozira/fcitx5-osd.git
cd fcitx5-osd/addon
cmake -B build
cmake --build build
sudo cmake --install build
```

This drops the addon into fcitx5's addon directory so it starts relaying IM-switch events over the Unix socket automatically.

### Build the Avalonia overlay app

```bash
cd ../overlay
dotnet publish -c Release -r linux-x64 --self-contained
```

Grab the published binary from `bin/Release/net.../linux-x64/publish/` and run it — it'll connect to the same socket the addon writes to.

### Run it

Start the overlay app first, then restart fcitx5 (or just trigger an IM switch) to confirm events are coming through:

```bash
./fcitx5-osd-overlay
```

If the addon and overlay are both running, switching input methods should trigger the popup.

## what now

Well I've tried to fix the bug for a while now but still no luck T.T Maybe it's a KDE thing (my current setup)
