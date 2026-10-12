using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using WebAccessibilityChecker.Models;

namespace WebAccessibilityChecker.Services
{
    public class AccessibilityChecker
    {
        /// <summary>
        /// Deduplicates issues by grouping identical issue types together.
        /// Groups by: Type and Category.
        /// Preserves all unique element instances for detailed reporting.
        /// </summary>
        private List<Issue> DeduplicateIssues(List<Issue> issues)
        {
            var grouped = issues
                .GroupBy(i => new { i.Type, i.Category })
                .Select(g =>
                {
                    var firstIssue = g.First();
                    firstIssue.Count = g.Count();
                    // Collect all unique element instances instead of hiding them
                    firstIssue.ElementInstances = g.Select(i => i.ElementSnippet ?? "").Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
                    // Set the first element as the main element snippet for backward compatibility
                    firstIssue.ElementSnippet = firstIssue.ElementInstances.FirstOrDefault() ?? firstIssue.ElementSnippet;
                    firstIssue.WcagGuideline ??= g.FirstOrDefault(i => !string.IsNullOrEmpty(i.WcagGuideline))?.WcagGuideline;
                    firstIssue.Impact ??= g.FirstOrDefault(i => !string.IsNullOrEmpty(i.Impact))?.Impact;
                    return firstIssue;
                })
                .ToList();

            return grouped;
        }

        public Report CheckAccessibility(PageLoadResult loadResult)
        {
            var report = new Report();
            var doc = loadResult.Document;

            // Set basic environmental data
            report.PageSize = loadResult.PageSize;
            report.RequestCount = loadResult.RequestCount;
            report.PageLoadTime = loadResult.LoadTime;

            // Check for CDN usage (simplified detection)
            report.UsesCDN = DetectCDNUsage(doc);

            // Calculate environmental impact
            CalculateEnvironmentalImpact(report);

            report.Issues.AddRange(CheckAltText(doc));
            report.Issues.AddRange(CheckLabels(doc));
            report.Issues.AddRange(CheckTitle(doc));
            report.Issues.AddRange(CheckHeadingHierarchy(doc));
            report.Issues.AddRange(CheckColorContrast(doc));
            report.Issues.AddRange(CheckEyeComfort(doc));
            report.Issues.AddRange(CheckAriaAttributes(doc));
            report.Issues.AddRange(CheckLangAttributes(doc));
            report.Issues.AddRange(CheckWebXRSupport(doc));
            report.Issues.AddRange(CheckBestPractices(doc));
            report.Issues.AddRange(CheckMobileResponsiveness(doc));
            report.Issues.AddRange(CheckDarkModeSupport(doc));
            report.Issues.AddRange(CheckEnvironment(doc, loadResult));
            report.Issues.AddRange(CheckSafety(doc, loadResult, report.WebsiteUrl));
            report.Issues.AddRange(CheckTargetSize(doc));
            report.Issues.AddRange(CheckInputPurposeAutocomplete(doc));
            report.Issues.AddRange(CheckEmptyLinks(doc));
            report.Issues.AddRange(CheckEmptyButtons(doc));
            report.Issues.AddRange(CheckDuplicateIds(doc));
            report.Issues.AddRange(CheckViewportScalability(doc));
            report.Issues.AddRange(CheckLandmarks(doc));
            report.Issues.AddRange(CheckTableAccessibility(doc));
            report.Issues.AddRange(CheckAudioVideoAutoplay(doc));

            // Add performance issues
            report.Issues.AddRange(CheckPerformance(loadResult));

            // Enrich issue metadata with WCAG guidelines and impact
            EnrichIssueMetadata(report.Issues);

            // Deduplicate issues before calculating score
            report.Issues = DeduplicateIssues(report.Issues);

            // Calculate scores (aligned with Lighthouse-style scoring)
            var accessibilityIssues = report.Issues.Where(i => i.Category == Category.Accessibility).ToList();
            int errorCount = accessibilityIssues.Count(i => i.SeverityLevel == Severity.Error);
            int warningCount = accessibilityIssues.Count(i => i.SeverityLevel == Severity.Warning);
            int infoCount = accessibilityIssues.Count(i => i.SeverityLevel == Severity.Info);

            // Scoring formula: info issues do not affect the score
            // baseScore - (errors * 15) - (warnings * 5) - (info * 0)
            int score = 100 - (errorCount * 15) - (warningCount * 5);

            report.AccessibilityScore = (int)Math.Max(0, Math.Min(100, score));

            // Calculate performance score
            var performanceIssues = report.Issues.Where(i => i.Category == Category.Performance).ToList();
            int perfPenalty = performanceIssues.Count(i => i.SeverityLevel == Severity.Error) * 10 +
                              performanceIssues.Count(i => i.SeverityLevel == Severity.Warning) * 5 +
                              performanceIssues.Count(i => i.SeverityLevel == Severity.Info) * 1;
            report.PerformanceScore = Math.Max(0, 100 - perfPenalty);

            // Calculate environment score
            var environmentIssues = report.Issues.Where(i => i.Category == Category.Environment).ToList();
            int envPenalty = environmentIssues.Count(i => i.SeverityLevel == Severity.Error) * 10 +
                             environmentIssues.Count(i => i.SeverityLevel == Severity.Warning) * 5 +
                             environmentIssues.Count(i => i.SeverityLevel == Severity.Info) * 1;
            report.EnvironmentScore = Math.Max(0, 100 - envPenalty);

            // Calculate safety score
            var safetyIssues = report.Issues.Where(i => i.Category == Category.Safety).ToList();
            int safetyPenalty = safetyIssues.Count(i => i.SeverityLevel == Severity.Error) * 10 +
                                safetyIssues.Count(i => i.SeverityLevel == Severity.Warning) * 5 +
                                safetyIssues.Count(i => i.SeverityLevel == Severity.Info) * 1;
            report.SafetyScore = Math.Max(0, 100 - safetyPenalty);

            // Overall automated scan status - automated scans cannot certify conformance
            if (report.AccessibilityScore >= 95) report.ComplianceStatus = "Low Automated Issues (Manual Review Required)";
            else if (report.AccessibilityScore >= 80) report.ComplianceStatus = "Minor Violations (Manual Review Required)";
            else if (report.AccessibilityScore >= 60) report.ComplianceStatus = "Partial Conformance (Remediation Needed)";
            else report.ComplianceStatus = "Critical Violations Found (Action Required)";

            return report;
        }

