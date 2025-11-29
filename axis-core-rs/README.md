# AXIS-CORE SDK (Rust)

A fast, memory-safe Rust library for automated web accessibility checking. Built with performance and reliability in mind.

## Installation

Add this to your `Cargo.toml`:

```toml
[dependencies]
axis-core = "1.0"
```

## Quick Start

```rust
use axis_core::AxisCore;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let checker = AxisCore::new();
    let report = checker.check_url("https://example.com").await?;

    println!("Accessibility Score: {}/100", report.accessibility_score);
    println!("Issues Found: {}", report.total_issues);

    Ok(())
}
```

## Features

- **WCAG 2.1 Compliance**: Automated checks against accessibility guidelines
- **URL & HTML Analysis**: Test live websites or raw HTML content
- **Performance Focused**: Zero-cost abstractions and async support
- **Memory Safe**: Rust's ownership system prevents common bugs
- **Comprehensive Reporting**: Detailed issue breakdowns and scoring
- **Environmental Impact**: CO₂ emissions and energy consumption tracking

## API Reference

### AxisCore

Main SDK struct for accessibility checking.

#### Methods

- `new() -> AxisCore`: Create a new checker instance
- `check_url(url: &str) -> Result<Report>`: Check a website by URL
- `check_html(html: &str, base_url: Option<&str>) -> Report`: Check HTML content
- `export_to_text(report: &Report) -> String`: Export report as text
- `version() -> &'static str`: Get SDK version

### Report

Contains the results of an accessibility analysis.

#### Fields

- `issues: Vec<Issue>` - All issues found
- `total_issues: usize` - Total number of issues
- `error_count/warning_count/info_count: usize` - Issues by severity
- `accessibility_score: u32` - Score from 0-100
- `compliance_status: String` - Overall compliance level
- `page_load_time: f64` - Load time in seconds
- `energy_consumption_kwh: f64` - Estimated energy use
- `co2_emissions_grams: f64` - Estimated carbon emissions

### Issue

Represents a single accessibility issue.

#### Fields

- `issue_type: String` - Type of issue (e.g., "Missing Alt Text")
- `element_snippet: Option<String>` - HTML snippet where issue was found
- `suggested_fix: Option<String>` - How to fix the issue
- `severity: Severity` - Error, Warning, or Info
- `fix_example: Option<String>` - Example fix code
- `category: Category` - Issue category

## Examples

### Basic URL Check

```rust
use axis_core::AxisCore;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let checker = AxisCore::new();
    let report = checker.check_url("https://example.com").await?;

    match report.accessibility_score {
        95..=100 => println!("Fully compliant!"),
        80..=94 => println!("Mostly compliant"),
        60..=79 => println!("Needs improvement"),
        _ => println!("Major accessibility issues"),
    }

    Ok(())
}
```

### HTML Content Analysis

```rust
use axis_core::AxisCore;

let checker = AxisCore::new();
let html = r#"<html><body><img src="test.jpg" /></body></html>"#;
let report = checker.check_html(html, None);

// Check for missing alt text
for issue in &report.issues {
    if issue.issue_type.contains("Alt Text") {
        println!("Found accessibility issue: {}", issue.suggested_fix.as_deref().unwrap_or("Unknown"));
    }
}
```

### Export Report

```rust
use axis_core::AxisCore;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let checker = AxisCore::new();
    let report = checker.check_url("https://example.com").await?;

    let text_report = checker.export_to_text(&report);
    std::fs::write("accessibility-report.txt", text_report)?;

    Ok(())
}
```

## Understanding Scores

- **95-100**: Fully Compliant - Meets all WCAG guidelines
- **80-94**: Mostly Compliant - Minor issues to address
- **60-79**: Partially Compliant - Significant improvements needed
- **<60**: Not Compliant - Major accessibility barriers present

## Issue Categories

- **Accessibility**: Core WCAG compliance issues
- **SEO**: Search engine optimization problems
- **Performance**: Speed and loading issues
- **Environment**: Energy usage and carbon footprint
- **Safety**: Security and privacy concerns

## Performance

AXIS-CORE is designed for high performance:

- **Async/Await**: Non-blocking I/O operations
- **Zero-copy parsing**: Efficient HTML processing with `scraper`
- **Memory safe**: Rust's ownership system prevents leaks
- **Fast checks**: Optimized algorithms for common issues

## Error Handling

The SDK uses Rust's `Result` type for error handling:

```rust
match checker.check_url("https://example.com").await {
    Ok(report) => println!("Score: {}", report.accessibility_score),
    Err(e) => eprintln!("Error: {}", e),
}
```

## Contributing

Issues and pull requests welcome! This SDK is built with Rust's safety and performance guarantees.

## License

GPL-3.0-or-later

---

Built with ❤️ for a more accessible web