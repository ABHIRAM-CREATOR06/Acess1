use crate::models::{Issue, Report, PageLoadResult, Severity, Category};
use scraper::{Html, Selector};

/// Core accessibility checker implementation
pub struct AccessibilityChecker;

impl AccessibilityChecker {
    /// Create a new accessibility checker
    pub fn new() -> Self {
        Self
    }

    /// Check accessibility of a page and return a comprehensive report
    pub fn check_accessibility(&self, load_result: PageLoadResult) -> Report {
        let mut report = Report::new(load_result.website_url.as_deref());

        // Set basic page data
        report.page_load_time = load_result.load_time;
        report.request_count = load_result.request_count;
        report.page_size = load_result.page_size;
        report.uses_cdn = self.detect_cdn_usage(&load_result.document);
        self.calculate_environmental_impact(&mut report);

        // Run all accessibility checks
        report.issues.extend(self.check_alt_text(&load_result.document));
        report.issues.extend(self.check_labels(&load_result.document));
        report.issues.extend(self.check_title(&load_result.document));
        report.issues.extend(self.check_heading_hierarchy(&load_result.document));
        report.issues.extend(self.check_color_contrast(&load_result.document));
        report.issues.extend(self.check_lang_attributes(&load_result.document));
        report.issues.extend(self.check_best_practices(&load_result.document));

        // Performance and environment checks
        report.issues.extend(self.check_performance(&load_result));
        report.issues.extend(self.check_environment(&load_result.document, &load_result));

        // Calculate final scores
        report.recalculate();

        report
    }

    /// Check for missing alt text on images
    fn check_alt_text(&self, document: &Html) -> Vec<Issue> {
        let mut issues = Vec::new();
        let selector = Selector::parse("img").unwrap();

        for img in document.select(&selector) {
            let has_alt = img.value().attr("alt").is_some();
            let alt_value = img.value().attr("alt").unwrap_or("");
            let has_aria_label = img.value().attr("aria-label").is_some();
            let has_aria_labelledby = img.value().attr("aria-labelledby").is_some();
            let is_decorative = img.value().attr("role")
                .map(|r| r == "presentation")
                .unwrap_or(false);

            if !has_alt && !has_aria_label && !has_aria_labelledby && !is_decorative {
                issues.push(Issue::new(
                    "Missing Alt Text",
                    Some(&img.html()),
                    Some("Add alt attribute describing the image, or use alt='' for decorative images"),
                    Severity::Warning,
                    Some("<img src='image.jpg' alt='Description of image'>"),
                    Category::Accessibility,
                ));
            } else if has_alt && alt_value.trim().is_empty() && !is_decorative {
                issues.push(Issue::new(
                    "Empty Alt Text",
                    Some(&img.html()),
                    Some("Provide meaningful alt text or use alt='' for decorative images"),
                    Severity::Warning,
                    Some("<img src='image.jpg' alt='Description of image'>"),
                    Category::Accessibility,
                ));
            }
        }

        issues
    }

    /// Check form labels
    fn check_labels(&self, document: &Html) -> Vec<Issue> {
        let mut issues = Vec::new();
        let input_selector = Selector::parse("input, select, textarea").unwrap();

        for input in document.select(&input_selector) {
            let input_type = input.value().attr("type").unwrap_or("text");
            let id = input.value().attr("id");

            // Skip certain input types
            if matches!(input_type, "hidden" | "submit" | "button" | "image") {
                continue;
            }

            let has_label = if let Some(id) = id {
                let label_selector = format!("label[for=\"{}\"]", id);
                document.select(&Selector::parse(&label_selector).unwrap()).next().is_some()
            } else {
                false
            };

            let has_aria_label = input.value().attr("aria-label").is_some();
            let has_aria_labelledby = input.value().attr("aria-labelledby").is_some();
            let is_wrapped_in_label = input.parent()
                .and_then(|p| p.value().as_element())
                .map(|e| e.name() == "label")
                .unwrap_or(false);

            if !has_label && !has_aria_label && !has_aria_labelledby && !is_wrapped_in_label {
                issues.push(Issue::new(
                    "Missing Label",
                    Some(&input.html()),
                    Some("Add label with for attribute, aria-label, or wrap in label element"),
                    Severity::Warning,
                    Some("<label for='inputId'>Label text</label><input id='inputId' type='text'>"),
                    Category::Accessibility,
                ));
            }
        }

        issues
    }

