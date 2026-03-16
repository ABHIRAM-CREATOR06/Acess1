<div align="center">

<br/>

```
 █████╗ ██╗  ██╗██╗███████╗
██╔══██╗╚██╗██╔╝██║██╔════╝
███████║ ╚███╔╝ ██║███████╗
██╔══██║ ██╔██╗ ██║╚════██║
██║  ██║██╔╝ ██╗██║███████║
╚═╝  ╚═╝╚═╝  ╚═╝╚═╝╚══════╝
```

### Web Accessibility Checker

**Automated WCAG 2.1 compliance auditing for Windows — with real-time scoring, headless JS rendering, and environmental impact analysis.**

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D4?style=flat-square&logo=windows)](https://microsoft.com/windows)
[![License](https://img.shields.io/badge/License-GPL--3.0-22C55E?style=flat-square)](LICENSE)
[![WCAG](https://img.shields.io/badge/WCAG-2.1-F59E0B?style=flat-square)](https://www.w3.org/WAI/WCAG21/quickref/)

<br/>

</div>

---

## What is AXIS?

AXIS is a desktop application that audits websites and local HTML files against **WCAG 2.1** standards. It renders JavaScript-heavy pages using a headless Chrome engine, computes an accessibility score from 0–100, flags issues by severity, and exports professional reports in TXT or PDF.

It also ships an **SDK** — `AXIS-CORE` — so teams can embed the same checks into their CI/CD pipelines or developer tooling.

---

## Features

| Category | What's included |
|---|---|
| **Auditing** | Alt text, form labels, heading hierarchy, color contrast, font readability |
| **Rendering** | PuppeteerSharp headless Chrome for JS-rendered content |
| **Scoring** | 0–100 accessibility score + compliance tier |
| **Environmental** | CO₂ emissions estimate, CDN detection, energy rating |
| **Export** | TXT + PDF reports with recommendations |
| **Standards** | WCAG 2.1, Section 508, India RPwD Act |

---

## Compliance Tiers

```
95 – 100  ████████████  Fully Compliant
80 –  94  ██████████░░  Mostly Compliant
60 –  79  ███████░░░░░  Partially Compliant
 0 –  59  ████░░░░░░░░  Not Compliant
```

---

## Getting Started

### Option 1 — Standalone `.exe` (recommended)

Download `WebAccessibilityChecker.exe` from Releases and double-click. No installation needed.

### Option 2 — Run from source

```bash
git clone <repository-url>
cd WebAccessibilityChecker
dotnet run
```

### Option 3 — Build self-contained binary

```bash
dotnet publish -c Release -r win-x64 --self-contained
# Output: bin/Release/net9.0-windows/win-x64/publish/WebAccessibilityChecker.exe
```

> **Requirements:** Windows 10/11 · .NET 9.0 Runtime · Internet connection for URL analysis

---

## Usage

```
1. Launch   →  Run the .exe or use dotnet run
2. Input    →  Paste a URL  or  browse to a local HTML file
3. Analyze  →  Click Analyze — JS is rendered automatically
4. Review   →  Severity-coded table: Red (errors) · Orange (warnings) · Green (info)
5. Export   →  Save as TXT or PDF
```

---

## Environmental Impact

AXIS estimates your page's carbon footprint as part of every audit.

| Rating | CO₂ per page load |
|---|---|
| 🌱 Eco | < 10 g |
| 🟡 Moderate | 10 – 50 g |
| 🔴 High Impact | > 50 g |

Metrics include page weight, request count, CDN usage, and estimated server energy draw. Environmental data is included in TXT/PDF exports.

---

## Architecture

```
WebAccessibilityChecker/
├── Models/             # Issue, Report data models
├── Services/           # Parser, Checker — core audit logic
├── Utils/              # Export helpers (TXT, PDF)
├── Resources/          # App resources
├── MainWindow.xaml     # WPF UI layout
└── MainWindow.xaml.cs  # UI logic
```

**Stack:** WPF · .NET 9 · C# · HtmlAgilityPack · PuppeteerSharp · QuestPDF

---

## AXIS-CORE SDK

Embed the same audit engine into your own tools. Available for .NET, JavaScript, Rust, and Python (planned).

### .NET

```bash
dotnet add package AXIS-CORE
```

```csharp
var checker = new AxisCore();
var report = await checker.CheckUrlAsync("https://example.com");
Console.WriteLine($"Score: {report.AccessibilityScore}/100");
```

### JavaScript / Node.js

```bash
npm install axis-core-sdk
```

```js
const { AxisCore } = require('axis-core');
const report = await new AxisCore().checkUrl('https://example.com');
console.log(`Score: ${report.accessibilityScore}/100`);
```

### Rust

```toml
# Cargo.toml
[dependencies]
axis-core = "1.0"
```

### Packages

| Runtime | Registry | Status |
|---|---|---|
| .NET | [NuGet — AXIS-CORE](https://www.nuget.org/packages/AXIS-CORE/) | Published |
| JavaScript | [npm — axis-core-sdk](https://www.npmjs.com/package/axis-core-sdk) | Published  |
| Rust | [crates.io — axis-core](https://crates.io/crates/axis-core) | Published  |
| Python | [PyPI — axis-core](https://pypi.org/project/axis-core/) | Planned |

---

## Troubleshooting

**Headless browser failed on first run**
Chrome downloads automatically on first launch. The app falls back to HTTP fetching in the meantime — most sites work fine with this fallback.

**Application won't start**
Ensure .NET 9.0 Runtime is installed. Try running as administrator. Check Windows Firewall isn't blocking the process.

**Memory usage**
~200 MB with Chrome active · ~50 MB with HTTP fallback

---

## Standards Covered

- **WCAG 2.1** — Perceivable, Operable, Understandable, Robust
- **Section 508** — US federal accessibility requirements
- **India RPwD Act** — Rights of Persons with Disabilities

---

## Contributing

```bash
# 1. Fork the repo
# 2. Create a feature branch
git checkout -b feature/your-feature

# 3. Install SDK and restore packages
dotnet restore

# 4. Run in dev mode
dotnet run

# 5. Open a pull request
```

---

## Related

> **[A11Y: Lazy Edition](https://github.com/ABHIRAM-CREATOR06/a11y-check)** — Check accessibility directly inside VS Code while you develop.

---

<div align="center">

**GPL-3.0-or-later** · Made with ❤️ for a more accessible web

</div>
