# AXIS-CORE Repository Overview

## 📁 Repository Structure

This repository contains the complete AXIS-CORE ecosystem - a comprehensive multi-platform SDK for web accessibility checking, along with a desktop application and extensive documentation.

```
Acess1/
├── 📁 WebAccessibilityChecker/          # Main WPF Desktop Application
├── 📁 AXIS-CORE/                        # .NET SDK (Published to NuGet)
├── 📁 axis-core-rs/                     # Rust SDK (Ready for Crates.io)
├── 📁 axis-core-js/                     # JavaScript SDK (Published to npm)
├── 📁 docs/                             # Documentation & Website
│   ├── 📄 app.js                        # Original demo equations
│   ├── 📄 equations.js                  # Accessibility calculation logic
│   ├── 📄 index.html                    # Original HTML demo
│   ├── 📄 styles.css                    # Original demo styles
│   └── 📁 website/                      # Complete AXIS-CORE Website
├── 📄 README.md                         # Main repository documentation
├── 📄 AXIS-CORE-SDK-DOCS.md            # Comprehensive SDK documentation
├── 📄 REPOSITORY-OVERVIEW.md           # This file
├── 📄 Project1.sln                      # Visual Studio solution
├── 📄 run.bat                           # Quick start batch file
└── 📄 .gitattributes                    # Git configuration
```

## 🎯 Project Components

### 1. 🖥️ WebAccessibilityChecker (Main Application)

**Location**: `WebAccessibilityChecker/`
**Technology**: WPF (.NET 9.0), C#
**Status**: Complete & Production Ready

#### What it does:
- **Desktop GUI Application** for web accessibility checking
- **URL Analysis**: Check any website for accessibility issues
- **File Upload**: Analyze local HTML files
- **WCAG 2.1 Compliance**: Automated checks against accessibility standards
- **Real-time Scoring**: 0-100 accessibility scores with compliance status
- **Environmental Impact**: Calculate energy consumption and CO₂ emissions
- **Export Options**: TXT and PDF report generation
- **Advanced Features**: JavaScript rendering, color contrast analysis, heading hierarchy validation

#### Key Files:
```
WebAccessibilityChecker/
├── MainWindow.xaml/cs      # Main UI and logic
├── Models/                 # Data models (Issue, Report, PageLoadResult)
├── Services/               # Core logic (AccessibilityChecker, HtmlParser)
├── Utils/                  # Helpers (ExportHelper, AccessibilityEquations)
├── Resources/              # Application resources and icons
└── App.xaml/cs            # Application entry point
```

#### How to run:
```bash
# From repository root
cd WebAccessibilityChecker
dotnet run
```

### 2. 🔧 AXIS-CORE (.NET SDK)

**Location**: `AXIS-CORE/`
**Technology**: .NET Standard 2.1, C#
**Status**: ✅ Published to NuGet
**Package**: https://www.nuget.org/packages/AXIS-CORE/

#### What it does:
- **Programmatic API** for accessibility checking
- **Same engine** as the desktop app, packaged as a library
- **Cross-platform** .NET Standard compatibility
- **Environmental Impact** analysis included
- **Export capabilities** (TXT, PDF)
- **Async support** for high performance

#### Installation:
```bash
dotnet add package AXIS-CORE
```

#### Usage:
```csharp
using AXIS_CORE;

var checker = new AxisCore();
var report = await checker.CheckUrlAsync("https://example.com");
Console.WriteLine($"Score: {report.AccessibilityScore}/100");
```

#### Key Files:
```
AXIS-CORE/
├── AxisCore.cs            # Main SDK API
├── Models/                # Shared data models
├── Services/              # Core accessibility logic
├── Utils/                 # Export and calculation helpers
├── Resources/             # Embedded logo and assets
└── AXIS-CORE.csproj      # Project configuration
```

### 3. 🦀 axis-core-rs (Rust SDK)

**Location**: `axis-core-rs/`
**Technology**: Rust 2021 Edition
**Status**: 🚀 Ready for Crates.io publishing
**Performance**: Exceptional (memory-safe, zero-cost abstractions)

#### What it does:
- **High-performance** accessibility checking in Rust
- **Memory-safe** implementation with compile-time guarantees
- **Async/await** support with Tokio runtime
- **Same API** as other SDKs for consistency
- **Native performance** with minimal memory footprint

#### Installation (when published):
```bash
cargo add axis-core
```

