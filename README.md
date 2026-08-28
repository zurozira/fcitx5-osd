# fcitx5-osd
Animated on-screen display for fcitx5 input method switches: a minimal C++ bridge addon that relays IM-switch events over a Unix socket to a C#/Avalonia animated overlay app.

Current issues:

The Avalonia popup window itself is not reliably visible on screen.

Symptoms: the window fades in briefly, then disappears during the "hold"
period, then flickers back into view right as the fade-out animation starts,
then disappears for good on Close().
