using HtmlAgilityPack;
using System.Collections.Generic;
using WebAccessibilityChecker.Models;

namespace WebAccessibilityChecker.Services
{
    public class AccessibilityChecker
    {
        public Report CheckAccessibility(HtmlDocument doc)
        {
            var report = new Report();
            report.Issues.AddRange(CheckAltText(doc));
            report.Issues.AddRange(CheckLabels(doc));
            report.Issues.AddRange(CheckTitle(doc));
            report.Issues.AddRange(CheckHeadingHierarchy(doc));
            report.Issues.AddRange(CheckColorContrast(doc));
            report.Issues.AddRange(CheckEyeComfort(doc));

            // Calculate score and compliance
            int penalty = report.ErrorCount * 10 + report.WarningCount * 5 + report.InfoCount * 1;
            report.AccessibilityScore = Math.Max(0, 100 - penalty);
            if (report.AccessibilityScore >= 95) report.ComplianceStatus = "Fully Compliant";
            else if (report.AccessibilityScore >= 80) report.ComplianceStatus = "Mostly Compliant";
            else if (report.AccessibilityScore >= 60) report.ComplianceStatus = "Partially Compliant";
            else report.ComplianceStatus = "Not Compliant";

            return report;
        }

        private List<Issue> CheckAltText(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var imgs = doc.DocumentNode.SelectNodes("//img");
            if (imgs != null)
            {
                foreach (var img in imgs)
                {
                    if (!img.Attributes.Contains("alt") || string.IsNullOrEmpty(img.Attributes["alt"].Value))
                    {
                        issues.Add(new Issue
                        {
                            Type = "Missing Alt Text",
                            ElementSnippet = img.OuterHtml,
                            SuggestedFix = "Add alt attribute to img tag",
                            SeverityLevel = Severity.Error,
                            FixExample = "<img src='image.jpg' alt='Description of image'>"
                        });
                    }
                }
            }
            return issues;
        }

        private List<Issue> CheckLabels(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var inputs = doc.DocumentNode.SelectNodes("//input");
            if (inputs != null)
            {
                foreach (var input in inputs)
                {
                    var id = input.Attributes["id"]?.Value;
                    if (!string.IsNullOrEmpty(id))
                    {
                        var label = doc.DocumentNode.SelectSingleNode($"//label[@for='{id}']");
                        if (label == null)
                        {
                            issues.Add(new Issue
                            {
                                Type = "Missing Label",
                                ElementSnippet = input.OuterHtml,
                                SuggestedFix = "Add label with for attribute",
                                SeverityLevel = Severity.Error,
                                FixExample = "<label for='inputId'>Label text</label><input id='inputId' type='text'>"
                            });
                        }
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
                    FixExample = "<title>Page Title</title>"
                });
            }
            return issues;
        }

        private List<Issue> CheckHeadingHierarchy(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var headings = doc.DocumentNode.SelectNodes("//h1 | //h2 | //h3 | //h4 | //h5 | //h6");
            if (headings != null)
            {
                int lastLevel = 0;
                foreach (var h in headings)
                {
                    int level = int.Parse(h.Name.Substring(1));
                    if (level > lastLevel + 1)
                    {
                        issues.Add(new Issue
                        {
                            Type = "Heading Hierarchy",
                            ElementSnippet = h.OuterHtml,
                            SuggestedFix = "Ensure headings follow logical order",
                            SeverityLevel = Severity.Warning,
                            FixExample = "Use h1, then h2, etc."
                        });
                    }
                    lastLevel = level;
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
                                    Type = "Low Color Contrast",
                                    ElementSnippet = el.OuterHtml,
                                    SuggestedFix = "Increase contrast ratio to at least 4.5:1",
                                    SeverityLevel = Severity.Warning,
                                    FixExample = "Use darker text on lighter background"
                                });
                            }
                        }
                    }
                }
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
                        if (fontSize < 14)
                        {
                            issues.Add(new Issue
                            {
                                Type = "Small Font Size",
                                ElementSnippet = el.OuterHtml,
                                SuggestedFix = "Increase font size to at least 14px",
                                SeverityLevel = Severity.Warning,
                                FixExample = "font-size: 16px;"
                            });
                        }
                        if (lineHeight < 1.5)
                        {
                            issues.Add(new Issue
                            {
                                Type = "Low Line Height",
                                ElementSnippet = el.OuterHtml,
                                SuggestedFix = "Increase line height to at least 1.5",
                                SeverityLevel = Severity.Info,
                                FixExample = "line-height: 1.6;"
                            });
                        }
                    }
                }
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

        private double GetLuminance((double r, double g, double b) color)
        {
            var r = color.r <= 0.03928 ? color.r / 12.92 : Math.Pow((color.r + 0.055) / 1.055, 2.4);
            var g = color.g <= 0.03928 ? color.g / 12.92 : Math.Pow((color.g + 0.055) / 1.055, 2.4);
            var b = color.b <= 0.03928 ? color.b / 12.92 : Math.Pow((color.b + 0.055) / 1.055, 2.4);
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }
    }
}