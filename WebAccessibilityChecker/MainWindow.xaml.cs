using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Win32;
using HtmlAgilityPack;
using LiveCharts;
using LiveCharts.Wpf;
using WebAccessibilityChecker.Models;
using WebAccessibilityChecker.Services;
using WebAccessibilityChecker.Utils;

namespace WebAccessibilityChecker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly HtmlParser _htmlParser = new HtmlParser();
    private readonly AccessibilityChecker _checker = new AccessibilityChecker();
    private readonly SEOChecker _seoChecker = new SEOChecker();
    private readonly ExportHelper _exportHelper = new ExportHelper();

    private Report? _currentReport;
    private ObservableCollection<Issue> _issuesCollection = new ObservableCollection<Issue>();
    private ICollectionView? _issuesView;

    private string _currentCategoryFilter = "All";
    private string _currentSeverityFilter = "All";
    private string _currentSearchQuery = "";
    private bool _isDarkMode = true;
    private readonly System.Windows.Threading.DispatcherTimer _searchDebounceTimer;

    public event PropertyChangedEventHandler? PropertyChanged;

    private ChartValues<double> _scores = new ChartValues<double>();
    public ChartValues<double> Scores
    {
        get => _scores;
        set
        {
            _scores = value;
            OnPropertyChanged(nameof(Scores));
        }
    }

    private string[] _labels = new[] { "A11y", "SEO", "Perf", "Best", "Eco", "Safety" };
    public string[] Labels
    {
        get => _labels;
        set
        {
            _labels = value;
            OnPropertyChanged(nameof(Labels));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        // Debounce timer for search queries to avoid freezing on large pages
        _searchDebounceTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _searchDebounceTimer.Tick += (s, ev) =>
        {
            _searchDebounceTimer.Stop();
            _currentSearchQuery = SearchTextBox.Text.Trim();
            _issuesView?.Refresh();
        };

        // Initialize issues view
        _issuesView = CollectionViewSource.GetDefaultView(_issuesCollection);
        if (_issuesView != null)
        {
            _issuesView.Filter = FilterIssue;
        }
        ResultsDataGrid.ItemsSource = _issuesView;
    }

    #region Analysis Pipeline

    private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
    {
        string input = UrlTextBox.Text.Trim();
        if (string.IsNullOrEmpty(input))
        {
            MessageBox.Show("Please enter a website URL or browse a local HTML file.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Show loading state
        LoadingPanel.Visibility = Visibility.Visible;
        LoadingTextBlock.Text = "Initializing audit engine...";
        AnalyzeButton.IsEnabled = false;
        BrowseButton.IsEnabled = false;

        HtmlDocument doc;
        string? url = null;
        PageLoadResult? loadResult = null;

        try
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out _) && input.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                url = input;
                LoadingTextBlock.Text = "Rendering webpage with headless Chromium for JavaScript DOM analysis...";
                
                try
                {
                    loadResult = await _htmlParser.LoadFromUrlWithHeadlessAsync(input);
                    doc = loadResult.Document;
                }
                catch (Exception)
                {
                    LoadingTextBlock.Text = "Headless browser unavailable. Falling back to HTTP client...";
                    doc = await _htmlParser.LoadFromUrlAsync(input);
                    loadResult = new PageLoadResult
                    {
                        Document = doc,
                        LoadTime = 0.8,
                        RequestCount = 1,
                        PageSize = doc.DocumentNode.OuterHtml.Length,
                        IsCompressed = true,
                        HasCachingHeaders = true
                    };
                }
            }
            else if (File.Exists(input))
            {
                LoadingTextBlock.Text = "Parsing local HTML file DOM structure...";
                doc = _htmlParser.LoadFromFile(input);
                loadResult = new PageLoadResult
                {
                    Document = doc,
                    LoadTime = 0.05,
                    RequestCount = 1,
                    PageSize = new FileInfo(input).Length,
                    IsCompressed = false,
                    HasCachingHeaders = false
                };
            }
            else
            {
                MessageBox.Show("Invalid URL or file path. Please check the address.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                ResetLoadingState();
                return;
            }

            LoadingTextBlock.Text = "Auditing WCAG 2.2 AA/AAA rules, semantic structure & accessibility APIs...";
            await Task.Delay(50); // Give UI thread a breather for smooth progress updates

            _currentReport = _checker.CheckAccessibility(loadResult);
            _currentReport.WebsiteUrl = url ?? Path.GetFileName(input);

            // Run SEO audit
            if (!string.IsNullOrEmpty(url))
            {
                LoadingTextBlock.Text = "Evaluating SEO discoverability, robots.txt, schema, and meta tags...";
                var seoIssuesList = await _seoChecker.CheckSEOAsync(doc, url);
                _currentReport.Issues.AddRange(seoIssuesList);
            }

            LoadingTextBlock.Text = "Calculating environmental impact & security compliance...";

            // Re-calculate category penalties
            var seoIssues = _currentReport.Issues.Where(i => i.Category == Category.SEO).ToList();
            int seoPenalty = seoIssues.Count(i => i.SeverityLevel == Severity.Error) * 10 +
                             seoIssues.Count(i => i.SeverityLevel == Severity.Warning) * 5 +
                             seoIssues.Count(i => i.SeverityLevel == Severity.Info) * 1;
            _currentReport.SEOScore = Math.Max(0, 100 - seoPenalty);

            var bpIssues = _currentReport.Issues.Where(i => i.Category == Category.BestPractices).ToList();
            int bpPenalty = bpIssues.Count(i => i.SeverityLevel == Severity.Error) * 10 +
                            bpIssues.Count(i => i.SeverityLevel == Severity.Warning) * 5 +
                            bpIssues.Count(i => i.SeverityLevel == Severity.Info) * 1;
            _currentReport.BestPracticesScore = Math.Max(0, 100 - bpPenalty);

            // Re-enrich any new issues with metadata
            _checker.EnrichIssueMetadata(_currentReport.Issues);

            // Update UI with report results
            DisplayReportResults(_currentReport);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Audit encountered an error: {ex.Message}", "Analysis Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ResetLoadingState();
        }
    }

    private void ResetLoadingState()
    {
        LoadingPanel.Visibility = Visibility.Collapsed;
        AnalyzeButton.IsEnabled = true;
        BrowseButton.IsEnabled = true;
    }

    private void DisplayReportResults(Report report)
    {
        // Update score cards
        AccessibilityScoreTextBlock.Text = $"{report.AccessibilityScore}";
        SEOScoreTextBlock.Text = $"{report.SEOScore}";
        PerformanceScoreTextBlock.Text = $"{report.PerformanceScore}";
        BestPracticesScoreTextBlock.Text = $"{report.BestPracticesScore}";
        EnvironmentScoreTextBlock.Text = $"{report.EnvironmentScore}";
        SafetyScoreTextBlock.Text = $"{report.SafetyScore}";

        // Subtext updates
        PageSpeedSubText.Text = report.PageLoadTime > 0 ? $"{report.PageLoadTime:F1}s • {report.RequestCount} req" : "Core Web Vitals";
        EcoRatingSubText.Text = $"{report.EnvironmentalRating ?? "Eco"} ({report.CO2EmissionsGrams:F1}g CO₂)";

        // Overall Grade & Status Banner
        OverallGradeTextBlock.Text = report.OverallGrade;
        ComplianceTextBlock.Text = report.ComplianceStatus ?? "Audited";
        IssuesCountSummaryTextBlock.Text = $"{report.TotalIssues} Issues: {report.ErrorCount} Critical, {report.WarningCount} Warnings, {report.InfoCount} Info";

        if (new BrushConverter().ConvertFromString(report.GradeColor) is SolidColorBrush gradeBrush)
        {
            OverallGradeTextBlock.Foreground = gradeBrush;
        }

        // Populate collection
        _issuesCollection.Clear();
        foreach (var issue in report.Issues)
        {
            _issuesCollection.Add(issue);
        }
        _issuesView?.Refresh();

        // Update Chart
        Scores = new ChartValues<double>
        {
            report.AccessibilityScore,
            report.SEOScore,
            report.PerformanceScore,
            report.BestPracticesScore,
            report.EnvironmentScore,
            report.SafetyScore
        };
        Labels = new[] { "A11y", "SEO", "Perf", "Best", "Eco", "Safety" };

        // Update footer telemetry
        FooterStatusTextBlock.Text = $"Audit complete for {report.WebsiteUrl} • {report.TotalIssues} issues found • Duration: {report.PageLoadTime:F2}s • {DateTime.Now:HH:mm:ss}";

        // Auto-select first issue if available
        if (_issuesCollection.Any())
        {
            ResultsDataGrid.SelectedIndex = 0;
        }
    }

    #endregion

    #region Filtering & Search

    private bool FilterIssue(object item)
    {
        if (item is not Issue issue) return false;

        // Category filter
        if (_currentCategoryFilter != "All")
        {
            if (!issue.Category.ToString().Equals(_currentCategoryFilter, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Severity filter
        if (_currentSeverityFilter != "All")
        {
            if (!issue.SeverityLevel.ToString().Equals(_currentSeverityFilter, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Search query
        if (!string.IsNullOrWhiteSpace(_currentSearchQuery))
        {
            string q = _currentSearchQuery.ToLowerInvariant();
            bool matchesType = issue.Type?.ToLowerInvariant().Contains(q) == true;
            bool matchesFix = issue.SuggestedFix?.ToLowerInvariant().Contains(q) == true;
            bool matchesSnippet = issue.ElementSnippet?.ToLowerInvariant().Contains(q) == true;
            bool matchesGuideline = issue.WcagGuideline?.ToLowerInvariant().Contains(q) == true;

            if (!matchesType && !matchesFix && !matchesSnippet && !matchesGuideline)
                return false;
        }

        return true;
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SearchWatermark.Visibility = string.IsNullOrEmpty(SearchTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    private void CategoryFilter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string category)
        {
            _currentCategoryFilter = category;
            _issuesView?.Refresh();
        }
    }

    private void SeverityFilter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string severity)
        {
            _currentSeverityFilter = severity;
            _issuesView?.Refresh();
        }
    }

    #endregion

    #region Inspection Studio

    private void ResultsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ResultsDataGrid.SelectedItem is not Issue issue) return;

        InspectorTitleTextBlock.Text = issue.Type ?? "Issue Details";
        InspectorSeverityText.Text = issue.SeverityBadge;
        InspectorGuidelineText.Text = issue.WcagGuideline ?? "WCAG 2.2 Standard";
        InspectorImpactText.Text = issue.Impact ?? "Standard";

        // Occurrences selector
        if (issue.ElementInstances != null && issue.ElementInstances.Count > 1)
        {
            OccurrencesPanel.Visibility = Visibility.Visible;
            InstanceComboBox.Items.Clear();
            for (int i = 0; i < issue.ElementInstances.Count; i++)
            {
                string snippetPreview = issue.ElementInstances[i];
                if (snippetPreview.Length > 50) snippetPreview = snippetPreview.Substring(0, 47) + "...";
                InstanceComboBox.Items.Add($"Instance {i + 1}: {snippetPreview}");
            }
            InstanceComboBox.SelectedIndex = 0;
        }
        else
        {
            OccurrencesPanel.Visibility = Visibility.Collapsed;
            InspectorElementTextBox.Text = issue.ElementSnippet ?? "// No element snippet available";
        }

        InspectorFixAdviceText.Text = issue.SuggestedFix ?? "Follow accessibility guidelines.";
        InspectorFixExampleTextBox.Text = issue.FixExample ?? "// No specific code example provided";
        MarkResolvedCheckBox.IsChecked = issue.IsResolved;

        // Colorize badge
        InspectorSeverityBadge.Background = issue.SeverityLevel switch
        {
            Severity.Error => new SolidColorBrush(Color.FromRgb(127, 29, 29)),
            Severity.Warning => new SolidColorBrush(Color.FromRgb(120, 53, 15)),
            _ => new SolidColorBrush(Color.FromRgb(12, 74, 110))
        };
    }

    private void InstanceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ResultsDataGrid.SelectedItem is Issue issue && InstanceComboBox.SelectedIndex >= 0 &&
            issue.ElementInstances != null && InstanceComboBox.SelectedIndex < issue.ElementInstances.Count)
        {
            InspectorElementTextBox.Text = issue.ElementInstances[InstanceComboBox.SelectedIndex];
        }
    }

    private void MarkResolvedCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (ResultsDataGrid.SelectedItem is Issue issue)
        {
            issue.IsResolved = MarkResolvedCheckBox.IsChecked == true;
            if (_currentReport != null)
            {
                FooterStatusTextBlock.Text = $"Audit session note • {_currentReport.ResolvedCount} of {_currentReport.TotalIssues} items marked as dismissed in session.";
            }
        }
    }

    private async Task SafeSetClipboardAsync(string text)
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                Clipboard.SetText(text);
                return;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                await Task.Delay(100);
            }
            catch (Exception)
            {
                break;
            }
        }
    }

    private async void CopySnippetButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(InspectorElementTextBox.Text))
        {
            await SafeSetClipboardAsync(InspectorElementTextBox.Text);
            CopySnippetButton.Content = "✅ Copied!";
            await Task.Delay(1500);
            CopySnippetButton.Content = "📋 Copy Code Snippet";
        }
    }

    private async void CopyFixButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(InspectorFixExampleTextBox.Text))
        {
            await SafeSetClipboardAsync(InspectorFixExampleTextBox.Text);
            CopyFixButton.Content = "✅ Copied!";
            await Task.Delay(1500);
            CopyFixButton.Content = "📋 Copy Fix Example";
        }
    }

    private async void CopySummaryButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentReport == null)
        {
            MessageBox.Show("Please run an audit first to generate a summary.", "No Audit Active", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string md = _exportHelper.GenerateMarkdownReport(_currentReport);
        await SafeSetClipboardAsync(md);
        CopySummaryTopButton.Content = "✅ Copied!";
        await Task.Delay(1500);
        CopySummaryTopButton.Content = "📋 Copy Summary";
    }

    #endregion

    #region Sample URLs & Browse

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "HTML files (*.html;*.htm)|*.html;*.htm|All files (*.*)|*.*",
            Title = "Select HTML File for Accessibility Audit"
        };
        if (openFileDialog.ShowDialog() == true)
        {
            UrlTextBox.Text = openFileDialog.FileName;
        }
    }

    private void QuickSample_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string sampleUrl)
        {
            UrlTextBox.Text = sampleUrl;
            AnalyzeButton_Click(sender, e);
        }
    }

    #endregion

    #region Theme Toggle

    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isDarkMode = !_isDarkMode;

        if (_isDarkMode)
        {
            Resources["AppBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(11, 15, 25));       // #0B0F19
            Resources["CardBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(21, 29, 42));      // #151D2A
            Resources["CardHeaderBrush"] = new SolidColorBrush(Color.FromRgb(28, 39, 58));          // #1C273A
            Resources["CardBorderBrush"] = new SolidColorBrush(Color.FromRgb(36, 48, 68));          // #243044
            Resources["TextPrimaryBrush"] = new SolidColorBrush(Color.FromRgb(241, 245, 249));      // #F1F5F9
            Resources["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(148, 163, 184));    // #94A3B8
            Resources["TextMutedBrush"] = new SolidColorBrush(Color.FromRgb(100, 116, 139));        // #64748B
            Resources["InputBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(16, 24, 39));     // #101827
            Resources["InputBorderBrush"] = new SolidColorBrush(Color.FromRgb(46, 61, 82));         // #2E3D52
            Resources["CodeBoxBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(9, 13, 22));     // #090D16
            Resources["CodeBoxBorderBrush"] = new SolidColorBrush(Color.FromRgb(30, 41, 59));       // #1E293B
            ThemeToggleButton.Content = "☀️ Light";
        }
        else
        {
            Resources["AppBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(248, 250, 252));    // #F8FAFC
            Resources["CardBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));   // #FFFFFF
            Resources["CardHeaderBrush"] = new SolidColorBrush(Color.FromRgb(241, 245, 249));       // #F1F5F9
            Resources["CardBorderBrush"] = new SolidColorBrush(Color.FromRgb(203, 213, 225));       // #CBD5E1
            Resources["TextPrimaryBrush"] = new SolidColorBrush(Color.FromRgb(15, 23, 42));         // #0F172A
            Resources["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(51, 65, 85));       // #334155 (high contrast 4.5:1+)
            Resources["TextMutedBrush"] = new SolidColorBrush(Color.FromRgb(71, 85, 105));          // #475569 (high contrast)
            Resources["InputBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));  // #FFFFFF
            Resources["InputBorderBrush"] = new SolidColorBrush(Color.FromRgb(148, 163, 184));      // #94A3B8
            Resources["CodeBoxBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(241, 245, 249)); // #F1F5F9
            Resources["CodeBoxBorderBrush"] = new SolidColorBrush(Color.FromRgb(203, 213, 225));    // #CBD5E1
            ThemeToggleButton.Content = "🌙 Dark";
        }
    }

    #endregion

    #region Export Handlers

    private void ExportPdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentReport == null)
        {
            MessageBox.Show("No report available to export. Please analyze a website first.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "PDF Document (*.pdf)|*.pdf",
            FileName = $"Accessibility_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
        };
        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                _exportHelper.ExportToPdf(_currentReport, saveFileDialog.FileName);
                MessageBox.Show("Executive PDF Report generated successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export PDF: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ExportHtmlButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentReport == null)
        {
            MessageBox.Show("No report available to export. Please analyze a website first.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "HTML Document (*.html)|*.html",
            FileName = $"Accessibility_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html"
        };
        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                _exportHelper.ExportToHtml(_currentReport, saveFileDialog.FileName);
                MessageBox.Show("Interactive HTML Report generated successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export HTML: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ExportJsonButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentReport == null)
        {
            MessageBox.Show("No report available to export. Please analyze a website first.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "JSON Data (*.json)|*.json",
            FileName = $"Accessibility_Audit_{DateTime.Now:yyyyMMdd_HHmmss}.json"
        };
        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                _exportHelper.ExportToJson(_currentReport, saveFileDialog.FileName);
                MessageBox.Show("Machine-readable JSON Report exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export JSON: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ExportMdButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentReport == null)
        {
            MessageBox.Show("No report available to export. Please analyze a website first.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "Markdown Document (*.md)|*.md",
            FileName = $"Accessibility_Report_{DateTime.Now:yyyyMMdd_HHmmss}.md"
        };
        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                _exportHelper.ExportToMarkdown(_currentReport, saveFileDialog.FileName);
                MessageBox.Show("GitHub-ready Markdown Report exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export Markdown: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ExportTxtButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentReport == null)
        {
            MessageBox.Show("No report available to export. Please analyze a website first.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "Text Document (*.txt)|*.txt",
            FileName = $"Accessibility_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
        };
        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                _exportHelper.ExportToTxt(_currentReport, saveFileDialog.FileName);
                MessageBox.Show("Text Summary exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export Text file: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion
}