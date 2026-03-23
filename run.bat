@echo off
echo ========================================
echo   🚀 Web Accessibility Checker v2.0
echo ========================================
echo.

cd WebAccessibilityChecker

echo Checking if .NET is available...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET SDK not found. Please install .NET 9.0
    echo Download from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo ✅ .NET found. Starting application...
echo.
echo Note: The app window may appear behind other windows.
echo Look for "🚀 Web Accessibility Checker" in your taskbar.
echo.

REM Check if Release build exists and use it
if exist "bin\Release\net9.0-windows\WebAccessibilityChecker.exe" (
    echo Starting Release build...
    bin\Release\net9.0-windows\WebAccessibilityChecker.exe
) else (
    echo Starting Debug build...
    dotnet run
)

if %errorlevel% neq 0 (
    echo.
    echo ❌ Application failed to start.
    echo Try running as administrator or check the error messages above.
    echo.
)

echo.
echo Press any key to exit...
pause >nul