        private bool DetectCDNUsage(HtmlDocument doc)
        {
            var scripts = doc.DocumentNode.SelectNodes("//script[@src]");
            var links = doc.DocumentNode.SelectNodes("//link[@href]");
            var cdnDomains = new[] { "cdn.jsdelivr.net", "cdnjs.cloudflare.com", "unpkg.com", "ajax.googleapis.com", "code.jquery.com", "stackpath.bootstrapcdn.com", "maxcdn.bootstrapcdn.com" };

            if (scripts != null)
            {
                foreach (var script in scripts)
                {
                    var src = script.Attributes["src"]?.Value;
                    if (!string.IsNullOrEmpty(src) && cdnDomains.Any(domain => src.Contains(domain)))
                        return true;
                }
            }

            if (links != null)
            {
                foreach (var link in links)
                {
                    var href = link.Attributes["href"]?.Value;
                    if (!string.IsNullOrEmpty(href) && cdnDomains.Any(domain => href.Contains(domain)))
                        return true;
                }
            }

            return false;
        }

        private void CalculateEnvironmentalImpact(Report report)
        {
            // Base energy consumption calculation
            // Formula based on research: ~0.2 kWh per GB of data transfer
            // Plus base consumption for device and network

            double dataTransferGB = report.PageSize / (1024.0 * 1024.0 * 1024.0); // Convert bytes to GB
            double baseEnergyKWh = 0.01; // Base energy for loading a page
            double dataEnergyKWh = dataTransferGB * 200; // 0.2 kWh per GB
            double requestEnergyKWh = report.RequestCount * 0.0001; // Small energy per request

            // CDN reduces energy (cached content)
            if (report.UsesCDN)
                dataEnergyKWh *= 0.7; // 30% reduction

            report.EnergyConsumptionKWh = baseEnergyKWh + dataEnergyKWh + requestEnergyKWh;

            // CO₂ calculation: Average 0.5 kg CO₂ per kWh (varies by region)
            report.CO2EmissionsGrams = report.EnergyConsumptionKWh * 500; // Convert to grams

            // Environmental rating
            if (report.CO2EmissionsGrams < 10) report.EnvironmentalRating = "Eco";
            else if (report.CO2EmissionsGrams < 50) report.EnvironmentalRating = "Moderate";
            else report.EnvironmentalRating = "High Impact";
        }

        public List<Issue> CheckPerformance(PageLoadResult loadResult)
        {
            var issues = new List<Issue>();
            if (loadResult.LoadTime > 3)
            {
                issues.Add(new Issue
                {
                    Type = "Slow Page Load",
                    ElementSnippet = $"Load time: {loadResult.LoadTime}s",
                    SuggestedFix = "Optimize images, minify CSS/JS, use caching",
                    SeverityLevel = Severity.Warning,
                    FixExample = "Use lazy loading, compress assets",
                    Category = Category.Performance
                });
            }
            if (loadResult.RequestCount > 50)
            {
                issues.Add(new Issue
                {
                    Type = "High Request Count",
                    ElementSnippet = $"Requests: {loadResult.RequestCount}",
                    SuggestedFix = "Combine files, use sprites, reduce dependencies",
                    SeverityLevel = Severity.Warning,
                    FixExample = "Bundle CSS/JS files",
                    Category = Category.Performance
                });
            }
            if (!loadResult.IsCompressed)
            {
                issues.Add(new Issue
                {
                    Type = "No Compression",
                    ElementSnippet = "Content-Encoding header missing",
                    SuggestedFix = "Enable gzip compression on server",
                    SeverityLevel = Severity.Info,
                    FixExample = "Configure server for gzip",
                    Category = Category.Performance
                });
            }
            if (!loadResult.HasCachingHeaders)
            {
                issues.Add(new Issue
                {
                    Type = "No Caching Headers",
                    ElementSnippet = "Cache-Control or Expires missing",
                    SuggestedFix = "Add caching headers for static assets",
                    SeverityLevel = Severity.Info,
                    FixExample = "Cache-Control: max-age=31536000",
                    Category = Category.Performance
                });
            }
            return issues;
        }

        private List<Issue> CheckAltText(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var imgs = doc.DocumentNode.SelectNodes("//img");
            if (imgs != null)
            {
                foreach (var img in imgs)
                {
                    // Check if image has alt attribute
                    bool hasAlt = img.Attributes.Contains("alt");
                    string altValue = hasAlt ? img.Attributes["alt"].Value : "";

                    // Check for aria-label as alternative
                    bool hasAriaLabel = img.Attributes.Contains("aria-label") && !string.IsNullOrEmpty(img.Attributes["aria-label"].Value);
                    bool hasAriaLabelledBy = img.Attributes.Contains("aria-labelledby") && !string.IsNullOrEmpty(img.Attributes["aria-labelledby"].Value);

                    // Check if it's a decorative/presentational image
                    bool isDecorative = img.Attributes.Contains("role") && img.Attributes["role"].Value == "presentation";
                    bool hasEmptyAlt = hasAlt && string.IsNullOrEmpty(altValue.Trim());

                    // Only flag as issue if:
                    // 1. No alt attribute at all AND no aria-label/labelledby AND not marked as decorative
                    // 2. Has alt but it's just whitespace (should be empty string for decorative)
                    if ((!hasAlt && !hasAriaLabel && !hasAriaLabelledBy && !isDecorative) ||
                        (hasAlt && string.IsNullOrWhiteSpace(altValue) && !isDecorative))
                    {
                        issues.Add(new Issue
                        {
                            Type = "Missing Alt Text",
                            ElementSnippet = img.OuterHtml,
                            SuggestedFix = "Add alt attribute describing the image, or use alt='' for decorative images",
                            SeverityLevel = Severity.Warning, // Changed from Error to Warning
                            FixExample = "<img src='image.jpg' alt='Description of image'>",
                            Category = Category.Accessibility
                        });
                    }
                }
            }
            return issues;
        }

        private List<Issue> CheckLabels(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var inputs = doc.DocumentNode.SelectNodes("//input | //select | //textarea");
            if (inputs != null)
            {
                foreach (var input in inputs)
                {
                    var id = input.Attributes["id"]?.Value;
                    var type = input.Attributes["type"]?.Value?.ToLower();

                    // Skip hidden, submit, button, and other input types that don't need labels
                    if (type == "hidden" || type == "submit" || type == "button" || type == "image")
                        continue;

                    // Check for associated label
                    bool hasLabel = false;
                    if (!string.IsNullOrEmpty(id))
                    {
                        var label = doc.DocumentNode.SelectSingleNode($"//label[@for='{id}']");
                        hasLabel = label != null;
                    }

                    // Check for aria-label
                    bool hasAriaLabel = input.Attributes.Contains("aria-label") &&
                                       !string.IsNullOrEmpty(input.Attributes["aria-label"].Value);

                    // Check for aria-labelledby
                    bool hasAriaLabelledBy = input.Attributes.Contains("aria-labelledby") &&
                                            !string.IsNullOrEmpty(input.Attributes["aria-labelledby"].Value);

                    // Check if input is inside a label element
                    bool isWrappedInLabel = input.ParentNode != null && input.ParentNode.Name == "label";

                    // Only flag as issue if no accessible label method is present
                    if (!hasLabel && !hasAriaLabel && !hasAriaLabelledBy && !isWrappedInLabel)
                    {
                        issues.Add(new Issue
                        {
                            Type = "Missing Label",
                            ElementSnippet = input.OuterHtml,
                            SuggestedFix = "Add label with for attribute, aria-label, or wrap in label element",
                            SeverityLevel = Severity.Warning, // Changed from Error to Warning
                            FixExample = "<label for='inputId'>Label text</label><input id='inputId' type='text'>",
                            Category = Category.Accessibility
                        });
                    }
                }
            }
            return issues;
        }

