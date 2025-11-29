# 🚀 Web Accessibility Checker

A comprehensive Windows desktop application for checking web accessibility compliance with WCAG guidelines, featuring modern UI design and advanced JavaScript rendering capabilities.

## ✨ Features

### Core Functionality
- **URL Analysis**: Check any website for accessibility issues
- **File Upload**: Analyze local HTML files
- **WCAG Compliance**: Automated checks against WCAG 2.1 standards
- **Real-time Scoring**: Get instant accessibility scores (0-100)
- **Compliance Status**: Determine if your site is Fully/Mostly/Partially/Not Compliant

### Advanced Checks
- **Alt Text Validation**: Ensure all images have proper alt attributes
- **Form Accessibility**: Check labels and form controls
- **Heading Hierarchy**: Validate proper heading structure
- **Color Contrast**: Analyze text/background color ratios
- **Eye Comfort**: Check font sizes and line heights
- **JavaScript Content**: Render dynamic content with headless browser
- **Environmental Impact**: Calculate energy use and CO₂ emissions

### Export Options
- **TXT Reports**: Detailed text-based accessibility reports
- **PDF Reports**: Professional PDF documents with formatting
- **Comprehensive Data**: Includes scores, compliance status, and fix recommendations

## 🎯 Requirements

- **Windows 10/11**
- **.NET 9.0 Runtime** (automatically included in standalone version)
- **Internet connection** for URL analysis

## 🚀 Installation & Usage

### Option 1: Standalone Executable (Recommended)
1. Download the `WebAccessibilityChecker.exe` from the releases
2. Double-click to run (no installation required)
3. The app includes all dependencies

### Option 2: From Source
```bash
# Clone the repository
git clone <repository-url>
cd WebAccessibilityChecker

# Run the application
dotnet run
```

### Option 3: Build Standalone Version
```bash
# Build for distribution
dotnet publish -c Release -r win-x64 --self-contained

# Run from publish directory
cd bin/Release/net9.0-windows/win-x64/publish
./WebAccessibilityChecker.exe
```

## 📖 How to Use

1. **Launch the Application**
   - Run `WebAccessibilityChecker.exe` or use `dotnet run`

2. **Enter URL or Select File**
   - Type a website URL (e.g., `https://example.com`)
   - Or click "📁 Browse File" to select an HTML file

3. **Analyze Content**
   - Click "🔍 Analyze" to start checking
   - The app will render JavaScript content and analyze accessibility

4. **Review Results**
   - View issues in the table with color-coded severity
   - Check your accessibility score and compliance status
   - See detailed recommendations for each issue

5. **Export Reports**
   - Use "📄 Export to TXT" for detailed text reports
   - Use "📕 Export to PDF" for professional PDF documents

## 🎨 User Interface

### Modern Design Features
- **Card-based Layout**: Clean, professional appearance
- **Color-coded Results**: Red (Errors), Orange (Warnings), Green (Info)
- **Responsive Design**: Adapts to different window sizes
- **Emoji Icons**: Visual cues for better user experience
- **Segoe UI Font**: Modern, readable typography

### Accessibility Score System
- **95-100**: Fully Compliant
- **80-94**: Mostly Compliant
- **60-79**: Partially Compliant
- **<60**: Not Compliant

## 🔧 Technical Details

### Architecture
- **Frontend**: WPF (Windows Presentation Foundation)
- **Backend**: .NET 9.0 with C#
- **HTML Parsing**: HtmlAgilityPack
- **JavaScript Rendering**: PuppeteerSharp with headless Chrome
- **PDF Generation**: QuestPDF

### Project Structure
```
WebAccessibilityChecker/
├── Models/           # Data models (Issue, Report)
├── Services/         # Core logic (Parser, Checker)
├── Utils/            # Helper classes (Export)
├── Resources/        # Application resources
├── MainWindow.xaml   # UI layout
└── MainWindow.xaml.cs # UI logic
```

### Dependencies
- **HtmlAgilityPack**: HTML parsing and manipulation
- **PuppeteerSharp**: Headless browser for JavaScript rendering
- **QuestPDF**: PDF document generation
- **Microsoft.Extensions**: Dependency injection and logging

## 🐛 Troubleshooting

### Common Issues

**"Headless browser failed"**
- This is normal on first run - the app falls back to HTTP download
- Chrome will be downloaded automatically for future runs
- The app works perfectly with HTTP fallback

**"Cannot find Chrome"**
- The app will automatically download Chrome on first use
- If download fails, it uses HTTP method which works for most sites

**"Application won't start"**
- Ensure you have .NET 9.0 runtime installed
- Try running as administrator
- Check Windows Firewall settings

### Performance Tips
- **URL Analysis**: Works best with modern websites
- **File Analysis**: Instant results for local HTML files
- **Large Sites**: May take longer due to JavaScript rendering
- **Memory Usage**: ~200MB with Chrome, ~50MB with HTTP fallback

## 📊 Accessibility Standards