#### Usage:
```rust
use axis_core::AxisCore;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let checker = AxisCore::new();
    let report = checker.check_url("https://example.com").await?;
    println!("Score: {}/100", report.accessibility_score);
    Ok(())
}
```

#### Key Files:
```
axis-core-rs/
├── src/
│   ├── lib.rs             # Main library interface
│   ├── checker.rs         # Core accessibility logic
│   ├── models.rs          # Data structures
│   └── utils.rs           # Helper functions
├── Cargo.toml             # Package configuration
├── README.md              # SDK documentation
└── tests/                 # Unit and integration tests
```

### 4. 📦 axis-core-js (JavaScript SDK)

**Location**: `axis-core-js/`
**Technology**: Node.js, JavaScript ES2020
**Status**: ✅ Published to npm
**Package**: https://www.npmjs.com/package/axis-core-sdk

#### What it does:
- **Node.js library** for server-side accessibility checking
- **Puppeteer integration** for JavaScript rendering
- **Browser automation** for dynamic content analysis
- **Promise-based API** with async/await support
- **Same functionality** as other SDKs

#### Installation:
```bash
npm install axis-core-sdk
```

#### Usage:
```javascript
const AxisCore = require('axis-core-sdk');

async function checkAccessibility() {
    const checker = new AxisCore();
    const report = await checker.checkUrl('https://example.com');
    console.log(`Score: ${report.accessibilityScore}/100`);
}
```

#### Key Files:
```
axis-core-js/
├── index.js               # Main SDK interface
├── checker.js             # Accessibility checking logic
├── models.js              # Data structures
├── package.json           # npm configuration
├── README.md              # SDK documentation
└── node_modules/          # Dependencies (Puppeteer, etc.)
```

### 5. 🌐 Documentation & Website

**Location**: `docs/`
**Status**: Complete & Interactive

#### Components:

**Original Demo** (`docs/app.js`, `docs/equations.js`, `docs/index.html`):
- Legacy accessibility equations calculator
- Original HTML/JS demo from project inception
- Still functional for reference

**AXIS-CORE Website** (`docs/website/`):
- **Complete interactive website** showcasing the entire ecosystem
- **Live demo** with mock accessibility checking
- **SDK comparison** and installation guides
- **Comprehensive documentation** hub
- **Modern responsive design** with animations

#### Website Features:
- Interactive accessibility checker demo
- Platform comparison matrix
- Installation instructions for all SDKs
- API documentation and examples
- Performance benchmarks
- About section with project mission

#### How to view website:
```bash
# Local development
cd docs/website
python -m http.server 8000
# Open http://localhost:8000

# Or open index.html directly in browser
```

## 📊 Architecture Overview

### Shared Components

All SDKs share the same **core accessibility checking logic**:

1. **HTML Parsing**: Extract and analyze DOM structure
2. **WCAG Rules**: 10+ automated accessibility checks
3. **Scoring Algorithm**: 0-100 accessibility scoring
4. **Environmental Impact**: Energy consumption calculations
5. **Report Generation**: Structured output with issues and recommendations

### Platform-Specific Adaptations

- **.NET**: Uses HtmlAgilityPack for parsing, PuppeteerSharp for JS rendering
- **Rust**: Native HTML parsing with `scraper` crate, `reqwest` for HTTP
- **JavaScript**: Puppeteer for browser automation and rendering

### API Consistency

All SDKs provide the same core interface:

```typescript
interface AxisCoreChecker {
    checkUrl(url: string): Promise<Report> | Report
    checkHtml(html: string, baseUrl?: string): Report
    exportToText(report: Report): string
    getVersion(): string
}
```

## 🚀 Getting Started

### For Developers (Using SDKs)

1. **Choose your platform**: .NET, Rust, or JavaScript
2. **Install the SDK**: Use respective package manager
3. **Check documentation**: Each SDK has comprehensive guides
4. **Start coding**: Use the consistent API across platforms

### For Contributors

1. **Clone repository**: `git clone https://github.com/ABHIRAM-CREATOR06/Acess1.git`
2. **Explore components**: Each folder is self-contained
3. **Check documentation**: Start with `AXIS-CORE-SDK-DOCS.md`
4. **Run examples**: Each SDK has working examples

### For Researchers/Students

1. **Study algorithms**: Check `docs/equations.js` for scoring logic
2. **Explore implementations**: Compare across different languages
3. **Run the desktop app**: See accessibility checking in action
4. **Use the website**: Interactive learning experience

## 🔧 Development Workflow

