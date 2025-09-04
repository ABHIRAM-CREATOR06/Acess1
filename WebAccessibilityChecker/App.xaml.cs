using System.Configuration;
using System.Data;
using System.Windows;
using QuestPDF;
using QuestPDF.Infrastructure;

namespace WebAccessibilityChecker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        // Configure QuestPDF license for community use
        QuestPDF.Settings.License = LicenseType.Community;
    }
}

