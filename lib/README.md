# Third-Party Dependencies (Forks)

This folder contains pre-compiled binaries for external forks required by `BanchoMultiplayerBot`. They are committed here to make the repository fully self-contained and portable upon cloning.

## 1. BanchoSharp
- **Original Author:** [hburn7 (Stage)](https://github.com/hburn7/BanchoSharp)
- **Fork Repository:** [matte-ek/BanchoSharp](https://github.com/matte-ek/BanchoSharp)
- **Branch:** `bancho-multiplayer-bot`
- **Commit:** `0239aaf` (or latest Release net8.0 build)
- **Files:** `BanchoSharp.dll`, `BanchoSharp.pdb`
- **Purpose:** Bancho IRC client library with custom patches (TcpClient exposure, long player name checks, WinCondition enum order fix).

## 2. osu.NET
- **Original Author:** [minisbett](https://github.com/minisbett/osu.NET)
- **Fork Repository:** [matte-ek/osu.NET](https://github.com/matte-ek/osu.NET)
- **Branch:** `master`
- **Target:** `net9.0`
- **Files:** `osu.NET.dll`, `osu.NET.pdb`, `osu.NET.xml`
- **Purpose:** osu! API v2 wrapper with fixes for .NET 9.0 compatibility.

