# Purple Pen

**Purple Pen** is a desktop course-setting program for orienteering races. It lets you design courses on a map, manage control points and course variations, produce control descriptions, and export print-ready materials.

> **Status:** v4.0.0.110 Alpha 1 — the cross-platform Avalonia port is under active development.

---

## Features

- **Map support** — OCAD (versions 6–12), OpenMapper (.omap), georeferenced PDF backgrounds, and bitmap templates
- **Course design** — normal, score, and relay courses; map exchanges; course variations
- **Control descriptions** — multi-language IOF symbol descriptions; print-ready layouts
- **Export** — course PDFs, bitmap exports (PNG/JPEG), purple-pen XML course files (.ppen)
- **Multi-language UI** — live language switching (English, French, German, Slovak, and more)
- **Cross-platform rendering** — SkiaSharp backend for macOS/Linux; GDI+ for Windows

---

## Platform Support

| Platform | Application | Status |
|----------|-------------|--------|
| macOS (Apple Silicon & Intel) | Avalonia (`src/AvPurplePen`) | Active development |
| Windows | Avalonia (`src/AvPurplePen`) | Active development |
| Windows | WinForms (`src/PurplePen`) | Legacy; feature-complete but not receiving new UI work |

---

## Getting Started

### macOS

See [MACOS.md](MACOS.md) for full prerequisites, build, publish, and troubleshooting instructions.

Quick start:

```bash
# Prerequisites: .NET 10 SDK, Xcode Command Line Tools
cd src
dotnet run --project AvPurplePen/AvPurplePen.csproj
```

### Windows

```bash
cd src

# Run the Avalonia app
dotnet run --project AvPurplePen/AvPurplePen.csproj

# Or build the legacy WinForms app (requires Visual Studio 2022)
msbuild PurplePen/PurplePen.csproj /p:Configuration=Release
```

---

## Running Tests

```bash
cd src

# ViewModel unit tests (cross-platform, no UI)
dotnet test PurplePenViewModels.Tests/PurplePenViewModels.Tests.csproj

# Skia rendering tests
dotnet test MapModel/Map_Skia.Tests/Map_Skia.Tests.csproj -f net10.0

# Graphics2D abstraction tests
dotnet test MapModel/Graphics2D.Tests/Graphics2D.Tests.csproj -f net10.0

# PDF output tests
dotnet test MapModel/Map_PDF.Tests/Map_PDF.Tests.csproj -f net10.0

# WinForms integration tests (Windows only)
dotnet test PurplePen_Tests/PurplePen_Tests.csproj
```

---

## Architecture

Purple Pen uses a layered MVC architecture:

- **Controller** (`src/PurplePen/Controller.cs`) — central command coordinator; all UI operations flow through here
- **EventDB** (`src/PurplePen/EventDB.cs`) — course and event data with automatic undo/redo via `UndoMgr`
- **CourseView** (`src/PurplePen/CourseView.cs`) — immutable course snapshots for rendering
- **IGraphicsTarget** (`src/MapModel/Graphics2D-Shared/IGraphicsTarget.cs`) — rendering abstraction; the same drawing commands target GDI+, SkiaSharp, WPF, PDF, Direct2D, and iOS backends
- **CMYK colors throughout** — all colors use CMYK internally; backends convert to the platform color model as needed

---

## Contributing

1. Fork the repository and create a branch: `git checkout -b issue-N-short-description`
2. Make your changes following the coding conventions in [AGENTS.md](AGENTS.md)
3. Run the relevant tests (see above)
4. Open a pull request against `main`

For larger changes, open an issue first to discuss the approach.

---

## License

Copyright © 2006-2007, Peter Golde. See [LICENSE](LICENSE) for the full BSD 3-Clause license text.
