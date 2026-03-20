use crate::models::PageLoadResult;
use scraper::Html;
use std::time::Instant;

/// HTML parser for loading and parsing web content
pub struct HtmlParser {
    client: reqwest::Client,
}

impl HtmlParser {
    /// Create a new HTML parser
    pub fn new() -> Self {
        let client = reqwest::Client::builder()
            .user_agent("AXIS-CORE/1.0 (https://github.com/ABHIRAM-CREATOR06/Acess1)")
            .timeout(std::time::Duration::from_secs(30))
            .build()
            .expect("Failed to create HTTP client");

        Self { client }
    }

    /// Load HTML content from a URL
    pub async fn load_from_url(&self, url: &str) -> Result<PageLoadResult, Box<dyn std::error::Error>> {
        let start_time = Instant::now();

        // Make the HTTP request
        let response = self.client.get(url).send().await?;
        let status = response.status();

        if !status.is_success() {
            return Err(format!("HTTP request failed with status: {}", status).into());
        }

        // Check for caching headers before consuming the response
        let has_caching_headers = response.headers()
            .get("cache-control")
            .or_else(|| response.headers().get("expires"))
            .is_some();

        // Get the content
        let content = response.text().await?;
        let load_time = start_time.elapsed().as_secs_f64();

        // Parse HTML
        let document = Html::parse_document(&content);

        // Basic analysis (simplified compared to full implementation)
        let page_size = content.len() as u64;
        let request_count = 1; // Simplified - would need more complex analysis for full count

        // Check for compression (basic check)
        let is_compressed = content.contains("gzip") || content.contains("deflate");

        Ok(PageLoadResult::new(
            document,
            load_time,
            request_count,
            page_size,
            is_compressed,
            has_caching_headers,
            Some(url),
        ))
    }

    /// Parse HTML content directly
    pub fn parse_html(&self, html: &str, base_url: Option<&str>) -> PageLoadResult {
        let document = Html::parse_document(html);
        let page_size = html.len() as u64;

        PageLoadResult::new(
            document,
            0.0, // No load time for direct parsing
            1,   // Single "request"
            page_size,
            false, // Assume not compressed
            false, // Assume no caching headers
            base_url,
        )
    }
}

impl Default for HtmlParser {
    fn default() -> Self {
        Self::new()
    }
}