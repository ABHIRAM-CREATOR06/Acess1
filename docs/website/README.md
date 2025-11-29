# AXIS-CORE Website

A comprehensive website showcasing the AXIS-CORE multi-platform SDK ecosystem for web accessibility checking.

## Overview

This website provides a complete overview of the AXIS-CORE project, including:

- **SDK Ecosystem**: Multi-platform accessibility checking SDKs
- **Interactive Demo**: Try accessibility checking live in your browser
- **Documentation**: Comprehensive guides and API references
- **Installation Guides**: Platform-specific setup instructions
- **Performance Metrics**: Benchmark comparisons across platforms

## Features

### 🎯 Interactive Demo
- Real-time accessibility checking
- Mock API responses for demonstration
- Visual score display with compliance status
- Detailed issue breakdowns

### 📊 SDK Showcase
- Platform comparison matrix
- Installation instructions
- Code examples for each SDK
- Performance benchmarks

### 📚 Documentation Hub
- API reference guides
- Integration tutorials
- Troubleshooting guides
- Best practices

### 🎨 Modern Design
- Responsive layout for all devices
- Smooth animations and transitions
- Accessibility-compliant design
- Dark/light theme support (future)

## File Structure

```
docs/website/
├── index.html          # Main website
├── styles.css          # Styling and responsive design
├── script.js           # Interactive functionality
└── README.md           # This file
```

## Local Development

### Prerequisites
- Modern web browser (Chrome, Firefox, Safari, Edge)
- Local web server (optional, but recommended)

### Running Locally

#### Option 1: Using Python (Simple)
```bash
cd docs/website
python -m http.server 8000
# Open http://localhost:8000
```

#### Option 2: Using Node.js
```bash
cd docs/website
npx http-server -p 8000
# Open http://localhost:8000
```

#### Option 3: Using PHP
```bash
cd docs/website
php -S localhost:8000
# Open http://localhost:8000
```

#### Option 4: Direct File Opening
Simply open `index.html` in your web browser (some features may be limited)

## Features Overview

### Navigation
- Smooth scrolling between sections
- Active section highlighting
- Mobile-responsive navigation

### Hero Section
- Animated code preview
- Key statistics display
- Call-to-action buttons

### SDK Cards
- Platform-specific information
- Installation commands (click to copy)
- Status indicators (Published/Ready)
- Direct links to package registries

### Interactive Demo
- URL input validation
- Loading states and error handling
- Mock accessibility results
- Visual score representation
- Issue categorization and display

### Documentation Links
- Internal documentation navigation
- External resource links
- GitHub integration

### Responsive Design
- Mobile-first approach
- Tablet and desktop optimizations
- Touch-friendly interactions

## Browser Support

- **Chrome**: 90+
- **Firefox**: 88+
- **Safari**: 14+
- **Edge**: 90+

## Performance

- **First Contentful Paint**: < 1.5s
- **Largest Contentful Paint**: < 2.5s
- **Total Bundle Size**: ~50KB (gzipped)
- **Lighthouse Score**: 95+ (Performance, Accessibility, Best Practices, SEO)

## Customization

### Colors and Themes
The website uses CSS custom properties for easy theming:

```css
:root {
    --primary-color: #2563eb;
    --secondary-color: #64748b;
    --background-color: #ffffff;
    --text-primary: #1e293b;
    /* ... more variables */
}
```

### Adding New SDKs
To add a new SDK to the showcase:

1. Add SDK data to the HTML structure in `index.html`
2. Update the comparison matrix
3. Add installation instructions
4. Include performance benchmarks

### Modifying Demo Behavior
The demo uses mock data. To connect to real APIs:

1. Replace `simulateAccessibilityCheck()` in `script.js`
2. Add proper error handling
3. Implement real API calls to your backend

## Deployment

### GitHub Pages
```bash
# The website is automatically available at:
# https://yourusername.github.io/Acess1/docs/website/
```

### Netlify/Vercel
1. Connect your GitHub repository
2. Set build command: `echo "Static site - no build needed"`
3. Set publish directory: `docs/website`
4. Deploy

### Custom Server
Deploy the static files to any web server or CDN.

## Contributing

### Adding Content
1. Update `index.html` for new sections
2. Modify `styles.css` for styling changes
3. Enhance `script.js` for new interactions
4. Test across different browsers and devices

### Code Style
- Use semantic HTML5 elements
- Follow BEM CSS methodology
- Write readable, commented JavaScript
- Ensure accessibility compliance

## Analytics & Tracking

The website includes placeholder functions for analytics:

```javascript
// Track user interactions
trackEvent('demo_check_started', {
    url: 'https://example.com',
    includeEnvironmental: true
});
```

Integrate with your preferred analytics service (Google Analytics, Plausible, etc.).

## Future Enhancements

### Planned Features
- [ ] Dark/light theme toggle
- [ ] Multi-language support (i18n)
- [ ] PWA capabilities (offline access)
- [ ] Advanced demo with real API integration
- [ ] Blog/documentation section
- [ ] Community showcase

### Performance Optimizations
- [ ] Image optimization and lazy loading
- [ ] Service worker for caching
- [ ] Bundle splitting and code splitting
- [ ] CDN integration for assets

## License

This website is part of the AXIS-CORE project and follows the same GPL-3.0-or-later license.

## Support

- **Issues**: [GitHub Issues](https://github.com/ABHIRAM-CREATOR06/Acess1/issues)
- **Discussions**: [GitHub Discussions](https://github.com/ABHIRAM-CREATOR06/Acess1/discussions)
- **Documentation**: [AXIS-CORE SDK Docs](../AXIS-CORE-SDK-DOCS.md)

---

**Made with ❤️ for a more accessible web**