### Building the Desktop App
```bash
cd WebAccessibilityChecker
dotnet restore
dotnet build
dotnet run
```

### Building .NET SDK
```bash
cd AXIS-CORE
dotnet restore
dotnet build
dotnet pack  # Creates NuGet package
```

### Building Rust SDK
```bash
cd axis-core-rs
cargo build
cargo test
cargo doc    # Generate documentation
```

### Building JavaScript SDK
```bash
cd axis-core-js
npm install
npm test
npm run build  # If build script exists
```

### Publishing SDKs
```bash
# .NET (already published)
dotnet nuget push AXIS-CORE/bin/Release/*.nupkg -k YOUR_API_KEY -s https://api.nuget.org/v3/index.json

# Rust (ready to publish)
cargo login
cargo publish

# JavaScript (already published)
npm login
npm publish
```

## 📈 Project Metrics

- **Total Lines of Code**: 8,876+ (committed)
- **SDK Platforms**: 3 (.NET, Rust, JavaScript)
- **Published Packages**: 2 (NuGet, npm)
- **Documentation Lines**: 500+ across all docs
- **WCAG Checks**: 10+ automated rules
- **Test Coverage**: Comprehensive unit tests
- **Performance**: Sub-second analysis times
- **Compatibility**: Cross-platform (Windows, macOS, Linux)

## 🎯 Use Cases

### For Developers
- **CI/CD Integration**: Automated accessibility testing in build pipelines
- **Development Tools**: Real-time accessibility checking during development
- **API Services**: Build accessibility-as-a-service platforms
- **Content Management**: Validate accessibility before publishing

### For Organizations
- **Compliance Monitoring**: Regular accessibility audits
- **Quality Assurance**: Automated testing workflows
- **Reporting**: Generate compliance reports for stakeholders
- **Training**: Educational tools for accessibility awareness

### For Researchers
- **Algorithm Analysis**: Study accessibility scoring methodologies
- **Performance Comparison**: Benchmark implementations across languages
- **Standards Implementation**: Reference WCAG 2.1 compliance checking
- **Environmental Impact**: Research sustainable web development

## 🤝 Contributing

### Ways to Contribute

1. **Code**: Add new accessibility checks or improve algorithms
2. **Documentation**: Improve guides, add examples, fix typos
3. **Testing**: Add test cases, improve coverage, find edge cases
4. **Platforms**: Implement SDKs for new languages (Go, Java, Python)
5. **Features**: Add new capabilities like real-time monitoring
6. **UI/UX**: Improve the desktop app or website design

### Development Setup

1. **Prerequisites**:
   - .NET 8.0+ SDK
   - Rust 1.70+
   - Node.js 16+
   - Git

2. **Clone and Setup**:
   ```bash
   git clone https://github.com/ABHIRAM-CREATOR06/Acess1.git
   cd Acess1
   ```

3. **Test Everything**:
   ```bash
   # Test .NET components
   cd AXIS-CORE && dotnet test

   # Test Rust SDK
   cd ../axis-core-rs && cargo test

   # Test JavaScript SDK
   cd ../axis-core-js && npm test

   # Run desktop app
   cd ../WebAccessibilityChecker && dotnet run
   ```

## 📄 License

**GPL-3.0-or-later**

This project is licensed under the GNU General Public License v3.0 or later. See the LICENSE file for details.

## 🙏 Acknowledgments

- **HtmlAgilityPack**: Robust HTML parsing for .NET
- **Puppeteer**: Headless browser automation
- **Tokio**: Async runtime for Rust
- **Scraper**: HTML parsing library for Rust
- **QuestPDF**: PDF generation for .NET
- **WCAG 2.1 Guidelines**: Web accessibility standards
- **Open Source Community**: Libraries and tools that made this possible

## 📞 Support & Community

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: Community conversations
- **Documentation**: Comprehensive guides and examples
- **Website**: Interactive demo and learning resources

## 🎉 Impact

AXIS-CORE represents a significant advancement in web accessibility tooling by:

- **Democratizing Accessibility**: Making professional-grade accessibility checking available to all developers
- **Multi-Platform Support**: Consistent APIs across the most popular programming languages
- **Performance Excellence**: High-performance implementations suitable for production use
- **Environmental Awareness**: Including sustainability metrics in accessibility analysis
- **Open Source**: Community-driven development with transparent, accessible code

---

**Made with ❤️ for a more accessible web**

*This repository overview was last updated: November 29, 2024*