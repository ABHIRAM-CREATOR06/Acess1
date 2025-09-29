using System.Collections.Generic;
using System.Linq;

namespace WebAccessibilityChecker.Models
{
    public enum Category { Accessibility, SEO, Performance, BestPractices, Environment, Safety }

    public class Report
    {
        public List<Issue> Issues { get; set; } = new List<Issue>();
        public int TotalIssues => Issues.Count;
        public int ErrorCount => Issues.Count(i => i.SeverityLevel == Severity.Error);
        public int WarningCount => Issues.Count(i => i.SeverityLevel == Severity.Warning);
        public int InfoCount => Issues.Count(i => i.SeverityLevel == Severity.Info);
        public int AccessibilityScore { get; set; }
        public int SEOScore { get; set; }
        public int PerformanceScore { get; set; }
        public int BestPracticesScore { get; set; }
        public int EnvironmentScore { get; set; }
        public int SafetyScore { get; set; }
        public string? ComplianceStatus { get; set; }
        public string? WebsiteUrl { get; set; }
        public double PageLoadTime { get; set; } // in seconds
        public int RequestCount { get; set; }
        public long PageSize { get; set; } // in bytes
        public double SustainabilityScore { get; set; } // carbon footprint estimate

        // Enhanced Environmental Impact Properties
        public bool UsesCDN { get; set; } // Whether CDN is detected
        public double EnergyConsumptionKWh { get; set; } // Estimated energy use in kWh per page load
        public double CO2EmissionsGrams { get; set; } // CO₂ emissions in grams per page load
        public string? EnvironmentalRating { get; set; } // Eco, Moderate, High Impact
    }
}