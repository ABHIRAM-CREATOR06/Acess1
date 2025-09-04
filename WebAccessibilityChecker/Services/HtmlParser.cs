using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using PuppeteerSharp;
using System.IO;
using System;

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
            var doc = new HtmlDocument();
            doc.Load(path);
            return doc;
        }

        public async Task<HtmlDocument> LoadFromUrlWithHeadlessAsync(string url)
        {
            try
            {
                // Try to use system Chrome first
                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = GetChromePath()
                };

                await using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(launchOptions);
                await using var page = await browser.NewPageAsync();
                await page.GoToAsync(url);
                await page.WaitForSelectorAsync("body");
                await Task.Delay(2000); // Wait for JS
                var content = await page.GetContentAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(content);
                return doc;
            }
            catch (Exception ex)
            {
                // Fallback to simple HTTP download if headless fails
                Console.WriteLine($"Headless browser failed: {ex.Message}. Falling back to HTTP download.");
                return await LoadFromUrlAsync(url);
            }
        }

        private string GetChromePath()
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

            // If Chrome not found, let PuppeteerSharp download its own version
            throw new FileNotFoundException("Chrome not found in standard locations");
        }
    }
}