const axios = require('axios');
const cheerio = require('cheerio');
const puppeteer = require('puppeteer');

/**
 * AXIS-CORE SDK for programmatic web accessibility checking (JavaScript/Node.js)
 */
class AxisCore {
    constructor() {
        this.version = '1.0.0';
    }

    /**
     * Check accessibility of a web page by URL
     * @param {string} url - The URL of the web page to check
     * @returns {Promise<Object>} Accessibility report
     */
    async checkUrl(url) {
        try {
            // Launch browser for dynamic content
            const browser = await puppeteer.launch({
                headless: true,
                args: ['--no-sandbox', '--disable-setuid-sandbox']
            });

            const page = await browser.newPage();
            await page.setViewport({ width: 1280, height: 720 });

            // Navigate and wait for content
            await page.goto(url, { waitUntil: 'networkidle0', timeout: 30000 });

            // Get the HTML content
            const content = await page.content();
            await browser.close();

            // Parse and check accessibility
            return this.checkHtml(content, url);
        } catch (error) {
            // Fallback to simple HTTP request
            try {
                const response = await axios.get(url, { timeout: 10000 });
                return this.checkHtml(response.data, url);
            } catch (httpError) {
                throw new Error(`Failed to load URL: ${error.message}`);
            }
        }
    }

    /**
     * Check accessibility of HTML content
     * @param {string} htmlContent - The HTML content to check
     * @param {string} baseUrl - Optional base URL for resolving relative links
     * @returns {Object} Accessibility report
     */
    checkHtml(htmlContent, baseUrl = '') {
        const $ = cheerio.load(htmlContent);
        const issues = [];

        // Check for missing alt text
        $('img').each((i, elem) => {
            const alt = $(elem).attr('alt');
            const src = $(elem).attr('src') || '';
            const ariaLabel = $(elem).attr('aria-label');
            const ariaLabelledBy = $(elem).attr('aria-labelledby');
            const role = $(elem).attr('role');

            if (!alt && !ariaLabel && !ariaLabelledBy && role !== 'presentation') {
                issues.push({
                    type: 'Missing Alt Text',
                    elementSnippet: $.html(elem).substring(0, 100) + '...',
                    suggestedFix: 'Add alt attribute describing the image',
                    severity: 'Warning',
                    category: 'Accessibility'
                });
            }
        });

        // Check for missing labels
        $('input, select, textarea').each((i, elem) => {
            const type = $(elem).attr('type') || '';
            if (['hidden', 'submit', 'button', 'image'].includes(type.toLowerCase())) {
                return; // Skip these types
            }

            const id = $(elem).attr('id');
            const ariaLabel = $(elem).attr('aria-label');
            const ariaLabelledBy = $(elem).attr('aria-labelledby');
            const hasLabel = id && $(`label[for="${id}"]`).length > 0;
            const isWrappedInLabel = $(elem).parent().is('label');

            if (!hasLabel && !ariaLabel && !ariaLabelledBy && !isWrappedInLabel) {
                issues.push({
                    type: 'Missing Label',
                    elementSnippet: $.html(elem).substring(0, 100) + '...',
                    suggestedFix: 'Add label with for attribute or aria-label',
                    severity: 'Warning',
                    category: 'Accessibility'
                });
            }
        });

        // Check for missing title
        if (!$('title').length || !$('title').text().trim()) {
            issues.push({
                type: 'Missing Title',
                elementSnippet: '<head>...</head>',
                suggestedFix: 'Add <title> tag in <head>',
                severity: 'Error',
                category: 'Accessibility'
            });
        }

        // Check heading hierarchy
        const headings = [];
        $('h1, h2, h3, h4, h5, h6').each((i, elem) => {
            const level = parseInt(elem.tagName.substring(1));
            headings.push({ level, element: elem });
        });

        let lastLevel = 0;
        headings.forEach((heading, index) => {
            if (index > 0 && heading.level > lastLevel + 1) {
                issues.push({
                    type: 'Heading Hierarchy Skip',
                    elementSnippet: $.html(heading.element).substring(0, 50) + '...',
                    suggestedFix: 'Use intermediate heading levels',
                    severity: 'Info',
                    category: 'Accessibility'
                });
            }
            lastLevel = heading.level;
        });

        // Check for missing H1
        if (!$('h1').length) {
            issues.push({
                type: 'Missing H1 Heading',
                elementSnippet: '<body>',
                suggestedFix: 'Add an h1 element as the main page heading',
                severity: 'Warning',
                category: 'Accessibility'
            });
        }

        // Check for missing lang attribute
        const htmlLang = $('html').attr('lang');
        if (!htmlLang) {
            issues.push({
                type: 'Missing lang Attribute',
                elementSnippet: '<html>',
                suggestedFix: 'Add lang attribute to html tag',
                severity: 'Warning',
                category: 'Accessibility'
            });
        }

        // Check for missing viewport meta
        const hasViewport = $('meta[name="viewport"]').length > 0;
        if (!hasViewport) {
            issues.push({
                type: 'Missing Viewport Meta',
                elementSnippet: '<head>',
                suggestedFix: 'Add viewport meta tag for mobile',
                severity: 'Warning',
                category: 'Accessibility'
            });
        }

        // Check Target Size (WCAG 2.5.8)
        const interactiveElements = $('a, button, input, select, textarea');
        let hasInteractiveElements = false;
        interactiveElements.each((i, elem) => {
            const type = $(elem).attr('type') || '';
            if (type.toLowerCase() === 'hidden') {
                return; // Skip hidden
            }
            hasInteractiveElements = true;
            return false; // Break
        });

        if (hasInteractiveElements) {
            issues.push({
                type: 'Target Size Verification (WCAG 2.2)',
                elementSnippet: 'Interactive Elements',
                suggestedFix: 'Ensure all clickable targets are at least 24x24 CSS pixels',
                severity: 'Info',
                category: 'Accessibility'
            });
        }

        // Check Redundant Entry (WCAG 3.3.7)
        $('input[type="text"], input[type="email"], input:not([type])').each((i, elem) => {
            const name = ($(elem).attr('name') || '').toLowerCase();
            const id = ($(elem).attr('id') || '').toLowerCase();
            const autocomplete = $(elem).attr('autocomplete');
            
            if (name.includes('name') || name.includes('email') || name.includes('phone') || name.includes('address') ||
                id.includes('name') || id.includes('email') || id.includes('phone') || id.includes('address')) {
                if (!autocomplete) {
                    issues.push({
                        type: 'Redundant Entry Risk (WCAG 2.2)',
                        elementSnippet: $.html(elem).substring(0, 100) + '...',
                        suggestedFix: 'Add autocomplete attribute to fields requesting user data',
                        severity: 'Warning',
                        category: 'Accessibility'
                    });
                }
            }
        });

        // Deduplicate issues: group by type and category, preserving all unique element instances
        const deduplicateIssues = (issues) => {
            const grouped = {};
            issues.forEach(issue => {
                const key = `${issue.type}|${issue.category}`;
                if (!grouped[key]) {
                    grouped[key] = { 
                        ...issue, 
                        count: 1,
                        elementInstances: [issue.elementSnippet] // Store all unique elements
                    };
                } else {
                    grouped[key].count++;
                    // Add unique element to instances list
                    const element = issue.elementSnippet;
                    if (!grouped[key].elementInstances.includes(element)) {
                        grouped[key].elementInstances.push(element);
                    }
                }
            });
            return Object.values(grouped);
        };

        const deduplicatedIssues = deduplicateIssues(issues);

        // Calculate scores
        const accessibilityIssues = deduplicatedIssues.filter(i => i.category === 'Accessibility');
        const errorCount = accessibilityIssues.filter(i => i.severity === 'Error').length;
        const warningCount = accessibilityIssues.filter(i => i.severity === 'Warning').length;

        // Simple scoring algorithm: info issues don't affect score
        let accessibilityScore = 100;
        accessibilityScore -= errorCount * 15;
        accessibilityScore -= warningCount * 5;
        // infoCount * 0 = no penalty
        accessibilityScore = Math.max(0, Math.min(100, accessibilityScore));

        // Determine compliance status
        let complianceStatus = 'Not Compliant';
        if (accessibilityScore >= 95) complianceStatus = 'Fully Compliant';
        else if (accessibilityScore >= 80) complianceStatus = 'Mostly Compliant';
        else if (accessibilityScore >= 60) complianceStatus = 'Partially Compliant';

        return {
            issues: deduplicatedIssues,
            totalIssues: issues.length,
            errorCount: errorCount,
            warningCount: warningCount,
            infoCount: issues.filter(i => i.severity === 'Info').length,
            accessibilityScore: accessibilityScore,
            seoScore: 0, // Not implemented in basic version
            performanceScore: 0, // Not implemented in basic version
            bestPracticesScore: 0, // Not implemented in basic version
            environmentScore: 0, // Not implemented in basic version
            safetyScore: 0, // Not implemented in basic version
            complianceStatus: complianceStatus,
            websiteUrl: baseUrl,
            pageLoadTime: 0, // Not measured in basic version
            requestCount: 1, // Basic estimate
            pageSize: htmlContent.length,
            energyConsumptionKwh: 0, // Not calculated
            co2EmissionsGrams: 0, // Not calculated
            environmentalRating: 'Unknown'
        };
    }

