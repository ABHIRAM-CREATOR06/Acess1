# AXIS-CORE SDK

AXIS-CORE is a cross-platform SDK for programmatic web accessibility checking. It provides developers with tools to integrate accessibility audits into their applications, CI/CD pipelines, and automated testing workflows.

## Features

- **URL Analysis**: Check any website for accessibility issues
- **HTML Content Analysis**: Analyze local HTML strings
- **WCAG Compliance**: Automated checks against WCAG 2.1 standards
- **Real-time Scoring**: Get instant accessibility scores (0-100)
- **Compliance Status**: Determine if content is Fully/Mostly/Partially/Not Compliant
- **Export Options**: Generate TXT and PDF reports
- **Cross-platform**: Available for .NET, JavaScript/Node.js, Python, and Rust

## Installation

### .NET
```bash
dotnet add package AXIS-CORE
```

### JavaScript/Node.js
```bash
npm install axis-core
```

### Python
```bash
pip install axis-core
```

### Rust
```toml
[dependencies]
axis-core = "1.0"
```

## Usage

### .NET
```csharp
using AXIS_CORE;

var checker = new AxisCore();
var report = await checker.CheckUrlAsync("https://example.com");
Console.WriteLine($"Accessibility Score: {report.AccessibilityScore}/100");
Console.WriteLine($"Compliance: {report.ComplianceStatus}");
```

### JavaScript/Node.js
```javascript
const { AxisCore } = require('axis-core');

const checker = new AxisCore();
checker.checkUrl('https://example.com').then(report => {
    console.log(`Accessibility Score: ${report.accessibilityScore}/100`);
    console.log(`Compliance: ${report.complianceStatus}`);
});
```

### Python
```python
from axis_core import AxisCore

checker = AxisCore()
report = checker.check_url('https://example.com')
print(f"Accessibility Score: {report.accessibility_score}/100")
print(f"Compliance: {report.compliance_status}")
```

### Rust
```rust
use axis_core::AxisCore;

let checker = AxisCore::new();
let report = checker.check_url("https://example.com").await?;
println!("Accessibility Score: {}/100", report.accessibility_score);
println!("Compliance: {}", report.compliance_status);
```

## API Reference

### Methods

- `CheckUrlAsync(url)` / `checkUrl(url)` / `check_url(url)` / `check_url(url)`: Analyze a web page by URL
- `CheckHtml(html, baseUrl?)` / `checkHtml(html, baseUrl?)` / `check_html(html, baseUrl?)` / `check_html(html, baseUrl?)`: Analyze HTML content
- `ExportToText(report)` / `exportToText(report)` / `export_to_text(report)` / `export_to_text(report)`: Export report as text
- `ExportToPdf(report)` / `exportToPdf(report)` / `export_to_pdf(report)` / `export_to_pdf(report)`: Export report as PDF bytes

### Report Properties

- `AccessibilityScore` / `accessibilityScore` / `accessibility_score`: Score from 0-100
- `ComplianceStatus` / `complianceStatus` / `compliance_status`: "Fully Compliant", "Mostly Compliant", "Partially Compliant", or "Not Compliant"
- `Issues` / `issues`: Array of accessibility issues found
- `TotalIssues` / `totalIssues` / `total_issues`: Total number of issues
- `ErrorCount` / `errorCount` / `error_count`: Number of error-level issues
- `WarningCount` / `warningCount` / `warning_count`: Number of warning-level issues
- `InfoCount` / `infoCount` / `info_count`: Number of info-level issues

## Checks Performed

AXIS-CORE checks for:

- Missing or improper alt text on images
- Form labels and accessibility
- Document title presence
- Heading hierarchy structure
- Color contrast ratios
- Font size and line height (eye comfort)
- ARIA attributes usage
- Language attributes
- WebXR accessibility considerations
- Best practices
- Mobile responsiveness
- Dark mode support

## License

GPL-3.0-or-later License - see LICENSE file for details.

## Contributing

Contributions are welcome! Please see the main repository for contribution guidelines.