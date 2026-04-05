# NexusStrap

Bootstrapper and launcher for Roblox on Windows.

A modern, modular Roblox launcher (WPF + .NET 8), inspired by tools like Bloxstrap. It provides a dashboard, performance options, FastFlags, mods, plugins, macros, and more—all at the launcher/config level (no injection or anti-cheat bypass).

**Repository:** [github.com/PhantomBum/NexusStrap](https://github.com/PhantomBum/NexusStrap)

## Requirements

- **Windows 10** (build 19041+) or **Windows 11**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or newer SDK that can target `net8.0-windows`)
- **Visual Studio 2022** (optional, with “.NET desktop development” workload) or **VS Code** + C# extension

## Clone

```powershell
git clone https://github.com/PhantomBum/NexusStrap.git
cd NexusStrap
```

## Run (from source)

```powershell
dotnet restore NexusStrap.slnx
dotnet run --project src\NexusStrap\NexusStrap.csproj
```

Or open `NexusStrap.slnx` in Visual Studio, set **NexusStrap** as the startup project, and press **F5**.

## Build

```powershell
dotnet build NexusStrap.slnx -c Release
```

Output: `src\NexusStrap\bin\Release\net8.0-windows10.0.19041\NexusStrap.exe`

## Releases (pre-built Windows installer)

Official builds are on [GitHub Releases](https://github.com/PhantomBum/NexusStrap/releases).

**Download `NexusStrapSetup.exe`** — a **single file** (self-contained .NET 8). No zip, no folder: double-click to run. No separate .NET runtime install.

**Build the setup EXE locally:**

```powershell
.\scripts\build-setup.ps1
```

Output: `artifacts\NexusStrapSetup.exe`

**Publish a new GitHub release** (maintainers): push a version tag — Actions builds and attaches `NexusStrapSetup.exe` automatically:

```powershell
git tag v1.0.1
git push origin v1.0.1
```

**Manual upload with GitHub CLI:** after `gh auth login`, run `.\scripts\publish-release.ps1 -Version v1.0.1`.

## Solution layout

| Project | Role |
|--------|------|
| `src/NexusStrap` | WPF app (UI, bootstrapper, features) |
| `src/NexusStrap.PluginSDK` | Plugin contracts (reference from plugins) |
| `src/NexusStrap.PluginHost` | Loads plugins with `AssemblyLoadContext` |

## Data & logs

Settings and logs are stored under:

`%LocalAppData%\NexusStrap\`

## License

Add a `LICENSE` file if you want a specific license (e.g. MIT).

## Disclaimer

NexusStrap is not affiliated with Roblox Corporation. Use at your own risk; follow Roblox’s Terms of Use and Community Standards.
