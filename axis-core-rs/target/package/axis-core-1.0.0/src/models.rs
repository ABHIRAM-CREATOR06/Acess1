use serde::{Deserialize, Serialize};

/// Severity levels for accessibility issues
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub enum Severity {
    Error,
    Warning,
    Info,
}

/// Categories of accessibility issues
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub enum Category {
    Accessibility,
    SEO,
    Performance,
    BestPractices,
    Environment,
    Safety,
}

/// Represents an accessibility issue found during analysis
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Issue {
    /// Type of issue (e.g., "Missing Alt Text")
    pub issue_type: String,
    /// HTML snippet where the issue was found
    pub element_snippet: Option<String>,
    /// Suggested fix for the issue
    pub suggested_fix: Option<String>,
    /// Severity level of the issue
    pub severity: Severity,
    /// Example of how to fix the issue
    pub fix_example: Option<String>,
    /// Category this issue belongs to
    pub category: Category,
}

impl Issue {
    pub fn new(
        issue_type: &str,
        element_snippet: Option<&str>,
        suggested_fix: Option<&str>,
        severity: Severity,
        fix_example: Option<&str>,
        category: Category,
    ) -> Self {
        Self {
            issue_type: issue_type.to_string(),
            element_snippet: element_snippet.map(|s| s.to_string()),
            suggested_fix: suggested_fix.map(|s| s.to_string()),
            severity,
            fix_example: fix_example.map(|s| s.to_string()),
            category,
        }
    }
}

/// Result of loading a web page
#[derive(Debug, Clone)]
pub struct PageLoadResult {
    /// Parsed HTML document
    pub document: scraper::Html,
    /// Time taken to load the page (in seconds)
    pub load_time: f64,
    /// Number of HTTP requests made
    pub request_count: u32,
    /// Size of the page in bytes
    pub page_size: u64,
    /// Whether the page uses compression
    pub is_compressed: bool,
    /// Whether the page has caching headers
    pub has_caching_headers: bool,
    /// Optional website URL
    pub website_url: Option<String>,
}

impl PageLoadResult {
    pub fn new(
        document: scraper::Html,
        load_time: f64,
        request_count: u32,
        page_size: u64,
        is_compressed: bool,
        has_caching_headers: bool,
        website_url: Option<&str>,
    ) -> Self {
        Self {
            document,
            load_time,
            request_count,
            page_size,
            is_compressed,
            has_caching_headers,
            website_url: website_url.map(|s| s.to_string()),
        }
    }
}

/// Comprehensive accessibility report
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Report {
    /// List of issues found
    pub issues: Vec<Issue>,
    /// Total number of issues
    pub total_issues: usize,
    /// Number of error-level issues
    pub error_count: usize,
    /// Number of warning-level issues
    pub warning_count: usize,
    /// Number of info-level issues
    pub info_count: usize,
    /// Accessibility score (0-100)
    pub accessibility_score: u32,
    /// SEO score (0-100)
    pub seo_score: u32,
    /// Performance score (0-100)
    pub performance_score: u32,
    /// Best practices score (0-100)
    pub best_practices_score: u32,
    /// Environment score (0-100)
    pub environment_score: u32,
    /// Safety score (0-100)
    pub safety_score: u32,
    /// Overall compliance status
    pub compliance_status: String,
    /// Website URL that was analyzed
    pub website_url: Option<String>,
    /// Page load time in seconds
    pub page_load_time: f64,
    /// Number of HTTP requests
    pub request_count: u32,
    /// Page size in bytes
    pub page_size: u64,
    /// Estimated energy consumption in kWh
    pub energy_consumption_kwh: f64,
    /// Estimated CO₂ emissions in grams
    pub co2_emissions_grams: f64,
    /// Environmental rating
    pub environmental_rating: String,
    /// Whether the site uses CDN
    pub uses_cdn: bool,
}

impl Report {
    pub fn new(website_url: Option<&str>) -> Self {
        Self {
            issues: Vec::new(),
            total_issues: 0,
            error_count: 0,
            warning_count: 0,
            info_count: 0,
            accessibility_score: 0,
            seo_score: 0,
            performance_score: 0,
            best_practices_score: 0,
            environment_score: 0,
            safety_score: 0,
            compliance_status: "Unknown".to_string(),
            website_url: website_url.map(|s| s.to_string()),
            page_load_time: 0.0,
            request_count: 0,
            page_size: 0,
            energy_consumption_kwh: 0.0,
            co2_emissions_grams: 0.0,
            environmental_rating: "Unknown".to_string(),
            uses_cdn: false,
        }
    }

    /// Recalculate counts and scores based on current issues
    pub fn recalculate(&mut self) {
        self.total_issues = self.issues.len();
        self.error_count = self.issues.iter().filter(|i| matches!(i.severity, Severity::Error)).count();
        self.warning_count = self.issues.iter().filter(|i| matches!(i.severity, Severity::Warning)).count();
        self.info_count = self.issues.iter().filter(|i| matches!(i.severity, Severity::Info)).count();

        // Calculate accessibility score
        let accessibility_issues: Vec<_> = self.issues.iter()
            .filter(|i| matches!(i.category, Category::Accessibility))
            .collect();

        let total_acc_issues = accessibility_issues.len() as f64;
        let critical_issues = accessibility_issues.iter()
            .filter(|i| matches!(i.severity, Severity::Error))
            .count() as f64;
        let warning_issues = accessibility_issues.iter()
            .filter(|i| matches!(i.severity, Severity::Warning))
            .count() as f64;

        let base_score = 100.0;
        let critical_penalty = critical_issues * 3.0;
        let warning_penalty = warning_issues * 1.0;
        let volume_penalty = if total_acc_issues > 10.0 { (total_acc_issues - 10.0) * 0.5 } else { 0.0 };

        self.accessibility_score = ((base_score - critical_penalty - warning_penalty - volume_penalty)
            .max(0.0).min(100.0)) as u32;

        // Set compliance status based on accessibility score
        self.compliance_status = if self.accessibility_score >= 95 {
            "Fully Compliant"
        } else if self.accessibility_score >= 80 {
            "Mostly Compliant"
        } else if self.accessibility_score >= 60 {
            "Partially Compliant"
        } else {
            "Not Compliant"
        }.to_string();
    }
}