    /// Check for page title
    fn check_title(&self, document: &Html) -> Vec<Issue> {
        let mut issues = Vec::new();
        let title_selector = Selector::parse("title").unwrap();

        if document.select(&title_selector).next().is_none() {
            issues.push(Issue::new(
                "Missing Title",
                Some("<head>...</head>"),
                Some("Add <title> tag in <head>"),
                Severity::Error,
                Some("<title>Page Title</title>"),
                Category::Accessibility,
            ));
        }

        issues
    }

    /// Check heading hierarchy
    fn check_heading_hierarchy(&self, document: &Html) -> Vec<Issue> {
        let mut issues = Vec::new();
        let heading_selector = Selector::parse("h1, h2, h3, h4, h5, h6").unwrap();

        let headings: Vec<_> = document.select(&heading_selector).collect();
        if headings.len() <= 1 {
            return issues;
        }

        let mut last_level = 0;
        let mut skip_count = 0;

        for heading in &headings {
            let tag_name = heading.value().name();
            let level = tag_name.chars().nth(1)
                .and_then(|c| c.to_digit(10))
                .unwrap_or(1) as i32;

            if last_level > 0 && level > last_level + 1 {
                skip_count += 1;
                if skip_count <= 3 {
                    issues.push(Issue::new(
                        "Heading Hierarchy Skip",
                        Some(&heading.html()),
                        Some("Consider using intermediate heading levels for better structure"),
                        Severity::Info,
                        Some(&format!("Use h{} before jumping to h{}", last_level + 1, level)),
                        Category::Accessibility,
                    ));
                }
            }
            last_level = level;
        }

        // Check for missing H1
        let has_h1 = headings.iter().any(|h| h.value().name() == "h1");
        if !has_h1 {
            issues.push(Issue::new(
                "Missing H1 Heading",
                Some("<body>"),
                Some("Add an h1 element as the main page heading"),
                Severity::Warning,
                Some("<h1>Main Page Title</h1>"),
                Category::Accessibility,
            ));
        }

        issues
    }

    /// Check color contrast (simplified - only inline styles)
    fn check_color_contrast(&self, document: &Html) -> Vec<Issue> {
        let mut issues = Vec::new();
        let style_selector = Selector::parse("[style]").unwrap();

        for element in document.select(&style_selector) {
            if let Some(style) = element.value().attr("style") {
                // This is a simplified check - real implementation would parse CSS properly
                if style.contains("color:") && style.contains("background-color:") {
                    issues.push(Issue::new(
                        "Color Contrast Check Needed",
                        Some(&element.html()),
                        Some("Verify text meets WCAG contrast requirements (4.5:1 minimum)"),
                        Severity::Info,
                        Some("Use a color contrast checker tool"),
                        Category::Accessibility,
                    ));
                }
            }
        }

        issues
    }

    /// Check language attributes
    fn check_lang_attributes(&self, document: &Html) -> Vec<Issue> {
        let mut issues = Vec::new();
        let html_selector = Selector::parse("html").unwrap();

        if let Some(html) = document.select(&html_selector).next() {
            if html.value().attr("lang").is_none() {
                issues.push(Issue::new(
                    "Missing lang Attribute",
                    Some("<html>"),
                    Some("Add lang attribute to html tag"),
                    Severity::Warning,
                    Some("<html lang=\"en\">"),
                    Category::Accessibility,
                ));
            }
        }

        issues
    }

    /// Check best practices
    fn check_best_practices(&self, document: &Html) -> Vec<Issue> {
        let mut issues = Vec::new();
        let favicon_selector = Selector::parse("link[rel*='icon']").unwrap();

        if document.select(&favicon_selector).next().is_none() {
            issues.push(Issue::new(
                "Missing Favicon",
                Some("<head>"),
                Some("Add favicon link"),
                Severity::Info,
                Some("<link rel=\"icon\" href=\"favicon.ico\">"),
                Category::BestPractices,
            ));
        }

        issues
    }

    /// Check performance issues
    fn check_performance(&self, load_result: &PageLoadResult) -> Vec<Issue> {
        let mut issues = Vec::new();

        if load_result.load_time > 3.0 {
            issues.push(Issue::new(
                "Slow Page Load",
                Some(&format!("Load time: {:.2}s", load_result.load_time)),
                Some("Optimize images, minify assets, use caching"),
                Severity::Warning,
                Some("Use lazy loading, compress assets"),
                Category::Performance,
            ));
        }

        if load_result.request_count > 50 {
            issues.push(Issue::new(
                "High Request Count",
                Some(&format!("Requests: {}", load_result.request_count)),
                Some("Combine files, use sprites, reduce dependencies"),
                Severity::Warning,
                Some("Bundle CSS/JS files"),
                Category::Performance,
            ));
        }

        issues
    }

