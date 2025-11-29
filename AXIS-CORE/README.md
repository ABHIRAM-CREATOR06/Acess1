# AXIS-CORE SDK

A powerful .NET library for automated web accessibility checking. Built to help developers ensure their websites meet WCAG 2.1 standards and provide a better experience for everyone.

## Quick Start

Install the package:
```bash
dotnet add package AXIS-CORE
```

Check a website:
```csharp
using AXIS_CORE;

var checker = new AxisCore();
var report = await checker.CheckUrlAsync("https://yourwebsite.com");

Console.WriteLine($"Accessibility Score: {report.AccessibilityScore}/100");
Console.WriteLine($"Issues Found: {report.TotalIssues}");
```

## Why AXIS-CORE?

I built this because I kept running into the same accessibility issues on client projects. Manual checking was taking forever, and existing tools were either too expensive or too complicated. So I took the accessibility checker I made for my own use and turned it into a proper SDK.

Now developers can catch accessibility problems early in development, integrate checks into CI/CD pipelines, and generate compliance reports automatically.

## Features

- **WCAG 2.1 Compliance**: Checks against all major accessibility guidelines
- **URL & HTML Analysis**: Test live websites or raw HTML content
- **Comprehensive Scoring**: Get detailed accessibility scores (0-100)
- **Export Reports**: Generate TXT and PDF reports
- **Environmental Impact**: Track CO₂ emissions and energy usage
- **Fast & Reliable**: Built with performance in mind

## Basic Usage

### Check a Website
```csharp
var checker = new AxisCore();
var report = await checker.CheckUrlAsync("https://example.com");

if (report.AccessibilityScore >= 95) {
    Console.WriteLine("Site is fully compliant!");
} else if (report.AccessibilityScore >= 80) {
    Console.WriteLine("Site is mostly compliant");
} else {
    Console.WriteLine("Site needs accessibility improvements");
}
```

### Check HTML Content
```csharp
var html = "<html><body><img src='test.jpg' /></body></html>";
var report = checker.CheckHtml(html);

// Missing alt text will be flagged as an issue
foreach (var issue in report.Issues) {
    Console.WriteLine($"{issue.Type}: {issue.SuggestedFix}");
}
```

### Export Reports
```csharp
var report = await checker.CheckUrlAsync("https://example.com");

// Get text report
string textReport = checker.ExportToText(report);
File.WriteAllText("accessibility-report.txt", textReport);

// Get PDF report
byte[] pdfBytes = checker.ExportToPdf(report);
File.WriteAllBytes("accessibility-report.pdf", pdfBytes);
```

## Understanding the Results

### Accessibility Score
- **95-100**: Fully Compliant - Meets all WCAG guidelines
- **80-94**: Mostly Compliant - Minor issues to address
- **60-79**: Partially Compliant - Significant improvements needed
- **<60**: Not Compliant - Major accessibility barriers present

### Issue Categories
- **Accessibility**: Core WCAG compliance issues
- **SEO**: Search engine optimization problems
- **Performance**: Speed and loading issues
- **Environment**: Energy usage and carbon footprint
- **Safety**: Security and privacy concerns

## Advanced Usage

### Custom Analysis Options
```csharp
// The SDK is designed to be simple by default
// but extensible for advanced use cases
var checker = new AxisCore();

// All configuration happens through the main API
// Future versions will add more customization options
```

### CI/CD Integration
```yaml
# Add to your GitHub Actions workflow
- name: Check Accessibility
  run: |
    dotnet test --filter "Accessibility"
    # Or integrate directly in your build process
```

## Common Issues & Solutions

**"Headless browser failed"**
- This is normal on first run
- The SDK falls back to HTTP download automatically
- Chrome gets downloaded for future runs

**"Slow analysis"**
- Large sites take longer due to JavaScript rendering
- Consider analyzing specific pages instead of entire sites
- Use the HTML checking method for faster local testing

**"False positives"**
- Some issues might be flagged incorrectly
- Always review results manually
- The tool is designed to be thorough rather than perfect

## Contributing

Found a bug or want to suggest a feature? Open an issue on GitHub. I built this tool because I needed it, and I'm always looking to improve it.

## License

GPL-3.0-or-later - Use it, modify it, share it. Just keep it open source.

---

Built with ❤️ for a more accessible web