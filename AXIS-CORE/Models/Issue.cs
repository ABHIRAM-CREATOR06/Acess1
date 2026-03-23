using System;

namespace AXIS_CORE.Models
{
    public enum Severity { Error, Warning, Info }

    public class Issue
    {
        public string? Type { get; set; }
        public string? ElementSnippet { get; set; }
        public string? SuggestedFix { get; set; }
        public Severity SeverityLevel { get; set; }
        public string? FixExample { get; set; } // For bonus
        public Category Category { get; set; }
        public int Count { get; set; } = 1; // For deduplication: tracks how many times this issue type occurs
    }
}