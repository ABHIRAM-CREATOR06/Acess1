//! # AXIS-CORE SDK
//!
//! A Rust library for automated web accessibility checking.
//!
//! ## Quick Start
//!
//! ```rust
//! use axis_core::AxisCore;
//!
//! #[tokio::main]
//! async fn main() -> Result<(), Box<dyn std::error::Error>> {
//!     let checker = AxisCore::new();
//!     let report = checker.check_url("https://example.com").await?;
//!
//!     println!("Accessibility Score: {}/100", report.accessibility_score);
//!     println!("Issues Found: {}", report.total_issues);
//!
//!     Ok(())
//! }
//! ```

pub mod models;
pub mod checker;
pub mod parser;

use crate::models::Report;
use crate::checker::AccessibilityChecker;
use crate::parser::HtmlParser;

/// Main AXIS-CORE SDK struct
pub struct AxisCore {
    checker: AccessibilityChecker,
    parser: HtmlParser,
}

impl AxisCore {
    /// Create a new AXIS-CORE checker instance
    pub fn new() -> Self {
        Self {
            checker: AccessibilityChecker::new(),
            parser: HtmlParser::new(),
        }
    }

    /// Check accessibility of a web page by URL
    ///
    /// # Example
    /// ```rust
    /// # use axis_core::AxisCore;
    /// # #[tokio::main]
    /// # async fn main() -> Result<(), Box<dyn std::error::Error>> {
    /// let checker = AxisCore::new();
    /// let report = checker.check_url("https://example.com").await?;
    /// # Ok(())
    /// # }
    /// ```
    pub async fn check_url(&self, url: &str) -> Result<Report, Box<dyn std::error::Error>> {
        let load_result = self.parser.load_from_url(url).await?;
        let report = self.checker.check_accessibility(load_result);
        Ok(report)
    }

    /// Check accessibility of HTML content
    ///
    /// # Example
    /// ```rust
    /// # use axis_core::AxisCore;
    /// let checker = AxisCore::new();
    /// let html = r#"<html><body><img src="test.jpg" /></body></html>"#;
    /// let report = checker.check_html(html, None);
    /// // Missing alt text will be flagged
    /// ```
    pub fn check_html(&self, html: &str, base_url: Option<&str>) -> Report {
        let load_result = self.parser.parse_html(html, base_url);
        self.checker.check_accessibility(load_result)
    }

    /// Export a report to text format
    pub fn export_to_text(&self, report: &Report) -> String {
        self.checker.export_to_text(report)
    }

    /// Get the SDK version
    pub fn version() -> &'static str {
        env!("CARGO_PKG_VERSION")
    }
}

impl Default for AxisCore {
    fn default() -> Self {
        Self::new()
    }
}