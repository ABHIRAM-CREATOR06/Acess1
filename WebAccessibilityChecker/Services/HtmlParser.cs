using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using PuppeteerSharp;
using System.IO;
using System;
using WebAccessibilityChecker.Models;

namespace WebAccessibilityChecker.Services
{
    public class HtmlParser
    {
        private static readonly HttpClient httpClient = new HttpClient();

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

        public async Task<PageLoadResult> LoadFromUrlWithHeadlessAsync(string url)
        {
            try
            {
                // Try to use system Chrome first, fallback to bundled version
                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage", "--disable-accelerated-2d-canvas", "--no-first-run", "--no-zygote", "--single-process", "--disable-gpu" }
                };

                // Try system Chrome first
                var chromePath = GetChromePath();
                if (!string.IsNullOrEmpty(chromePath))
                {
                    launchOptions.ExecutablePath = chromePath;
                }
                // If no system Chrome, PuppeteerSharp will download its own version

                // Set timeout for browser launch
                var browserTask = PuppeteerSharp.Puppeteer.LaunchAsync(launchOptions);
                if (await Task.WhenAny(browserTask, Task.Delay(30000)) != browserTask)
                {
                    throw new TimeoutException("Browser launch timeout");
                }

                await using var browser = await browserTask;
                await using var page = await browser.NewPageAsync();

                // Set page timeouts
                page.DefaultTimeout = 30000;
                page.DefaultNavigationTimeout = 30000;

                var startTime = DateTime.Now;

                // Navigate with timeout
                var navigationTask = page.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });
                if (await Task.WhenAny(navigationTask, Task.Delay(45000)) != navigationTask)
                {
                    throw new TimeoutException("Page navigation timeout");
                }

                await navigationTask;

                // Wait a bit for dynamic content
                await Task.Delay(1000);

                var loadTime = (DateTime.Now - startTime).TotalSeconds;

                // Get content with timeout
                var contentTask = page.GetContentAsync();
                if (await Task.WhenAny(contentTask, Task.Delay(10000)) != contentTask)
                {
                    throw new TimeoutException("Content retrieval timeout");
                }

                var content = await contentTask;
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                // Get performance metrics safely
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
                    // Use fallback values if evaluation fails
                    requestCount = 1;
                    pageSize = content.Length;
                }

                // Check for compression and caching (simplified)
                bool isCompressed = false;
                bool hasCachingHeaders = false;

                try
                {
                    var response = await page.GoToAsync(url);
                    isCompressed = response.Headers.ContainsKey("content-encoding") &&
                                 response.Headers["content-encoding"].Contains("gzip");
                    hasCachingHeaders = response.Headers.ContainsKey("cache-control") ||
                                      response.Headers.ContainsKey("expires");
                }
                catch
                {
                    // Use defaults if response check fails
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
                // Fallback to simple HTTP download if headless fails
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
                    // If even HTTP fails, create a minimal document
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
            // Common Chrome installation paths on Windows
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

            // Return null to let PuppeteerSharp download its own version
            return null;
        }
    }
}