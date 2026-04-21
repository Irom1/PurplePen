# Running PurplePen on macOS

PurplePen supports macOS via the **Avalonia** cross-platform app (`src/AvPurplePen`).
The legacy WinForms app (`src/PurplePen`) is Windows-only and is **not** supported on macOS.

---

## Prerequisites

| Tool | Minimum version | Notes |
|------|----------------|-------|
| .NET SDK | 10.0 | `dotnet --version` |
| Xcode Command Line Tools | current | `xcode-select --install` |
| Hardware | Apple Silicon (arm64) or Intel (x64) | Both architectures supported |

Install the .NET 10 SDK from <https://dotnet.microsoft.com/download/dotnet/10.0>.

---

## Building and running from source

All commands are run from the **`src/`** directory.

```bash
cd src

# Debug build
dotnet build AvPurplePen/AvPurplePen.csproj

# Run directly
dotnet run --project AvPurplePen/AvPurplePen.csproj
```

> **Note:** The first build also compiles the companion `PdfConverter` tool (used to
> rasterise PDF map backgrounds). Both binaries are copied to the same output directory
> automatically.

---

## Publishing a self-contained app

### Apple Silicon (arm64)

```bash
dotnet publish AvPurplePen/AvPurplePen.csproj \
    -r osx-arm64 \
    --self-contained true \
    -c Release \
    -o publish/osx-arm64
```

### Intel (x64)

```bash
dotnet publish AvPurplePen/AvPurplePen.csproj \
    -r osx-x64 \
    --self-contained true \
    -c Release \
    -o publish/osx-x64
```

The publish output is a self-contained directory.  The main executable is
`AvPurplePen` (no `.exe` extension).  The `PdfConverter` helper tool is
published alongside it with the matching native libraries for PDFiumCore.

### Running the published build

```bash
chmod +x publish/osx-arm64/AvPurplePen   # ensure executable bit is set
./publish/osx-arm64/AvPurplePen
```

---

## What is included in the macOS scope

| Feature | Status | Notes |
|---------|--------|-------|
| Open / save course files | ✅ Supported | |
| OCAD / OpenMapper map rendering | ✅ Supported | Skia-based rendering |
| PDF export (course PDFs) | ✅ Supported | PdfSharp |
| Bitmap export | ✅ Supported | SkiaSharp |
| PDF map background import | ✅ Supported | PdfConverter companion tool |
| Printing | 🚧 Partial | Print-to-PDF works; direct printer support in progress |
| Avalonia UI (all dialogs) | 🚧 In progress | Dialogs are being ported from WinForms |

---

## Excluded from the macOS build

The following projects are **Windows-only** and are not built on macOS:

- `PurplePen/PurplePen.csproj` — legacy WinForms app (`net48` / `net10.0-windows`)
- `PurplePen_Tests/PurplePen_Tests.csproj` — WinForms test suite
- `MapModel/Map_GDIPlus/` — GDI+ rendering backend
- `MapModel/Map_WPF/`, `MapModel/Map_D2D/` — WPF / Direct2D backends
- `Innosetup/`, `MsftStoreInstaller/` — Windows installer projects
- `CrashReporter.NET/` — Windows crash reporter

When working on macOS, build only the cross-platform projects:

```bash
dotnet build AvPurplePen/AvPurplePen.csproj
dotnet test PurplePenViewModels.Tests/PurplePenViewModels.Tests.csproj
dotnet test MapModel/Map_Skia.Tests/Map_Skia.Tests.csproj -f net10.0
dotnet test MapModel/Graphics2D.Tests/Graphics2D.Tests.csproj -f net10.0
dotnet test MapModel/Map_PDF.Tests/Map_PDF.Tests.csproj -f net10.0
```

---

## Troubleshooting

### `PdfConverter not found`

The `PdfConverter` helper tool must be present in the same directory as the
`AvPurplePen` binary.  For development builds it is copied automatically during
the build step.  For published builds it is published alongside the main app.

If you see the error when running `dotnet run`, try a full clean build:

```bash
dotnet build AvPurplePen/AvPurplePen.csproj --no-incremental
```

### Missing native libraries (SkiaSharp / PDFiumCore)

For framework-dependent builds, the native libraries live in a `runtimes/` subdirectory
alongside the application.  Ensure the full output directory is present; do not copy just
the main executable.

For self-contained publishes the correct native libraries are bundled automatically when
`-r osx-arm64` or `-r osx-x64` is specified.

### macOS Gatekeeper / quarantine

If macOS prevents launching a freshly built binary, remove the quarantine attribute:

```bash
xattr -dr com.apple.quarantine ./publish/osx-arm64
```

For distribution to end users, the application should be signed and notarized.
Code signing and notarization are not yet part of the build pipeline and are deferred
to a future milestone.

### Fonts

PurplePen bundles the Roboto typeface in the application output.  Avalonia finds these
fonts automatically without any system installation required.

---

## Architecture notes

- **Rendering:** All map and course drawing goes through `IGraphicsTarget` → `Map_SkiaStd`
  (SkiaSharp).  The GDI+ backend is not used in the Avalonia app.
- **Colors:** Internal representation is CMYK throughout; each rendering backend converts
  to the platform colour model as needed.
- **PDF export:** Uses PDFsharp (managed, cross-platform).
- **PDF map import:** A separate `PdfConverter` process (using PDFiumCore) rasterises PDF
  maps to PNG.  PDFiumCore ships platform-specific native libraries via NuGet.
- **Printing:** The Avalonia printing path is in progress; PDF export is a reliable
  alternative for distributing course maps on macOS.

---

## Running cross-platform tests

```bash
cd src

# ViewModel unit tests (pure .NET, no UI)
dotnet test PurplePenViewModels.Tests/PurplePenViewModels.Tests.csproj

# Skia rendering tests
dotnet test MapModel/Map_Skia.Tests/Map_Skia.Tests.csproj -f net10.0

# Graphics2D abstraction tests
dotnet test MapModel/Graphics2D.Tests/Graphics2D.Tests.csproj -f net10.0

# PDF output tests
dotnet test MapModel/Map_PDF.Tests/Map_PDF.Tests.csproj -f net10.0

# All cross-platform tests (run headless)
TEST_SILENTRUN=True dotnet test PurplePenViewModels.Tests/PurplePenViewModels.Tests.csproj
```

Skip Windows-only test projects (`PurplePen_Tests`, `PdfConverter.Tests`,
`DotSpatial.Projections.Tests`) when running on macOS — they target `net10.0-windows`
and will not compile.