    /**
     * Export report to text format
     * @param {Object} report - The accessibility report
     * @returns {string} Formatted text report
     */
    exportToText(report) {
        let text = 'AXIS-CORE Accessibility Report\n';
        text += '=' .repeat(40) + '\n\n';
        text += `Website: ${report.websiteUrl || 'N/A'}\n`;
        text += `Total Issues: ${report.totalIssues}\n`;
        text += `Errors: ${report.errorCount}, Warnings: ${report.warningCount}, Info: ${report.infoCount}\n`;
        text += `Accessibility Score: ${report.accessibilityScore}/100\n`;
        text += `Compliance Status: ${report.complianceStatus}\n\n`;

        if (report.issues.length > 0) {
            text += 'ISSUES FOUND:\n';
            text += '-'.repeat(20) + '\n\n';

            report.issues.forEach((issue, index) => {
                text += `${index + 1}. ${issue.type} (${issue.severity})\n`;
                text += `   Category: ${issue.category}\n`;
                text += `   Total Occurrences: ${issue.count}\n`;
                
                // Show all unique instances instead of hiding them
                const instances = issue.elementInstances && issue.elementInstances.length > 0 
                    ? issue.elementInstances 
                    : [issue.elementSnippet];
                
                instances.forEach((element, i) => {
                    text += `\n   Instance ${i + 1}:\n`;
                    text += `   Element: ${element}\n`;
                    text += `   Fix: ${issue.suggestedFix}\n`;
                });
                text += '\n';
            });
        } else {
            text += 'No accessibility issues found!\n';
        }

        return text;
    }

    /**
     * Get SDK version
     * @returns {string} Version string
     */
    getVersion() {
        return this.version;
    }
}

module.exports = AxisCore;