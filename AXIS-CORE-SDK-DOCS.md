# AXIS-CORE SDK Documentation

## Overview

AXIS-CORE is a comprehensive, cross-platform SDK ecosystem for programmatic web accessibility checking. It provides developers with consistent APIs across multiple programming languages to integrate accessibility auditing into their applications, CI/CD pipelines, and development workflows.

## Architecture

### Core Components

1. **Accessibility Engine**: Shared logic for WCAG 2.1 compliance checking
2. **Platform Adapters**: Language-specific implementations
3. **Report Generators**: Consistent output formats across platforms
4. **Export Utilities**: TXT and PDF report generation

### Supported Platforms

| Platform | Language | Status | Package Manager |
|----------|----------|--------|-----------------|
| .NET | C# | ✅ Published | NuGet |
| Rust | Rust | ✅ Ready | Cargo |
| JavaScript | Node.js | ✅ Ready | npm |

## API Reference

### Common Interface

All AXIS-CORE SDKs implement the same core interface:

```typescript
interface AxisCoreChecker {
    checkUrl(url: string): Promise<Report> | Report
    checkHtml(html: string, baseUrl?: string): Report
    exportToText(report: Report): string
    getVersion(): string
}

interface Report {
    // Issue counts
    totalIssues: number
    errorCount: number
    warningCount: number
    infoCount: number

    // Scores (0-100)
    accessibilityScore: number
    seoScore: number
    performanceScore: number
    environmentScore: number
    safetyScore: number

    // Compliance
    complianceStatus: string // "Fully/Mostly/Partially/Not Compliant"

    // Issues
    issues: Issue[]

    // Metadata
    websiteUrl: string
    pageSize: number
    pageLoadTime: number
    requestCount: number

    // Environmental
    energyConsumptionKwh: number
    co2EmissionsGrams: number
    environmentalRating: string
}

interface Issue {
    issueType: string
    elementSnippet: string
    suggestedFix: string
    severity: string // "Error", "Warning", "Info"
    category: string // "Accessibility", "SEO", etc.
    fixExample: string
}
```

## Platform-Specific Implementations

### .NET SDK

#### Installation
```bash
# NuGet Package Manager
Install-Package AXIS-CORE

# .NET CLI
dotnet add package AXIS-CORE
```

#### Usage
```csharp
using AXIS_CORE;

public async Task CheckWebsite()
{
    var checker = new AxisCore();

    try
    {
        // Check URL
        var report = await checker.CheckUrlAsync("https://example.com");

        Console.WriteLine($"Score: {report.AccessibilityScore}/100");
        Console.WriteLine($"Status: {report.ComplianceStatus}");

        // Export report
        var textReport = checker.ExportToText(report);
        await File.WriteAllTextAsync("report.txt", textReport);

        // Check HTML directly
        var htmlReport = checker.CheckHtml("<html>...</html>", "https://example.com");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
```

#### Configuration
```csharp
// Future: Configure checker options
var checker = new AxisCore();
// Options will be available in future versions
```

### Rust SDK

#### Installation
```toml
# Cargo.toml
[dependencies]
axis-core = "1.0"
tokio = { version = "1.0", features = ["full"] }
```

#### Usage
```rust
use axis_core::AxisCore;
use std::error::Error;

#[tokio::main]
async fn main() -> Result<(), Box<dyn Error>> {
    let checker = AxisCore::new();

    // Check URL
    let report = checker.check_url("https://example.com").await?;

    println!("Score: {}/100", report.accessibility_score);
    println!("Status: {}", report.compliance_status);
    println!("Issues: {}", report.total_issues);

    // Check HTML
    let html_report = checker.check_html("<html>...</html>", "https://example.com");

    // Export to text
    let text_report = checker.export_to_text(&report);
    println!("{}", text_report);

    Ok(())
}
```

#### Async Operations
```rust
// All network operations are async
let report = checker.check_url("https://example.com").await?;
```

### JavaScript SDK

#### Installation
```bash
npm install axis-core-sdk
```

#### Usage
```javascript
const AxisCore = require('axis-core-sdk');

async function checkAccessibility() {
    const checker = new AxisCore();

    try {
        // Check URL with browser rendering
        const report = await checker.checkUrl('https://example.com');

        console.log(`Score: ${report.accessibilityScore}/100`);
        console.log(`Status: ${report.complianceStatus}`);

        // Check HTML directly
        const htmlReport = checker.checkHtml('<html>...</html>', 'https://example.com');

        // Export report
        const textReport = checker.exportToText(report);
        console.log(textReport);

    } catch (error) {
        console.error('Error:', error.message);
    }
}

checkAccessibility();
```

#### Browser Configuration
```javascript
// Puppeteer handles browser automatically
// Custom browser options available in future versions
```