        private List<Issue> CheckTitle(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var title = doc.DocumentNode.SelectSingleNode("//title");
            if (title == null || string.IsNullOrEmpty(title.InnerText.Trim()))
            {
                issues.Add(new Issue
                {
                    Type = "Missing Title",
                    ElementSnippet = "<head>...</head>",
                    SuggestedFix = "Add <title> tag in <head>",
                    SeverityLevel = Severity.Error,
                    FixExample = "<title>Page Title</title>",
                    Category = Category.Accessibility
                });
            }
            return issues;
        }

        private List<Issue> CheckHeadingHierarchy(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var headings = doc.DocumentNode.SelectNodes("//h1 | //h2 | //h3 | //h4 | //h5 | //h6");
            if (headings != null && headings.Count > 1)
            {
                int lastLevel = 0;
                int skipCount = 0;
                foreach (var h in headings)
                {
                    int level = int.Parse(h.Name.Substring(1));

                    // Allow some flexibility - only flag if skipping more than 1 level AND it's not the first heading
                    if (lastLevel > 0 && level > lastLevel + 1)
                    {
                        skipCount++;
                        // Only report the first few skips to avoid spam
                        if (skipCount <= 3)
                        {
                            issues.Add(new Issue
                            {
                                Type = "Heading Hierarchy Skip",
                                ElementSnippet = h.OuterHtml,
                                SuggestedFix = "Consider using intermediate heading levels for better structure",
                                SeverityLevel = Severity.Info, // Changed from Warning to Info - heading skips are common and often acceptable
                                FixExample = $"Use h{lastLevel + 1} before jumping to h{level}",
                                Category = Category.Accessibility
                            });
                        }
                    }
                    lastLevel = level;
                }

                // Check for missing h1
                bool hasH1 = headings.Any(h => h.Name == "h1");
                if (!hasH1)
                {
                    issues.Add(new Issue
                    {
                        Type = "Missing H1 Heading",
                        ElementSnippet = "<body>",
                        SuggestedFix = "Add an h1 element as the main page heading",
                        SeverityLevel = Severity.Warning,
                        FixExample = "<h1>Main Page Title</h1>",
                        Category = Category.Accessibility
                    });
                }
            }
            return issues;
        }

        private List<Issue> CheckColorContrast(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var elements = doc.DocumentNode.SelectNodes("//*[@style]");
            if (elements != null)
            {
                foreach (var el in elements)
                {
                    var style = el.Attributes["style"]?.Value;
                    if (!string.IsNullOrEmpty(style))
                    {
                        var color = ExtractColor(style, "color");
                        var bgColor = ExtractColor(style, "background-color");
                        if (!string.IsNullOrEmpty(color) && !string.IsNullOrEmpty(bgColor))
                        {
                            var ratio = CalculateContrastRatio(color, bgColor);
                            if (ratio < 4.5)
                            {
                                issues.Add(new Issue
                                {
                                    Type = "Low Color Contrast (Inline Styles)",
                                    ElementSnippet = el.OuterHtml,
                                    SuggestedFix = "Increase contrast ratio to at least 4.5:1. Note: This check only covers inline styles; external CSS contrast should be verified manually.",
                                    SeverityLevel = Severity.Info, // Changed from Warning to Info since we can't check external CSS
                                    FixExample = "Use darker text on lighter background",
                                    Category = Category.Accessibility
                                });
                            }
                        }
                    }
                }
            }

            // Add a general note about CSS contrast checking limitation
            if (issues.Count == 0)
            {
                issues.Add(new Issue
                {
                    Type = "Color Contrast Check Limited",
                    ElementSnippet = "Note: Contrast checking is limited to inline styles only",
                    SuggestedFix = "For complete contrast analysis, manually check external CSS stylesheets",
                    SeverityLevel = Severity.Info,
                    FixExample = "Use browser dev tools or automated contrast checkers for CSS styles",
                    Category = Category.Accessibility
                });
            }

            return issues;
        }

        private List<Issue> CheckEyeComfort(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var elements = doc.DocumentNode.SelectNodes("//*[@style]");
            if (elements != null)
            {
                foreach (var el in elements)
                {
                    var style = el.Attributes["style"]?.Value;
                    if (!string.IsNullOrEmpty(style))
                    {
                        var fontSize = ExtractFontSize(style);
                        var lineHeight = ExtractLineHeight(style);
                        if (fontSize > 0 && fontSize < 14)
                        {
                            issues.Add(new Issue
                            {
                                Type = "Small Font Size (Inline Styles)",
                                ElementSnippet = el.OuterHtml,
                                SuggestedFix = "Consider increasing font size for better readability. Note: This only checks inline styles.",
                                SeverityLevel = Severity.Info, // Changed from Warning to Info
                                FixExample = "font-size: 16px;",
                                Category = Category.Accessibility
                            });
                        }
                        if (lineHeight > 0 && lineHeight < 1.4) // Relaxed from 1.5 to 1.4
                        {
                            issues.Add(new Issue
                            {
                                Type = "Tight Line Spacing (Inline Styles)",
                                ElementSnippet = el.OuterHtml,
                                SuggestedFix = "Consider increasing line height for better readability. Note: This only checks inline styles.",
                                SeverityLevel = Severity.Info,
                                FixExample = "line-height: 1.6;",
                                Category = Category.Accessibility
                            });
                        }
                    }
                }
            }

            // Add note about CSS limitations
            if (issues.Count == 0)
            {
                issues.Add(new Issue
                {
                    Type = "Typography Check Limited",
                    ElementSnippet = "Note: Font and spacing checks are limited to inline styles only",
                    SuggestedFix = "For complete typography analysis, manually check external CSS stylesheets",
                    SeverityLevel = Severity.Info,
                    FixExample = "Use browser dev tools to inspect computed styles",
                    Category = Category.Accessibility
                });
            }

            return issues;
        }

        private string? ExtractColor(string style, string property)
        {
            var start = style.IndexOf(property + ":");
            if (start == -1) return null;
            start += property.Length + 1;
            var end = style.IndexOf(";", start);
            if (end == -1) end = style.Length;
            var value = style.Substring(start, end - start).Trim();
            return value;
        }