The app checks compliance with:
- **WCAG 2.1**: Web Content Accessibility Guidelines
- **Section 508**: US federal accessibility standards
- **India RPwD Act**: Rights of Persons with Disabilities Act

### Check Categories
1. **Perceivable**: Alt text, color contrast, media alternatives
2. **Operable**: Keyboard navigation, timing, seizures
3. **Understandable**: Readable text, predictable behavior
4. **Robust**: Compatible with assistive technologies

## 🌱 Environmental Impact Analysis

The app now includes comprehensive environmental impact assessment to help developers understand and reduce their websites' carbon footprint.

### Environmental Metrics
- **Energy Consumption**: Calculated in kWh per page load
- **CO₂ Emissions**: Estimated carbon emissions in grams per page load
- **CDN Detection**: Identifies if the site uses content delivery networks
- **Environmental Rating**: Eco, Moderate, or High Impact classification

### How It Works
1. **Page Weight Analysis**: Measures total data transferred
2. **Request Counting**: Tracks number of HTTP requests
3. **CDN Detection**: Identifies popular CDN usage
4. **Energy Calculation**: Estimates server and network energy use
5. **CO₂ Estimation**: Converts energy use to carbon emissions

### Environmental Scoring
- **Eco (< 10g CO₂)**: Excellent environmental performance
- **Moderate (10-50g CO₂)**: Average environmental impact
- **High Impact (> 50g CO₂)**: Significant carbon footprint

### Export Integration
Environmental data is included in all export formats (TXT, PDF) but not displayed in the main UI to keep the focus on accessibility analysis.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

### Development Setup
```bash
# Install .NET 9.0 SDK
# Clone repository
git clone <repository-url>
cd WebAccessibilityChecker

# Restore packages
dotnet restore

# Run in development mode
dotnet run
```

## 📄 License

This project is licensed under the GPL-3.0-or-later License - see the LICENSE file for details.

## 🙏 Acknowledgments

- **HtmlAgilityPack**: For robust HTML parsing
- **PuppeteerSharp**: For headless browser capabilities
- **QuestPDF**: For beautiful PDF generation
- **.NET Community**: For excellent development tools

## 📞 Support

For issues, questions, or feature requests:
- Create an issue on GitHub
- Check the troubleshooting section
- Review the technical documentation
## 🌐 [AXIS-CORE Website](docs/website/index.html) - Interactive Demo & Documentation

Visit our comprehensive website for an interactive demo, complete documentation, and detailed guides.

## 🔧 AXIS-CORE SDK - Multi-Platform Accessibility Checking

AXIS-CORE is our comprehensive cross-platform SDK ecosystem that allows developers to integrate accessibility checking into their applications programmatically. Available for .NET, JavaScript/Node.js, and Rust with consistent APIs across all platforms.

### 🎯 SDK Overview

