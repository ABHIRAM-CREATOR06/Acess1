using System;
using System.Collections.Generic;

namespace WebAccessibilityChecker.Models
{
    public enum Severity { Error, Warning, Info }

    public class Issue
    {
        public string? Type { get; set; }
        public string? ElementSnippet { get; set; }
        public string? SuggestedFix { get; set; }
        public Severity SeverityLevel { get; set; }
        public string? FixExample { get; set; }
        public Category Category { get; set; }
        public int Count { get; set; } = 1;
        
        // Store multiple unique element instances for grouped display
        public List<string> ElementInstances { get; set; } = new List<string>();

        // Modern WCAG & Standards Reference
        public string? WcagGuideline { get; set; }
        public string? Impact { get; set; } // Critical, Serious, Moderate, Minor
        
        // Remediation tracking
        public bool IsResolved { get; set; } = false;

        // UI helper properties
        public string SeverityBadge => SeverityLevel switch
        {
            Severity.Error => "🔴 Error",
            Severity.Warning => "🟡 Warning",
            _ => "🔵 Info"
        };

        public string CategoryDisplay => Category switch
        {
            Category.Accessibility => "♿ Accessibility",
            Category.SEO => "🔍 SEO",
            Category.Performance => "⚡ Performance",
            Category.BestPractices => "✨ Best Practices",
            Category.Environment => "🌱 Environment",
            _ => "🔒 Safety"
        };

        public string InstancesSummary => Count > 1 ? $"{Count} occurrences" : "1 occurrence";
    }
}