    /// Check environmental impact
    fn check_environment(&self, _document: &Html, load_result: &PageLoadResult) -> Vec<Issue> {
        let mut issues = Vec::new();

        if load_result.page_size > 5 * 1024 * 1024 {
            issues.push(Issue::new(
                "Large Page Size",
                Some(&format!("Size: {:.2} MB", load_result.page_size as f64 / (1024.0 * 1024.0))),
                Some("Optimize images, minify assets, remove unused code"),
                Severity::Warning,
                Some("Compress images, use WebP format"),
                Category::Environment,
            ));
        }

        issues
    }

    /// Detect CDN usage
    fn detect_cdn_usage(&self, document: &Html) -> bool {
        let script_selector = Selector::parse("script[src]").unwrap();
        let link_selector = Selector::parse("link[href]").unwrap();

        let cdn_domains = [
            "cdn.jsdelivr.net",
            "cdnjs.cloudflare.com",
            "unpkg.com",
            "ajax.googleapis.com",
            "code.jquery.com",
            "stackpath.bootstrapcdn.com",
            "maxcdn.bootstrapcdn.com"
        ];

        for script in document.select(&script_selector) {
            if let Some(src) = script.value().attr("src") {
                if cdn_domains.iter().any(|domain| src.contains(domain)) {
                    return true;
                }
            }
        }

        for link in document.select(&link_selector) {
            if let Some(href) = link.value().attr("href") {
                if cdn_domains.iter().any(|domain| href.contains(domain)) {
                    return true;
                }
            }
        }

        false
    }

    /// Calculate environmental impact
    fn calculate_environmental_impact(&self, report: &mut Report) {
        let data_transfer_gb = report.page_size as f64 / (1024.0 * 1024.0 * 1024.0);
        let base_energy = 0.01;
        let data_energy = data_transfer_gb * 200.0;
        let request_energy = report.request_count as f64 * 0.0001;

        let mut total_energy = base_energy + data_energy + request_energy;

        if report.uses_cdn {
            total_energy *= 0.7; // 30% reduction with CDN
        }

        report.energy_consumption_kwh = total_energy;
        report.co2_emissions_grams = total_energy * 500.0; // 0.5 kg CO₂ per kWh

        report.environmental_rating = if report.co2_emissions_grams < 10.0 {
            "Eco"
        } else if report.co2_emissions_grams < 50.0 {
            "Moderate"
        } else {
            "High Impact"
        }.to_string();
    }

    /// Export report to text format
    pub fn export_to_text(&self, report: &Report) -> String {
        let mut output = String::new();

        output.push_str("Web Accessibility Report\n");
        output.push_str(&format!("Website: {}\n", report.website_url.as_deref().unwrap_or("N/A")));
        output.push_str(&format!("Total Issues: {}\n", report.total_issues));
        output.push_str(&format!("Errors: {}, Warnings: {}, Info: {}\n",
            report.error_count, report.warning_count, report.info_count));
        output.push_str(&format!("Accessibility Score: {}/100\n", report.accessibility_score));
        output.push_str(&format!("Compliance Status: {}\n", report.compliance_status));

        if report.page_load_time > 0.0 {
            output.push_str(&format!("Page Load Time: {:.2}s\n", report.page_load_time));
            output.push_str(&format!("Request Count: {}\n", report.request_count));
            output.push_str(&format!("Page Size: {:.2} MB\n", report.page_size as f64 / (1024.0 * 1024.0)));
            output.push_str(&format!("Uses CDN: {}\n", if report.uses_cdn { "Yes" } else { "No" }));
            output.push_str(&format!("Energy Consumption: {:.4} kWh\n", report.energy_consumption_kwh));
            output.push_str(&format!("CO₂ Emissions: {:.2} grams\n", report.co2_emissions_grams));
            output.push_str(&format!("Environmental Rating: {}\n", report.environmental_rating));
        }

        output.push_str("\nIssues:\n");
        for issue in &report.issues {
            output.push_str(&format!("\nCategory: {:?}\n", issue.category));
            output.push_str(&format!("Type: {}\n", issue.issue_type));
            output.push_str(&format!("Severity: {:?}\n", issue.severity));
            if let Some(snippet) = &issue.element_snippet {
                output.push_str(&format!("Element: {}\n", snippet));
            }
            if let Some(fix) = &issue.suggested_fix {
                output.push_str(&format!("Fix: {}\n", fix));
            }
            if let Some(example) = &issue.fix_example {
                output.push_str(&format!("Example: {}\n", example));
            }
            output.push_str("---\n");
        }

        output
    }
}

impl Default for AccessibilityChecker {
    fn default() -> Self {
        Self::new()
    }
}