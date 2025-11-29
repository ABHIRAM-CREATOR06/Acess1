using HtmlAgilityPack;

namespace AXIS_CORE.Models
{
    public class PageLoadResult
    {
        public required HtmlDocument Document { get; set; }
        public double LoadTime { get; set; } // in seconds
        public int RequestCount { get; set; }
        public long PageSize { get; set; } // in bytes
        public bool IsCompressed { get; set; }
        public bool HasCachingHeaders { get; set; }
    }
}