        private double ExtractFontSize(string style)
        {
            var value = ExtractValue(style, "font-size");
            if (value.EndsWith("px"))
            {
                return double.Parse(value.Replace("px", ""));
            }
            return 16; // default
        }

        private double ExtractLineHeight(string style)
        {
            var value = ExtractValue(style, "line-height");
            if (double.TryParse(value, out var lh))
            {
                return lh;
            }
            return 1.2; // default
        }

        private string ExtractValue(string style, string property)
        {
            var start = style.IndexOf(property + ":");
            if (start == -1) return "";
            start += property.Length + 1;
            var end = style.IndexOf(";", start);
            if (end == -1) end = style.Length;
            return style.Substring(start, end - start).Trim();
        }

        private double CalculateContrastRatio(string color1, string color2)
        {
            var lum1 = GetLuminance(ParseColor(color1));
            var lum2 = GetLuminance(ParseColor(color2));
            var brighter = Math.Max(lum1, lum2);
            var darker = Math.Min(lum1, lum2);
            return (brighter + 0.05) / (darker + 0.05);
        }

        private (double r, double g, double b) ParseColor(string color)
        {
            if (color.StartsWith("#") && color.Length == 7)
            {
                var r = int.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber) / 255.0;
                var g = int.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber) / 255.0;
                var b = int.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber) / 255.0;
                return (r, g, b);
            }
            return (0, 0, 0); // default black
        }

        private List<Issue> CheckAriaAttributes(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var allElements = doc.DocumentNode.SelectNodes("//*");
            if (allElements != null)
            {
                foreach (var el in allElements)
                {
                    if (el.Attributes.Any(a => a.Name.StartsWith("aria-")))
                    {
                        // Check for invalid roles
                        var role = el.Attributes["role"]?.Value;
                        if (!string.IsNullOrEmpty(role))
                        {
                            var validRoles = new[] { "button", "checkbox", "dialog", "gridcell", "link", "listbox", "menuitem", "menuitemcheckbox", "menuitemradio", "option", "progressbar", "radio", "scrollbar", "searchbox", "slider", "spinbutton", "tab", "tabpanel", "textbox", "tooltip", "treeitem", "banner", "complementary", "contentinfo", "main", "navigation", "region", "search", "alert", "log", "marquee", "status", "timer", "alertdialog", "application", "article", "columnheader", "definition", "directory", "document", "group", "heading", "img", "list", "listitem", "math", "note", "presentation", "row", "rowgroup", "rowheader", "separator", "toolbar", "grid", "row", "tree", "treegrid" };
                            if (!validRoles.Contains(role))
                            {
                                issues.Add(new Issue
                                {
                                    Type = "Invalid ARIA Role",
                                    ElementSnippet = el.OuterHtml,
                                    SuggestedFix = "Use a valid ARIA role",
                                    SeverityLevel = Severity.Error,
                                    FixExample = "role=\"button\"",
                                    Category = Category.Accessibility
                                });
                            }
                        }

                        // Check for redundant ARIA
                        if (el.Name == "button" && el.Attributes.Contains("role") && el.Attributes["role"].Value == "button")
                        {
                            issues.Add(new Issue
                            {
                                Type = "Redundant ARIA Role",
                                ElementSnippet = el.OuterHtml,
                                SuggestedFix = "Remove redundant role attribute",
                                SeverityLevel = Severity.Warning,
                                FixExample = "<button>Click me</button>",
                                Category = Category.Accessibility
                            });
                        }

                        // Check aria-label
                        var ariaLabel = el.Attributes["aria-label"]?.Value;
                        if (string.IsNullOrEmpty(ariaLabel) && el.Attributes.Contains("aria-labelledby"))
                        {
                            var labelledBy = el.Attributes["aria-labelledby"]?.Value;
                            if (!string.IsNullOrEmpty(labelledBy))
                            {
                                try
                                {
                                    // Use a safer approach to find elements by ID
                                    var labelEl = doc.GetElementbyId(labelledBy);
                                    if (labelEl == null || string.IsNullOrEmpty(labelEl.InnerText.Trim()))
                                    {
                                        issues.Add(new Issue
                                        {
                                            Type = "Missing aria-labelledby Target",
                                            ElementSnippet = el.OuterHtml,
                                            SuggestedFix = "Ensure aria-labelledby points to an element with text",
                                            SeverityLevel = Severity.Error,
                                            FixExample = "<div id=\"label\">Label text</div><input aria-labelledby=\"label\">",
                                            Category = Category.Accessibility
                                        });
                                    }
                                }
                                catch
                                {
                                    // If there's an issue finding the element, still report it
                                    issues.Add(new Issue
                                    {
                                        Type = "Invalid aria-labelledby Reference",
                                        ElementSnippet = el.OuterHtml,
                                        SuggestedFix = "Ensure aria-labelledby contains a valid element ID",
                                        SeverityLevel = Severity.Error,
                                        FixExample = "<div id=\"label\">Label text</div><input aria-labelledby=\"label\">",
                                        Category = Category.Accessibility
                                    });
                                }
                            }
                        }
                    }
                }
            }
            return issues;
        }

        private List<Issue> CheckLangAttributes(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var html = doc.DocumentNode.SelectSingleNode("//html");
            if (html == null || !html.Attributes.Contains("lang"))
            {
                issues.Add(new Issue
                {
                    Type = "Missing lang Attribute",
                    ElementSnippet = "<html>",
                    SuggestedFix = "Add lang attribute to html tag",
                    SeverityLevel = Severity.Warning,
                    FixExample = "<html lang=\"en\">",
                    Category = Category.Accessibility
                });
            }
            else
            {
                var lang = html.Attributes["lang"].Value;
                if (lang.Contains("-"))
                {
                    var parts = lang.Split('-');
                    if (parts.Length == 2)
                    {
                        // Check RTL languages
                        var rtlLangs = new[] { "ar", "he", "fa", "ur", "yi" };
                        if (rtlLangs.Contains(parts[0].ToLower()))
                        {
                            // Could check for dir="rtl" but for now, just note
                            issues.Add(new Issue
                            {
                                Type = "RTL Language Detected",
                                ElementSnippet = html.OuterHtml,
                                SuggestedFix = "Ensure proper RTL support",
                                SeverityLevel = Severity.Info,
                                FixExample = "Consider dir=\"rtl\" if needed",
                                Category = Category.Accessibility
                            });
                        }
                    }
                }
            }
            return issues;
        }

        private List<Issue> CheckWebXRSupport(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var scripts = doc.DocumentNode.SelectNodes("//script");
            bool hasWebXR = false;
            if (scripts != null)
            {
                foreach (var script in scripts)
                {
                    var src = script.Attributes["src"]?.Value;
                    var content = script.InnerText;
                    if ((!string.IsNullOrEmpty(src) && src.Contains("webxr")) || content.Contains("navigator.xr"))
                    {
                        hasWebXR = true;
                        break;
                    }
                }
            }
            if (hasWebXR)
            {
                // Basic check: ensure some accessibility considerations
                issues.Add(new Issue
                {
                    Type = "WebXR Accessibility",
                    ElementSnippet = "<script> with WebXR",
                    SuggestedFix = "Add accessibility features for VR/AR content",
                    SeverityLevel = Severity.Info,
                    FixExample = "Consider audio cues, haptic feedback, etc.",
                    Category = Category.Accessibility
                });
            }
            return issues;
        }

        private List<Issue> CheckBestPractices(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var head = doc.DocumentNode.SelectSingleNode("//head");
            if (head != null)
            {
                var favicon = head.SelectSingleNode("//link[@rel='icon']");
                if (favicon == null)
                {
                    issues.Add(new Issue
                    {
                        Type = "Missing Favicon",
                        ElementSnippet = "<head>",
                        SuggestedFix = "Add favicon link",
                        SeverityLevel = Severity.Info,
                        FixExample = "<link rel=\"icon\" href=\"favicon.ico\">",
                        Category = Category.BestPractices
                    });
                }
            }
            // Check for HTTPS (but since we load via HTTP, hard to check)
            // Other best practices can be added
            return issues;
        }

        private List<Issue> CheckMobileResponsiveness(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var viewport = doc.DocumentNode.SelectSingleNode("//meta[@name='viewport']");
            if (viewport == null)
            {
                issues.Add(new Issue
                {
                    Type = "Missing Viewport Meta",
                    ElementSnippet = "<head>",
                    SuggestedFix = "Add viewport meta for mobile",
                    SeverityLevel = Severity.Warning,
                    FixExample = "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">",
                    Category = Category.Accessibility // Mobile is part of accessibility
                });
            }
            return issues;
        }

        private List<Issue> CheckDarkModeSupport(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var styles = doc.DocumentNode.SelectNodes("//style | //link[@rel='stylesheet']");
            bool hasDarkMode = false;
            if (styles != null)
            {
                foreach (var style in styles)
                {
                    var content = style.InnerText;
                    if (content.Contains("@media (prefers-color-scheme: dark)") || content.Contains("prefers-color-scheme"))
                    {
                        hasDarkMode = true;
                        break;
                    }
                }
            }
            if (!hasDarkMode)
            {
                issues.Add(new Issue
                {
                    Type = "No Dark Mode Support",
                    ElementSnippet = "<style> or <link>",
                    SuggestedFix = "Add dark mode styles",
                    SeverityLevel = Severity.Info,
                    FixExample = "@media (prefers-color-scheme: dark) { body { background: black; } }",
                    Category = Category.Accessibility
                });
            }
            return issues;
        }

        private List<Issue> CheckEnvironment(HtmlDocument doc, PageLoadResult loadResult)
        {
            var issues = new List<Issue>();

            // Check for large page size (environmental impact)
            if (loadResult.PageSize > 5 * 1024 * 1024) // 5MB
            {
                issues.Add(new Issue
                {
                    Type = "Large Page Size",
                    ElementSnippet = $"Size: {loadResult.PageSize / (1024.0 * 1024.0):F2} MB",
                    SuggestedFix = "Optimize images, minify assets, remove unused code",
                    SeverityLevel = Severity.Warning,
                    FixExample = "Compress images, use WebP format",
                    Category = Category.Environment
                });
            }

            // Check for excessive requests
            if (loadResult.RequestCount > 100)
            {
                issues.Add(new Issue
                {
                    Type = "High Network Requests",
                    ElementSnippet = $"Requests: {loadResult.RequestCount}",
                    SuggestedFix = "Bundle resources, use HTTP/2, reduce dependencies",
                    SeverityLevel = Severity.Warning,
                    FixExample = "Combine CSS/JS files, use sprites",
                    Category = Category.Environment
                });
            }

            // Check for unoptimized images (basic check)
            var imgs = doc.DocumentNode.SelectNodes("//img");
            if (imgs != null)
            {
                foreach (var img in imgs)
                {
                    var src = img.Attributes["src"]?.Value;
                    if (!string.IsNullOrEmpty(src) && (src.Contains(".jpg") || src.Contains(".png")) && !src.Contains("compressed") && !src.Contains("optimized"))
                    {
                        issues.Add(new Issue
                        {
                            Type = "Unoptimized Images",
                            ElementSnippet = img.OuterHtml,
                            SuggestedFix = "Use compressed images, modern formats (WebP/AVIF)",
                            SeverityLevel = Severity.Info,
                            FixExample = "Convert to WebP, enable compression",
                            Category = Category.Environment
                        });
                        break; // Only report once
                    }
                }
            }

            return issues;
        }

        private List<Issue> CheckSafety(HtmlDocument doc, PageLoadResult loadResult, string? url)
        {
            var issues = new List<Issue>();

            // Check for HTTPS (basic check - since we load via HTTP, this is limited)
            if (!string.IsNullOrEmpty(url) && url.StartsWith("http://"))
            {
                issues.Add(new Issue
                {
                    Type = "HTTP Instead of HTTPS",
                    ElementSnippet = $"URL: {url}",
                    SuggestedFix = "Implement HTTPS for secure connections",
                    SeverityLevel = Severity.Warning,
                    FixExample = "Get SSL certificate, redirect HTTP to HTTPS",
                    Category = Category.Safety
                });
            }

            // Check for mixed content (basic detection)
            var scripts = doc.DocumentNode.SelectNodes("//script[@src]");
            var links = doc.DocumentNode.SelectNodes("//link[@href]");
            var mixedContent = false;

            if (scripts != null)
            {
                foreach (var script in scripts)
                {
                    var src = script.Attributes["src"]?.Value;
                    if (!string.IsNullOrEmpty(src) && src.StartsWith("http://"))
                    {
                        mixedContent = true;
                        break;
                    }
                }
            }

            if (!mixedContent && links != null)
            {
                foreach (var link in links)
                {
                    var href = link.Attributes["href"]?.Value;
                    if (!string.IsNullOrEmpty(href) && href.StartsWith("http://"))
                    {
                        mixedContent = true;
                        break;
                    }
                }
            }

            if (mixedContent)
            {
                issues.Add(new Issue
                {
                    Type = "Mixed Content",
                    ElementSnippet = "HTTP resources on HTTPS page",
                    SuggestedFix = "Use HTTPS for all resources",
                    SeverityLevel = Severity.Error,
                    FixExample = "Update resource URLs to HTTPS",
                    Category = Category.Safety
                });
            }

            // Check for potentially unsafe practices
            var forms = doc.DocumentNode.SelectNodes("//form");
            if (forms != null)
            {
                foreach (var form in forms)
                {
                    var action = form.Attributes["action"]?.Value;
                    var method = form.Attributes["method"]?.Value?.ToLower();

                    if (string.IsNullOrEmpty(method) || method == "get")
                    {
                        issues.Add(new Issue
                        {
                            Type = "Form Uses GET Method",
                            ElementSnippet = form.OuterHtml,
                            SuggestedFix = "Use POST for sensitive data",
                            SeverityLevel = Severity.Info,
                            FixExample = "<form method=\"post\">",
                            Category = Category.Safety
                        });
                    }
                }
            }

            // Check for external links without security attributes
            var externalLinks = doc.DocumentNode.SelectNodes("//a[@href]");
            if (externalLinks != null)
            {
                foreach (var link in externalLinks)
                {
                    var href = link.Attributes["href"]?.Value;
                    if (!string.IsNullOrEmpty(href) && href.StartsWith("http") && !string.IsNullOrEmpty(url) && !href.Contains(new Uri(url).Host))
                    {
                        if (!link.Attributes.Contains("rel") || !link.Attributes["rel"].Value.Contains("noopener"))
                        {
                            issues.Add(new Issue
                            {
                                Type = "External Link Without Security",
                                ElementSnippet = link.OuterHtml,
                                SuggestedFix = "Add rel=\"noopener noreferrer\" to external links",
                                SeverityLevel = Severity.Warning,
                                FixExample = "<a href=\"...\" rel=\"noopener noreferrer\">",
                                Category = Category.Safety
                            });
                        }
                        break; // Only report once
                    }
                }
            }

            return issues;
        }

        private double GetLuminance((double r, double g, double b) color)
        {
            var r = color.r <= 0.03928 ? color.r / 12.92 : Math.Pow((color.r + 0.055) / 1.055, 2.4);
            var g = color.g <= 0.03928 ? color.g / 12.92 : Math.Pow((color.g + 0.055) / 1.055, 2.4);
            var b = color.b <= 0.03928 ? color.b / 12.92 : Math.Pow((color.b + 0.055) / 1.055, 2.4);
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        private List<Issue> CheckTargetSize(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var targets = doc.DocumentNode.SelectNodes("//a | //button | //input | //select | //textarea");
            if (targets != null)
            {
                bool hasTargets = false;
                foreach (var target in targets)
                {
                    if (target.Name == "input" && target.Attributes["type"]?.Value?.ToLower() == "hidden")
                        continue;
                    hasTargets = true;
                    break;
                }

                if (hasTargets)
                {
                    issues.Add(new Issue
                    {
                        Type = "Target Size Verification (WCAG 2.2)",
                        ElementSnippet = "Interactive Elements (Links, Buttons, Inputs)",
                        SuggestedFix = "Ensure all clickable targets are at least 24x24 CSS pixels (WCAG 2.5.8 Target Size Minimum)",
                        SeverityLevel = Severity.Info,
                        FixExample = "Provide adequate padding or min-width/min-height in CSS",
                        Category = Category.Accessibility
                    });
                }
            }
            return issues;
        }

        private List<Issue> CheckInputPurposeAutocomplete(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var inputs = doc.DocumentNode.SelectNodes("//input[@type='text'] | //input[@type='email'] | //input[@type='tel'] | //input[not(@type)]");
            if (inputs != null)
            {
                foreach (var input in inputs)
                {
                    var name = input.Attributes["name"]?.Value?.ToLower() ?? "";
                    var id = input.Attributes["id"]?.Value?.ToLower() ?? "";
                    
                    if (name.Contains("name") || name.Contains("email") || name.Contains("phone") || name.Contains("address") ||
                        id.Contains("name") || id.Contains("email") || id.Contains("phone") || id.Contains("address"))
                    {
                        if (!input.Attributes.Contains("autocomplete"))
                        {
                            issues.Add(new Issue
                            {
                                Type = "Missing Autocomplete Token (WCAG 1.3.5)",
                                ElementSnippet = input.OuterHtml.Length > 200 ? input.OuterHtml.Substring(0, 197) + "..." : input.OuterHtml,
                                SuggestedFix = "Add standard 'autocomplete' attribute tokens (e.g. 'name', 'email', 'tel', 'street-address') to identify input purpose (WCAG 1.3.5 Identify Input Purpose)",
                                SeverityLevel = Severity.Warning,
                                FixExample = "<input type=\"email\" name=\"email\" autocomplete=\"email\">",
                                Category = Category.Accessibility,
                                WcagGuideline = "WCAG 2.2 SC 1.3.5 Identify Input Purpose (Level AA)",
                                Impact = "Moderate"
                            });
                        }
                    }
                }
            }
            return issues;
        }

        private List<Issue> CheckEmptyLinks(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var links = doc.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                foreach (var link in links)
                {
                    // Skip hidden links
                    if (link.Attributes["aria-hidden"]?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
                        continue;

                    string text = link.InnerText?.Trim() ?? "";
                    string ariaLabel = link.Attributes["aria-label"]?.Value?.Trim() ?? "";
                    string ariaLabelledBy = link.Attributes["aria-labelledby"]?.Value?.Trim() ?? "";
                    string title = link.Attributes["title"]?.Value?.Trim() ?? "";
                    bool hasImgWithAlt = link.SelectNodes(".//img[@alt and string-length(@alt) > 0]") != null;
                    bool hasSvgWithAria = link.SelectNodes(".//svg[@aria-label or @aria-labelledby or .//title]") != null;
                    bool hasChildWithLabel = link.SelectNodes(".//*[@aria-label or @title]") != null;

                    if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(ariaLabel) && string.IsNullOrEmpty(ariaLabelledBy) && 
                        string.IsNullOrEmpty(title) && !hasImgWithAlt && !hasSvgWithAria && !hasChildWithLabel)
                    {
                        issues.Add(new Issue
                        {
                            Type = "Empty or Unlabeled Link",
                            ElementSnippet = link.OuterHtml.Length > 200 ? link.OuterHtml.Substring(0, 197) + "..." : link.OuterHtml,
                            SuggestedFix = "Provide descriptive link text, an 'aria-label', or descriptive alt text on child images to clarify the link purpose",
                            SeverityLevel = Severity.Error,
                            FixExample = "<a href=\"/dashboard\" aria-label=\"User Dashboard\"><i class=\"icon-dash\"></i></a>",
                            Category = Category.Accessibility,
                            WcagGuideline = "WCAG 2.2 SC 2.4.4 Link Purpose (In Context) (Level A)",
                            Impact = "Critical"
                        });
                    }
                }
            }
            return issues;
        }

        private List<Issue> CheckEmptyButtons(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var buttons = doc.DocumentNode.SelectNodes("//button | //input[@type='button' or @type='submit' or @type='reset']");
            if (buttons != null)
            {
                foreach (var btn in buttons)
                {
                    // Skip hidden buttons
                    if (btn.Attributes["aria-hidden"]?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
                        continue;

                    // If it's an input button, check value attribute
                    if (btn.Name.Equals("input", StringComparison.OrdinalIgnoreCase))
                    {
                        string val = btn.Attributes["value"]?.Value?.Trim() ?? "";
                        string ariaLabelInput = btn.Attributes["aria-label"]?.Value?.Trim() ?? "";
                        string ariaLabelledByInput = btn.Attributes["aria-labelledby"]?.Value?.Trim() ?? "";
                        if (string.IsNullOrEmpty(val) && string.IsNullOrEmpty(ariaLabelInput) && string.IsNullOrEmpty(ariaLabelledByInput))
                        {
                            issues.Add(new Issue
                            {
                                Type = "Unlabeled Input Button",
                                ElementSnippet = btn.OuterHtml.Length > 200 ? btn.OuterHtml.Substring(0, 197) + "..." : btn.OuterHtml,
                                SuggestedFix = "Add a descriptive 'value' attribute or 'aria-label' to the button input",
                                SeverityLevel = Severity.Error,
                                FixExample = "<input type=\"submit\" value=\"Submit Form\">",
                                Category = Category.Accessibility,
                                WcagGuideline = "WCAG 2.2 SC 4.1.2 Name, Role, Value (Level A)",
                                Impact = "Critical"
                            });
                        }
                        continue;
                    }

                    string text = btn.InnerText?.Trim() ?? "";
                    string ariaLabel = btn.Attributes["aria-label"]?.Value?.Trim() ?? "";
                    string ariaLabelledBy = btn.Attributes["aria-labelledby"]?.Value?.Trim() ?? "";
                    string title = btn.Attributes["title"]?.Value?.Trim() ?? "";
                    
                    // Child elements providing accessible names
                    bool hasImgWithAlt = btn.SelectNodes(".//img[@alt and string-length(@alt) > 0]") != null;
                    bool hasSvgWithAria = btn.SelectNodes(".//svg[@aria-label or @aria-labelledby or .//title]") != null;
                    bool hasChildWithAria = btn.SelectNodes(".//*[@aria-label or @title]") != null;

                    if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(ariaLabel) && string.IsNullOrEmpty(ariaLabelledBy) && 
                        string.IsNullOrEmpty(title) && !hasImgWithAlt && !hasSvgWithAria && !hasChildWithAria)
                    {
                        issues.Add(new Issue
                        {
                            Type = "Unlabeled Button",
                            ElementSnippet = btn.OuterHtml.Length > 200 ? btn.OuterHtml.Substring(0, 197) + "..." : btn.OuterHtml,
                            SuggestedFix = "Add visible button text, an 'aria-label', or 'aria-labelledby' attribute to define its accessible name",
                            SeverityLevel = Severity.Error,
                            FixExample = "<button type=\"button\" aria-label=\"Close dialog\">×</button>",
                            Category = Category.Accessibility,
                            WcagGuideline = "WCAG 2.2 SC 4.1.2 Name, Role, Value (Level A)",
                            Impact = "Critical"
                        });
                    }
                }
            }
            return issues;
        }

        private List<Issue> CheckDuplicateIds(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var elementsWithId = doc.DocumentNode.SelectNodes("//*[@id]");
            if (elementsWithId != null)
            {
                var idGroups = elementsWithId
                    .GroupBy(n => n.Attributes["id"]?.Value?.Trim() ?? "")
                    .Where(g => !string.IsNullOrEmpty(g.Key) && g.Count() > 1);

                foreach (var group in idGroups)
                {
                    issues.Add(new Issue
                    {
                        Type = "Duplicate ID Attribute",
                        ElementSnippet = $"id=\"{group.Key}\" found on {group.Count()} elements",
                        SuggestedFix = "Element IDs must be unique across the document. Duplicates break form associations (<label for>) and ARIA references (aria-labelledby, aria-describedby).",
                        SeverityLevel = Severity.Error,
                        FixExample = "Ensure each ID is unique (e.g. id=\"user-nav\", id=\"mobile-nav\")",
                        Category = Category.Accessibility,
                        WcagGuideline = "WCAG 2.2 SC 1.3.1 Info and Relationships / SC 4.1.2 Name, Role, Value (Level A)",
                        Impact = "Serious"
                    });
                }
            }
            return issues;
        }

        private List<Issue> CheckViewportScalability(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var viewportMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='viewport']");
            if (viewportMeta != null)
            {
                string content = viewportMeta.Attributes["content"]?.Value?.ToLower() ?? "";
                if (content.Contains("user-scalable=no") || content.Contains("maximum-scale=1") || content.Contains("user-scalable=0"))
                {
                    issues.Add(new Issue
                    {
                        Type = "Pinch-to-Zoom Disabled",
                        ElementSnippet = viewportMeta.OuterHtml,
                        SuggestedFix = "Do not disable user zooming; remove 'user-scalable=no' and 'maximum-scale=1' to allow low-vision users to resize content (WCAG 1.4.4 Resize Text)",
                        SeverityLevel = Severity.Error,
                        FixExample = "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">",
                        Category = Category.Accessibility,
                        WcagGuideline = "WCAG 2.2 SC 1.4.4 Resize Text (Level AA)",
                        Impact = "Critical"
                    });
                }
            }
            return issues;
        }

        private List<Issue> CheckLandmarks(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var mainLandmark = doc.DocumentNode.SelectSingleNode("//main | //*[@role='main']");
            if (mainLandmark == null)
            {
                issues.Add(new Issue
                {
                    Type = "Missing Main Landmark",
                    ElementSnippet = "<body>...</body>",
                    SuggestedFix = "Wrap the primary page content inside a <main> element or add role=\"main\" to provide a landmark bypass block for screen reader users (WCAG 2.4.1 Bypass Blocks)",
                    SeverityLevel = Severity.Warning,
                    FixExample = "<main id=\"main-content\">...</main>",
                    Category = Category.Accessibility,
                    WcagGuideline = "WCAG 2.2 SC 2.4.1 Bypass Blocks (Level A) / Best Practice",
                    Impact = "Moderate"
                });
            }
            return issues;
        }

        private List<Issue> CheckTableAccessibility(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var tables = doc.DocumentNode.SelectNodes("//table");
            if (tables != null)
            {
                foreach (var table in tables)
                {
                    // Ignore layout tables explicitly marked with role="presentation" or role="none"
                    string role = table.Attributes["role"]?.Value?.Trim().ToLowerInvariant() ?? "";
                    if (role == "presentation" || role == "none") continue;

                    // Skip hidden tables
                    if (table.Attributes["aria-hidden"]?.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
                        continue;

                    // If table has no rows, skip
                    var rows = table.SelectNodes(".//tr");
                    if (rows == null || rows.Count == 0) continue;

                    var headers = table.SelectNodes(".//th");
                    if (headers == null || headers.Count == 0)
                    {
                        // Check if role="columnheader" or role="rowheader" are used instead of <th>
                        var roleHeaders = table.SelectNodes(".//*[@role='columnheader' or @role='rowheader']");
                        if (roleHeaders != null && roleHeaders.Count > 0) continue;

                        issues.Add(new Issue
                        {
                            Type = "Data Table Missing Header (<th>)",
                            ElementSnippet = table.OuterHtml.Length > 200 ? table.OuterHtml.Substring(0, 197) + "..." : table.OuterHtml,
                            SuggestedFix = "Data tables must include <th> header cells (or role=\"columnheader\"). If this table is used strictly for layout, mark it with role=\"presentation\" or role=\"none\".",
                            SeverityLevel = Severity.Warning,
                            FixExample = "<table><thead><tr><th scope=\"col\">Header</th></tr></thead><tbody>...</tbody></table>",
                            Category = Category.Accessibility,
                            WcagGuideline = "WCAG 2.2 SC 1.3.1 Info and Relationships (Level A)",
                            Impact = "Moderate"
                        });
                    }
                }
            }
            return issues;
        }

        private List<Issue> CheckAudioVideoAutoplay(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var mediaElements = doc.DocumentNode.SelectNodes("//video[@autoplay] | //audio[@autoplay]");
            if (mediaElements != null)
            {
                foreach (var media in mediaElements)
                {
                    bool hasControls = media.Attributes.Contains("controls");
                    bool isMuted = media.Attributes.Contains("muted");

                    // Video or audio that is muted produces no sound and therefore does not violate WCAG 1.4.2 Audio Control
                    if (isMuted) continue;
                    if (hasControls) continue;

                    issues.Add(new Issue
                    {
                        Type = "Autoplaying Audio Without Controls",
                        ElementSnippet = media.OuterHtml.Length > 200 ? media.OuterHtml.Substring(0, 197) + "..." : media.OuterHtml,
                        SuggestedFix = "Audio that plays automatically for more than 3 seconds must have user controls to pause or stop it, or must be muted by default (WCAG 1.4.2 Audio Control)",
                        SeverityLevel = Severity.Error,
                        FixExample = "<video autoplay muted controls>...</video>",
                        Category = Category.Accessibility,
                        WcagGuideline = "WCAG 2.2 SC 1.4.2 Audio Control (Level A)",
                        Impact = "Critical"
                    });
                }
            }
            return issues;
        }

        public void EnrichIssueMetadata(List<Issue> issues)
        {
            foreach (var issue in issues)
            {
                if (string.IsNullOrEmpty(issue.WcagGuideline))
                {
                    string t = issue.Type ?? "";
                    if (t.Contains("Alt Text"))
                        issue.WcagGuideline = "WCAG 2.2 SC 1.1.1 Non-text Content (Level A)";
                    else if (t.Contains("Label"))
                        issue.WcagGuideline = "WCAG 2.2 SC 1.3.1 Info & Relationships, SC 3.3.2 Labels (Level A)";
                    else if (t.Contains("Title"))
                        issue.WcagGuideline = "WCAG 2.2 SC 2.4.2 Page Titled (Level A)";
                    else if (t.Contains("Heading"))
                        issue.WcagGuideline = "WCAG 2.2 SC 1.3.1 Info and Relationships (Level A)";
                    else if (t.Contains("Contrast"))
                        issue.WcagGuideline = "WCAG 2.2 SC 1.4.3 Contrast (Minimum) (Level AA)";
                    else if (t.Contains("Eye Comfort"))
                        issue.WcagGuideline = "WCAG 2.2 SC 1.4.8 Visual Presentation (Level AAA)";
                    else if (t.Contains("ARIA"))
                        issue.WcagGuideline = "WCAG 2.2 SC 4.1.2 Name, Role, Value (Level A)";
                    else if (t.Contains("Language") || t.Contains("lang"))
                        issue.WcagGuideline = "WCAG 2.2 SC 3.1.1 Language of Page (Level A)";
                    else if (t.Contains("Target Size"))
                        issue.WcagGuideline = "WCAG 2.2 SC 2.5.8 Target Size (Minimum) (Level AA)";
                    else if (t.Contains("Autocomplete") || t.Contains("Redundant Entry"))
                        issue.WcagGuideline = "WCAG 2.2 SC 1.3.5 Identify Input Purpose (Level AA)";
                    else if (t.Contains("Zoom") || t.Contains("Scalab"))
                        issue.WcagGuideline = "WCAG 2.2 SC 1.4.4 Resize Text (Level AA)";
                    else if (t.Contains("Link"))
                        issue.WcagGuideline = "WCAG 2.2 SC 2.4.4 Link Purpose (In Context) (Level A)";
                    else if (t.Contains("Button"))
                        issue.WcagGuideline = "WCAG 2.2 SC 4.1.2 Name, Role, Value (Level A)";
                    else if (t.Contains("ID"))
                        issue.WcagGuideline = "WCAG 2.2 SC 1.3.1 Info & Relationships / SC 4.1.2 Name, Role, Value (Level A)";
                    else if (t.Contains("Landmark"))
                        issue.WcagGuideline = "WCAG 2.2 SC 2.4.1 Bypass Blocks (Level A) / Best Practice";
                    else if (t.Contains("Table"))
                        issue.WcagGuideline = "WCAG 2.2 SC 1.3.1 Info and Relationships (Level A)";
                    else if (t.Contains("Autoplay") || t.Contains("Audio"))
                        issue.WcagGuideline = "WCAG 2.2 SC 1.4.2 Audio Control (Level A)";
                    else if (t.Contains("Mobile") || t.Contains("Responsive"))
                        issue.WcagGuideline = "WCAG 2.2 SC 1.4.10 Reflow (Level AA)";
                    else
                    {
                        issue.WcagGuideline = issue.Category switch
                        {
                            Category.Accessibility => "WCAG 2.2 & Section 508 / RPwD Act 2016",
                            Category.SEO => "Core Discoverability & Search Quality Standards",
                            Category.Performance => "Core Web Vitals & Performance Standards",
                            Category.Environment => "Sustainable Web Design Standards (W3C)",
                            Category.Safety => "OWASP Web Security & Content Standards",
                            _ => "W3C Web Standards"
                        };
                    }
                }

                if (string.IsNullOrEmpty(issue.Impact))
                {
                    issue.Impact = issue.SeverityLevel switch
                    {
                        Severity.Error => "Critical",
                        Severity.Warning => "Serious",
                        _ => "Minor"
                    };
                }
            }
        }
    }
}