## Accessibility Checks

### Implemented Checks

1. **Alt Text Validation**
   - Missing alt attributes on images
   - Empty alt attributes
   - Decorative image handling

2. **Form Accessibility**
   - Missing labels for form controls
   - Improper label associations
   - Input type validation

3. **Heading Hierarchy**
   - Missing H1 tags
   - Improper heading level skips
   - Heading structure validation

4. **Document Structure**
   - Missing title tags
   - Missing lang attributes
   - Viewport meta tag validation

5. **Link Accessibility**
   - Missing link text
   - Generic link text validation

### Scoring Algorithm

```javascript
// Accessibility Score Calculation
function calculateScore(issues) {
    const errors = issues.filter(i => i.severity === 'Error').length;
    const warnings = issues.filter(i => i.severity === 'Warning').length;

    let score = 100;
    score -= errors * 10;      // 10 points per error
    score -= warnings * 3;     // 3 points per warning

    return Math.max(0, Math.min(100, score));
}

// Compliance Status
function getComplianceStatus(score) {
    if (score >= 95) return "Fully Compliant";
    if (score >= 80) return "Mostly Compliant";
    if (score >= 60) return "Partially Compliant";
    return "Not Compliant";
}
```

## Export Formats

### Text Export
```
AXIS-CORE Accessibility Report
====================================

Website: https://example.com
Total Issues: 3
Errors: 1, Warnings: 2, Info: 0
Accessibility Score: 85/100
Compliance Status: Mostly Compliant

ISSUES FOUND:
--------------

1. Missing Alt Text (Warning)
   Category: Accessibility
   Element: <img src="image.jpg">
   Fix: Add alt attribute describing the image

2. Missing Label (Warning)
   Category: Accessibility
   Element: <input type="text" id="name">
   Fix: Add label with for attribute or aria-label

3. Missing Title (Error)
   Category: Accessibility
   Element: <head>...</head>
   Fix: Add <title> tag in <head>
```

### PDF Export (.NET Only)
- Professional formatted reports
- Charts and graphs
- Compliance certificates
- Detailed issue breakdowns

## Environmental Impact Analysis

### Metrics Calculated

- **Energy Consumption**: kWh per page load
- **CO₂ Emissions**: grams of CO₂ per page load
- **CDN Detection**: identifies content delivery network usage
- **Environmental Rating**: Eco/Moderate/High Impact

### Calculation Formula

```javascript
function calculateEnvironmentalImpact(pageSize, requestCount, usesCDN) {
    // Base calculations
    const dataTransferGB = pageSize / (1024 * 1024 * 1024);
    const baseEnergy = 0.01; // kWh
    const dataEnergy = dataTransferGB * 200; // 0.2 kWh per GB
    const requestEnergy = requestCount * 0.0001; // per request

    // CDN reduces energy by 30%
    const totalEnergy = usesCDN
        ? (baseEnergy + dataEnergy + requestEnergy) * 0.7
        : baseEnergy + dataEnergy + requestEnergy;

    // Convert to CO₂ (500g per kWh average)
    const co2Grams = totalEnergy * 500;

    return {
        energyKwh: totalEnergy,
        co2Grams: co2Grams,
        rating: co2Grams < 10 ? 'Eco' :
                co2Grams < 50 ? 'Moderate' : 'High Impact'
    };
}
```

## Error Handling

### Common Errors

```javascript
// Network errors
try {
    const report = await checker.checkUrl('https://example.com');
} catch (error) {
    if (error.message.includes('timeout')) {
        console.log('Request timed out');
    } else if (error.message.includes('404')) {
        console.log('Page not found');
    }
}

// HTML parsing errors
try {
    const report = checker.checkHtml(invalidHtml);
} catch (error) {
    console.log('HTML parsing failed:', error.message);
}
```

### Platform-Specific Errors

**Rust:**
```rust
match checker.check_url(url).await {
    Ok(report) => println!("Success: {}", report.accessibility_score),
    Err(e) => eprintln!("Error: {}", e),
}
```

**.NET:**
```csharp
try {
    var report = await checker.CheckUrlAsync(url);
} catch (HttpRequestException ex) {
    Console.WriteLine($"Network error: {ex.Message}");
} catch (Exception ex) {
    Console.WriteLine($"General error: {ex.Message}");
}
```

## Performance Optimization

### Best Practices

1. **Batch Processing**: Check multiple URLs in parallel
2. **Caching**: Cache results for repeated checks
3. **Timeouts**: Set appropriate timeouts for network requests
4. **Resource Limits**: Limit concurrent checks

### Platform Performance

