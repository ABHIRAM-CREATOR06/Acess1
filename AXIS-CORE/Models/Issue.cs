using System;
using System.Collections.Generic;

namespace AXIS_CORE.Models
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

        // Standards reference & remediation metadata
        public string? WcagGuideline { get; set; }
        public string? Impact { get; set; }
        public bool IsResolved { get; set; } = false;
    }
}