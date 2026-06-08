using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System;
using AXIS_CORE.Models;

namespace AXIS_CORE.Services
{
    public class SEOChecker
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<List<Issue>> CheckSEOAsync(HtmlDocument doc, string url)
        {
            var issues = new List<Issue>();
            issues.AddRange(CheckMetaTags(doc));
            issues.AddRange(CheckHeadings(doc));

            // Run robots.txt and sitemap checks in parallel — they are independent HTTP requests.
            // Previously sequential, this cuts SEO check network time roughly in half.
            var robotsTask = CheckRobotsTxtAsync(url);
            var sitemapTask = CheckSitemapAsync(url);
            await Task.WhenAll(robotsTask, sitemapTask);

            issues.AddRange(await robotsTask);
            issues.AddRange(await sitemapTask);
            issues.AddRange(CheckSchemaOrg(doc));
            return issues;
        }

        private List<Issue> CheckMetaTags(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var head = doc.DocumentNode.SelectSingleNode("//head");
            if (head != null)
            {
                var metaDescription = head.SelectSingleNode("//meta[@name='description']");
                if (metaDescription == null || string.IsNullOrEmpty(metaDescription.Attributes["content"]?.Value))
                {
                    issues.Add(new Issue
                    {
                        Type = "Missing Meta Description",
                        ElementSnippet = "<head>",
                        SuggestedFix = "Add meta description tag",
                        SeverityLevel = Severity.Warning,
                        FixExample = "<meta name=\"description\" content=\"Page description\">",
                        Category = Category.SEO
                    });
                }

                var metaKeywords = head.SelectSingleNode("//meta[@name='keywords']");
                if (metaKeywords == null)
                {
                    issues.Add(new Issue
                    {
                        Type = "Missing Meta Keywords",
                        ElementSnippet = "<head>",
                        SuggestedFix = "Add meta keywords tag",
                        SeverityLevel = Severity.Info,
                        FixExample = "<meta name=\"keywords\" content=\"keyword1, keyword2\">",
                        Category = Category.SEO
                    });
                }

                var metaViewport = head.SelectSingleNode("//meta[@name='viewport']");
                if (metaViewport == null)
                {
                    issues.Add(new Issue
                    {
                        Type = "Missing Viewport Meta Tag",
                        ElementSnippet = "<head>",
                        SuggestedFix = "Add viewport meta tag for mobile",
                        SeverityLevel = Severity.Warning,
                        FixExample = "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">",
                        Category = Category.SEO
                    });
                }
            }
            return issues;
        }

        private List<Issue> CheckHeadings(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var h1s = doc.DocumentNode.SelectNodes("//h1");
            if (h1s == null || h1s.Count == 0)
            {
                issues.Add(new Issue
                {
                    Type = "Missing H1 Tag",
                    ElementSnippet = "<body>",
                    SuggestedFix = "Add at least one H1 tag",
                    SeverityLevel = Severity.Warning,
                    FixExample = "<h1>Main Heading</h1>",
                    Category = Category.SEO
                });
            }
            else if (h1s.Count > 1)
            {
                issues.Add(new Issue
                {
                    Type = "Multiple H1 Tags",
                    ElementSnippet = string.Join("", h1s.Select(h => h.OuterHtml)),
                    SuggestedFix = "Use only one H1 per page",
                    SeverityLevel = Severity.Info,
                    FixExample = "Use H2-H6 for subheadings",
                    Category = Category.SEO
                });
            }
            return issues;
        }

        private async Task<List<Issue>> CheckRobotsTxtAsync(string url)
        {
            var issues = new List<Issue>();
            try
            {
                var robotsUrl = new Uri(new Uri(url), "/robots.txt").ToString();
                var response = await httpClient.GetAsync(robotsUrl);
                if (!response.IsSuccessStatusCode)
                {
                    issues.Add(new Issue
                    {
                        Type = "Missing Robots.txt",
                        ElementSnippet = robotsUrl,
                        SuggestedFix = "Create robots.txt file",
                        SeverityLevel = Severity.Info,
                        FixExample = "User-agent: *\nDisallow: /private/",
                        Category = Category.SEO
                    });
                }
            }
            catch
            {
                issues.Add(new Issue
                {
                    Type = "Robots.txt Check Failed",
                    ElementSnippet = url + "/robots.txt",
                    SuggestedFix = "Ensure robots.txt is accessible",
                    SeverityLevel = Severity.Info,
                    FixExample = "Check server configuration",
                    Category = Category.SEO
                });
            }
            return issues;
        }

        private async Task<List<Issue>> CheckSitemapAsync(string url)
        {
            var issues = new List<Issue>();
            try
            {
                var sitemapUrl = new Uri(new Uri(url), "/sitemap.xml").ToString();
                var response = await httpClient.GetAsync(sitemapUrl);
                if (!response.IsSuccessStatusCode)
                {
                    issues.Add(new Issue
                    {
                        Type = "Missing Sitemap.xml",
                        ElementSnippet = sitemapUrl,
                        SuggestedFix = "Create sitemap.xml file",
                        SeverityLevel = Severity.Info,
                        FixExample = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"><url><loc>http://example.com</loc></url></urlset>",
                        Category = Category.SEO
                    });
                }
            }
            catch
            {
                issues.Add(new Issue
                {
                    Type = "Sitemap Check Failed",
                    ElementSnippet = url + "/sitemap.xml",
                    SuggestedFix = "Ensure sitemap.xml is accessible",
                    SeverityLevel = Severity.Info,
                    FixExample = "Check server configuration",
                    Category = Category.SEO
                });
            }
            return issues;
        }

        private List<Issue> CheckSchemaOrg(HtmlDocument doc)
        {
            var issues = new List<Issue>();
            var schemaElements = doc.DocumentNode.SelectNodes("//*[@itemtype] | //*[@itemscope]");
            if (schemaElements == null || schemaElements.Count == 0)
            {
                issues.Add(new Issue
                {
                    Type = "Missing Schema.org Markup",
                    ElementSnippet = "<body>",
                    SuggestedFix = "Add structured data markup",
                    SeverityLevel = Severity.Info,
                    FixExample = "<div itemscope itemtype=\"http://schema.org/Article\"><h1 itemprop=\"name\">Title</h1></div>",
                    Category = Category.SEO
                });
            }
            return issues;
        }
    }
}