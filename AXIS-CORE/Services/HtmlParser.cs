using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using PuppeteerSharp;
using System.IO;
using System;
using AXIS_CORE.Models;

namespace AXIS_CORE.Services
{
    public class HtmlParser
    {
        // Use SocketsHttpHandler for proper connection pooling and DNS refresh,
        // preventing socket exhaustion under repeated checks.
        private static readonly HttpClient httpClient = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 10,
        });

        public async Task<HtmlDocument> LoadFromUrlAsync(string url)
        {
            var html = await httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }

        public HtmlDocument LoadFromFile(string path)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.Load(path);
                return doc;
            }
            catch (Exception ex)
            {
                // Create a minimal document if loading fails
                var doc = new HtmlDocument();
                doc.LoadHtml($"<html><head><title>Error Loading File</title></head><body><h1>File Load Error</h1><p>Could not parse HTML file: {ex.Message}</p></body></html>");
                return doc;
            }
        }

        public async Task<PageLoadResult> LoadPageAsync(string url)
        {
            return await LoadFromUrlWithHeadlessAsync(url);
        }

        public PageLoadResult ParseHtml(string htmlContent, string? baseUrl = null)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            return new PageLoadResult
            {
                Document = doc,
                LoadTime = 0,
                RequestCount = 1,
                PageSize = htmlContent.Length,
                IsCompressed = false,
                HasCachingHeaders = false
            };
        }

        public async Task<PageLoadResult> LoadFromUrlWithHeadlessAsync(string url)
        {
            try
            {
                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    Args = new[]
                    {
                        "--no-sandbox",
                        "--disable-setuid-sandbox",
                        "--disable-dev-shm-usage",
                        "--disable-accelerated-2d-canvas",
                        "--no-first-run",
                        "--no-zygote",
                        "--single-process",
                        "--disable-gpu"
                    }
                };

                var chromePath = GetChromePath();
                if (!string.IsNullOrEmpty(chromePath))
                {
                    launchOptions.ExecutablePath = chromePath;
                }

                var browserTask = PuppeteerSharp.Puppeteer.LaunchAsync(launchOptions);
                if (await Task.WhenAny(browserTask, Task.Delay(30000)) != browserTask)
                    throw new TimeoutException("Browser launch timeout");

                await using var browser = await browserTask;
                await using var page = await browser.NewPageAsync();

                page.DefaultTimeout = 30000;
                page.DefaultNavigationTimeout = 30000;

                var startTime = DateTime.Now;

                // Navigate once and capture the response — eliminates the second navigation
                // that was previously used just to read headers.
                // DOMContentLoaded is used instead of Networkidle0: real-world sites with
                // ads/analytics/polling never reach network idle and cause unnecessary delays.
                var navigationTask = page.GoToAsync(url, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded }
                });

                if (await Task.WhenAny(navigationTask, Task.Delay(45000)) != navigationTask)
                    throw new TimeoutException("Page navigation timeout");

                var response = await navigationTask;

                // Short fixed delay for dynamic content — previously baked into Networkidle0
                await Task.Delay(500);

                var loadTime = (DateTime.Now - startTime).TotalSeconds;

                var contentTask = page.GetContentAsync();
                if (await Task.WhenAny(contentTask, Task.Delay(10000)) != contentTask)
                    throw new TimeoutException("Content retrieval timeout");

                var content = await contentTask;
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                // Read performance metrics
                int requestCount = 1;
                long pageSize = content.Length;

                try
                {
                    var performance = await page.EvaluateFunctionAsync<dynamic>(
                        "() => { try { return { requests: window.performance.getEntriesByType('resource').length, pageSize: document.documentElement.outerHTML.length }; } catch(e) { return { requests: 0, pageSize: 0 }; } }"
                    );
                    requestCount = (int)performance.requests + 1;
                    pageSize = (long)performance.pageSize;
                }
                catch
                {
                    requestCount = 1;
                    pageSize = content.Length;
                }

                // Read headers from the FIRST navigation response — no second page.GoToAsync needed
                bool isCompressed = false;
                bool hasCachingHeaders = false;

                if (response != null)
                {
                    isCompressed = response.Headers.TryGetValue("content-encoding", out var encoding)
                                   && encoding.Contains("gzip", StringComparison.OrdinalIgnoreCase);
                    hasCachingHeaders = response.Headers.ContainsKey("cache-control")
                                        || response.Headers.ContainsKey("expires");
                }

                return new PageLoadResult
                {
                    Document = doc,
                    LoadTime = loadTime,
                    RequestCount = requestCount,
                    PageSize = pageSize,
                    IsCompressed = isCompressed,
                    HasCachingHeaders = hasCachingHeaders
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Headless browser failed: {ex.Message}. Falling back to HTTP download.");
                try
                {
                    var doc = await LoadFromUrlAsync(url);
                    return new PageLoadResult
                    {
                        Document = doc,
                        LoadTime = 0,
                        RequestCount = 1,
                        PageSize = doc.DocumentNode.OuterHtml.Length,
                        IsCompressed = false,
                        HasCachingHeaders = false
                    };
                }
                catch (Exception httpEx)
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml($"<html><head><title>Loading Error</title></head><body><h1>Failed to Load Page</h1><p>Headless browser error: {ex.Message}</p><p>HTTP fallback error: {httpEx.Message}</p></body></html>");
                    return new PageLoadResult
                    {
                        Document = doc,
                        LoadTime = 0,
                        RequestCount = 1,
                        PageSize = doc.DocumentNode.OuterHtml.Length,
                        IsCompressed = false,
                        HasCachingHeaders = false
                    };
                }
            }
        }

        private string? GetChromePath()
        {
            var paths = new[]
            {
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Google\Chrome\Application\chrome.exe"
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                    return path;
            }

            return null;
        }
    }
}