| SDK | Language | Status | Package Registry | Performance |
|-----|----------|--------|------------------|-------------|
| **AXIS-CORE** | .NET C# | ✅ Published | [NuGet](https://www.nuget.org/packages/AXIS-CORE/) | Excellent |
| **axis-core** | Rust | 🚀 Ready | [Crates.io](https://crates.io/crates/axis-core) | Exceptional |
| **axis-core-sdk** | JavaScript | ✅ Published | [npm](https://www.npmjs.com/package/axis-core-sdk) | Good |

### ✨ Key Features

- **Programmatic API**: Check URLs and HTML content for accessibility issues
- **WCAG 2.1 Compliance**: Automated checks against Web Content Accessibility Guidelines
- **Intelligent Scoring**: 0-100 accessibility scores with compliance status
- **Environmental Impact**: Calculate energy consumption and CO₂ emissions
- **Export Options**: Generate detailed TXT and PDF reports
- **Cross-platform**: Consistent API across multiple programming languages
- **Performance Optimized**: Memory-safe implementations with async support

### 🚀 Quick Start Examples

#### .NET (C#)
```csharp
using AXIS_CORE;

var checker = new AxisCore();
var report = await checker.CheckUrlAsync("https://example.com");

Console.WriteLine($"Accessibility Score: {report.AccessibilityScore}/100");
Console.WriteLine($"Compliance Status: {report.ComplianceStatus}");
Console.WriteLine($"Issues Found: {report.TotalIssues}");

// Export to text
string textReport = checker.ExportToText(report);
Console.WriteLine(textReport);
```

#### Rust
```rust
use axis_core::AxisCore;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let checker = AxisCore::new();
    let report = checker.check_url("https://example.com").await?;

    println!("Accessibility Score: {}/100", report.accessibility_score);
    println!("Compliance Status: {}", report.compliance_status);
    println!("Issues Found: {}", report.total_issues);

    Ok(())
}
```

#### JavaScript/Node.js
```javascript
const AxisCore = require('axis-core-sdk');

async function checkAccessibility() {
    const checker = new AxisCore();

    try {
        const report = await checker.checkUrl('https://example.com');

        console.log(`Accessibility Score: ${report.accessibilityScore}/100`);
        console.log(`Compliance Status: ${report.complianceStatus}`);
        console.log(`Issues Found: ${report.totalIssues}`);

        // Export to text
        const textReport = checker.exportToText(report);
        console.log(textReport);

    } catch (error) {
        console.error('Error:', error.message);
    }
}

checkAccessibility();
```

### 📊 Report Structure

All SDKs return a consistent report structure:

```javascript
{
    // Issue Summary
    totalIssues: 5,
    errorCount: 1,
    warningCount: 3,
    infoCount: 1,

    // Scores (0-100)
    accessibilityScore: 85,
    seoScore: 0,
    performanceScore: 0,
    environmentScore: 0,
    safetyScore: 0,

    // Compliance Status
    complianceStatus: "Mostly Compliant",

    // Issues Array
    issues: [
        {
            issueType: "Missing Alt Text",
            elementSnippet: "<img src='...' ...>",
            suggestedFix: "Add alt attribute describing the image",
            severity: "Warning",
            category: "Accessibility",
            fixExample: "<img src='image.jpg' alt='Description'>"
        }
    ],

    // Metadata
    websiteUrl: "https://example.com",
    pageSize: 12345,
    pageLoadTime: 2.5,
    requestCount: 1,

    // Environmental Impact
    energyConsumptionKwh: 0.01,
    co2EmissionsGrams: 5.0,
    environmentalRating: "Eco"
}
```

### 📦 Installation & Setup

#### .NET SDK (Published)
```bash
# Install via NuGet Package Manager
Install-Package AXIS-CORE

# Or via .NET CLI
dotnet add package AXIS-CORE
```

#### Rust SDK (Ready to Publish)
```bash
# Add to Cargo.toml
[dependencies]
axis-core = "1.0"

# Or install directly
cargo add axis-core
```

#### JavaScript SDK (Ready to Publish)
```bash
# Install via npm
npm install axis-core-sdk

# Or via yarn
yarn add axis-core-sdk
```

### 📚 SDK Documentation

- **[AXIS-CORE .NET SDK](AXIS-CORE/README.md)** - Complete .NET documentation with examples
- **[axis-core Rust SDK](axis-core-rs/README.md)** - Rust crate documentation
- **[axis-core-sdk JavaScript](axis-core-js/README.md)** - Node.js package documentation

### 🔧 Advanced Usage

#### Custom Configuration
```csharp
// .NET - Configure checker options
var checker = new AxisCore();
// Options available in future versions
```

#### Batch Processing
```javascript
// JavaScript - Check multiple URLs
const urls = ['https://site1.com', 'https://site2.com'];
const reports = await Promise.all(urls.map(url => checker.checkUrl(url)));
```

#### HTML Content Analysis
```rust
// Rust - Check HTML strings directly
let html = r#"<html><body><img src="test.jpg"></body></html>"#;
let report = checker.check_html(html, "https://example.com")?;
```

### 🎯 Use Cases

- **CI/CD Integration**: Automate accessibility testing in build pipelines
- **Development Tools**: Real-time accessibility checking during development
- **Content Management**: Validate accessibility before publishing
- **Compliance Monitoring**: Regular accessibility audits
- **Educational Tools**: Learn accessibility best practices
- **API Services**: Build accessibility-as-a-service platforms

### 🌍 Standards Compliance

AXIS-CORE SDKs check compliance with:
- **WCAG 2.1**: Web Content Accessibility Guidelines
- **Section 508**: US federal accessibility standards
- **EN 301 549**: European accessibility requirements

### 📈 Performance Benchmarks

| SDK | Language | Cold Start | Warm Check | Memory Usage |
|-----|----------|------------|------------|--------------|
| AXIS-CORE | .NET | ~50ms | ~10ms | ~5MB |
| axis-core | Rust | ~5ms | ~1ms | ~1MB |
| axis-core-sdk | Node.js | ~200ms | ~50ms | ~50MB |

### 🤝 Contributing to AXIS-CORE

We welcome contributions to the AXIS-CORE SDK ecosystem:

1. **Bug Reports**: Use GitHub Issues for each SDK repository
2. **Feature Requests**: Propose new accessibility checks or API improvements
3. **Code Contributions**: Submit PRs for bug fixes or enhancements
4. **Documentation**: Help improve SDK documentation and examples

### 📞 Support & Community

- **GitHub Issues**: Report bugs and request features
- **Discussions**: Join community conversations
- **Documentation**: Comprehensive guides and examples
- **Examples Repository**: Sample applications using AXIS-CORE

### 🏆 Why AXIS-CORE?

- **Consistent API**: Same interface across all platforms
- **Production Ready**: Thoroughly tested and documented
- **Performance Focused**: Optimized for speed and memory usage
- **Developer Friendly**: Simple integration with comprehensive error handling
- **Future Proof**: Extensible architecture for new accessibility standards
- **Open Source**: GPL-3.0-or-later licensed, community-driven development

---

<br>Please do checkout our [A11Y: LAZY EDITION](https://github.com/ABHIRAM-CREATOR06/a11y-check) where developer could check accessibity right in process of development in vscode.<br>
<br> <br>
**Made with ❤️ for a more accessible web**