| Operation | .NET | Rust | JavaScript |
|-----------|------|------|------------|
| Cold Start | ~50ms | ~5ms | ~200ms |
| URL Check | ~500ms | ~100ms | ~800ms |
| HTML Check | ~10ms | ~1ms | ~50ms |
| Memory Usage | ~5MB | ~1MB | ~50MB |

## Integration Examples

### CI/CD Integration

#### GitHub Actions (.NET)
```yaml
- name: Check Accessibility
  run: |
    dotnet add package AXIS-CORE
    dotnet build
    dotnet run -- check-url https://example.com
```

#### GitHub Actions (Rust)
```yaml
- name: Check Accessibility
  run: |
    cargo add axis-core
    cargo build --release
    cargo run -- check-url https://example.com
```

### Web Application Integration

#### Express.js Middleware
```javascript
const express = require('express');
const AxisCore = require('axis-core-sdk');

const app = express();
const checker = new AxisCore();

app.post('/check-accessibility', async (req, res) => {
    try {
        const { url } = req.body;
        const report = await checker.checkUrl(url);
        res.json(report);
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});
```

### Desktop Application Integration

#### Electron + AXIS-CORE
```javascript
const { app, BrowserWindow, ipcMain } = require('electron');
const AxisCore = require('axis-core-sdk');

const checker = new AxisCore();

ipcMain.handle('check-accessibility', async (event, url) => {
    try {
        const report = await checker.checkUrl(url);
        return report;
    } catch (error) {
        throw new Error(error.message);
    }
});
```

## Testing

### Unit Tests

#### .NET (xUnit)
```csharp
[Fact]
public void CheckHtml_WithValidHtml_ReturnsReport()
{
    var checker = new AxisCore();
    var html = "<html><head><title>Test</title></head><body></body></html>";

    var report = checker.CheckHtml(html);

    Assert.NotNull(report);
    Assert.Equal(0, report.TotalIssues);
    Assert.Equal(100, report.AccessibilityScore);
}
```

#### Rust (tokio test)
```rust
#[tokio::test]
async fn test_check_html() {
    let checker = AxisCore::new();
    let html = r#"<html><head><title>Test</title></head><body></body></html>"#;

    let report = checker.check_html(html, "https://example.com");

    assert_eq!(report.total_issues, 0);
    assert_eq!(report.accessibility_score, 100);
}
```

### Integration Tests

```javascript
const AxisCore = require('axis-core-sdk');

describe('AxisCore Integration Tests', () => {
    let checker;

    beforeEach(() => {
        checker = new AxisCore();
    });

    test('should check valid website', async () => {
        const report = await checker.checkUrl('https://httpbin.org/html');
        expect(report.accessibilityScore).toBeGreaterThanOrEqual(0);
        expect(report.accessibilityScore).toBeLessThanOrEqual(100);
    }, 30000);

    test('should detect missing alt text', () => {
        const html = '<html><body><img src="test.jpg"></body></html>';
        const report = checker.checkHtml(html);

        expect(report.totalIssues).toBeGreaterThan(0);
        expect(report.issues.some(i => i.type === 'Missing Alt Text')).toBe(true);
    });
});
```

## Troubleshooting

### Common Issues

1. **Network Timeouts**
   - Increase timeout values
   - Check network connectivity
   - Use local HTML files for testing

2. **Memory Issues**
   - Process large sites in chunks
   - Implement result caching
   - Monitor memory usage

3. **Browser Launch Failures** (JavaScript)
   - Ensure Chrome/Chromium is installed
   - Check Puppeteer configuration
   - Use HTML checking as fallback

4. **Package Installation Issues**
   - Clear package cache
   - Check network connectivity
   - Verify package manager versions

### Debug Mode

```javascript
// Enable debug logging
const checker = new AxisCore();
// Debug options available in future versions
```

## Contributing

### Development Setup

1. **Clone Repository**
   ```bash
   git clone https://github.com/ABHIRAM-CREATOR06/Acess1.git
   cd Acess1
   ```

2. **Setup Development Environment**
   ```bash
   # .NET
   cd AXIS-CORE && dotnet restore

   # Rust
   cd axis-core-rs && cargo build

   # JavaScript
   cd axis-core-js && npm install
   ```

3. **Run Tests**
   ```bash
   # .NET
   dotnet test

   # Rust
   cargo test

   # JavaScript
   npm test
   ```

### Code Style Guidelines

- **Consistent Naming**: Use `checkUrl`/`check_url` across platforms
- **Error Handling**: Implement comprehensive error handling
- **Documentation**: Document all public APIs
- **Testing**: Maintain high test coverage

## License

AXIS-CORE SDK is licensed under GPL-3.0-or-later.

## Support

- **GitHub Issues**: Bug reports and feature requests
- **Documentation**: Comprehensive guides and examples
- **Community**: Join discussions and contribute

---

**Made with ❤️ for a more accessible web**