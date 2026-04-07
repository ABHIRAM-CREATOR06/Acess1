# Contributing to AXIS

Thanks for checking this out. AXIS is a solo project right now, so any contribution — bug reports, new checks, SDK work, docs — actually matters.

---

## What needs work

If you're not sure where to start, these are the areas that would genuinely move the project forward:

- **Python SDK** — `axis-core` for Python is planned but not started. If you work in Python and care about a11y tooling, this is the most impactful thing.
- **New WCAG checks** — there are criteria we don't cover yet. See the open issues for specifics.
- **Linux / macOS support** — the core auditing logic is portable, the WPF UI isn't. A CLI wrapper or cross-platform UI would open this up.
- **CI/CD examples** — GitHub Actions, GitLab CI, and similar pipeline integrations using AXIS-CORE SDK.
- **Test coverage** — the rule engine needs more edge case tests, especially for JS-heavy pages.

---

## Getting started

### Prerequisites

- Windows 10 or 11
- .NET 9.0 SDK
- Git

### Setup

```bash
git clone https://github.com/ABHIRAM-CREATOR06/Acess1
cd Acess1
dotnet restore
dotnet run
```

First launch downloads a headless Chrome binary automatically. This takes a minute — it's a one-time thing.

### Running from source vs standalone

```bash
# Dev mode
dotnet run

# Build self-contained binary
dotnet publish -c Release -r win-x64 --self-contained
```

---

## Making changes

```bash
# 1. Fork the repo on GitHub

# 2. Clone your fork
git clone https://github.com/your-username/Acess1

# 3. Create a branch
git checkout -b feature/your-feature-name

# 4. Restore packages
dotnet restore

# 5. Make your changes

# 6. Test manually — run against a few URLs and local HTML files

# 7. Push and open a pull request
git push origin feature/your-feature-name
```

---

## Project structure

```
WebAccessibilityChecker/
├── Models/             # Issue, Report data models
├── Services/           # Parser, Checker — core audit logic
├── Utils/              # Export helpers (TXT, PDF)
├── Resources/          # App resources
├── MainWindow.xaml     # WPF UI layout
└── MainWindow.xaml.cs  # UI logic
```

The audit logic lives in `Services/`. If you're adding a new WCAG check, that's where it goes. Keep each check isolated — one rule, one method.

---

## AXIS-CORE SDK

The SDK is a separate package from the desktop app. It lives in its Axis-CORE [.NET],axis-core-js [npm version],axis-core-rs[cargo] and is published independently to NuGet, npm, and crates.io.

If you're contributing to the SDK:

- Changes to audit logic should be reflected across all three runtimes where possible
- Keep the API surface consistent — `checkUrl()` in JS should behave the same as `CheckUrlAsync()` in .NET
- The Python SDK doesn't exist yet — if you're building it, open an issue first so we can align on the API shape

---

## Submitting a PR

- Keep PRs focused — one thing per PR
- Describe what you changed and why in the PR description
- If it's a new WCAG check, mention the criterion number (e.g. 1.4.3 Contrast Minimum)
- If it fixes a bug, link the issue

No rigid format required. Just make it easy to understand what you did.

---

## Reporting bugs

Open an issue. Include:

- What you were auditing (URL or local file)
- What you expected
- What actually happened
- Your OS and .NET version

---

## Questions

Open an issue or reach out directly — contact is in the README.
