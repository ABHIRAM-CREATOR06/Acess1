# AXIS-CORE JavaScript SDK

AXIS-CORE SDK for programmatic web accessibility checking in JavaScript/Node.js applications.

## Installation

```bash
npm install axis-core
```

## Quick Start

```javascript
const AxisCore = require('axis-core');

async function checkAccessibility() {
    const checker = new AxisCore();

    try {
        // Check a website
        const report = await checker.checkUrl('https://example.com');

        console.log(`Accessibility Score: ${report.accessibilityScore}/100`);
        console.log(`Issues found: ${report.totalIssues}`);
        console.log(`Compliance: ${report.complianceStatus}`);

        // Export to text
        const textReport = checker.exportToText(report);
        console.log(textReport);

    } catch (error) {
        console.error('Error:', error.message);
    }
}

checkAccessibility();
```

## API Reference

### `new AxisCore()`
Creates a new AXIS-CORE checker instance.

### `checkUrl(url)`
Checks accessibility of a web page by URL.

**Parameters:**
- `url` (string): The URL of the web page to check

**Returns:** Promise resolving to accessibility report object

### `checkHtml(htmlContent, baseUrl?)`
Checks accessibility of HTML content directly.

**Parameters:**
- `htmlContent` (string): The HTML content to check
- `baseUrl` (string, optional): Base URL for resolving relative links

**Returns:** Accessibility report object

### `exportToText(report)`
Exports a report to formatted text.

**Parameters:**
- `report` (object): The accessibility report

**Returns:** Formatted text string

## Report Structure

```javascript
{
    issues: [
        {
            type: "Missing Alt Text",
            elementSnippet: "<img src='...' ...>",
            suggestedFix: "Add alt attribute describing the image",
            severity: "Warning", // Error, Warning, or Info
            category: "Accessibility"
        }
    ],
    totalIssues: 5,
    errorCount: 1,
    warningCount: 3,
    infoCount: 1,
    accessibilityScore: 85, // 0-100
    complianceStatus: "Mostly Compliant", // Fully/Mostly/Partially/Not Compliant
    websiteUrl: "https://example.com",
    pageSize: 12345 // bytes
}
```

## Features

- **URL Analysis**: Check live websites with JavaScript rendering
- **HTML Analysis**: Check raw HTML content
- **WCAG Compliance**: Automated checks against accessibility guidelines
- **Scoring System**: 0-100 accessibility scores
- **Detailed Reports**: Comprehensive issue descriptions and fix suggestions
- **Export Support**: Text-based report generation

## Requirements

- Node.js 14+
- For URL checking: Chrome/Chromium (automatically managed by Puppeteer)

## Dependencies

- `axios`: HTTP client for basic requests
- `cheerio`: HTML parsing and manipulation
- `puppeteer`: Headless browser for JavaScript rendering

## License

GPL-3.0-or-later