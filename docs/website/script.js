// Smooth scrolling for navigation links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// Navbar background change on scroll
window.addEventListener('scroll', () => {
    const navbar = document.querySelector('.navbar');
    if (window.scrollY > 50) {
        navbar.style.background = 'rgba(255, 255, 255, 0.98)';
        navbar.style.boxShadow = '0 2px 10px rgba(0, 0, 0, 0.1)';
    } else {
        navbar.style.background = 'rgba(255, 255, 255, 0.95)';
        navbar.style.boxShadow = 'none';
    }
});

// Active navigation link highlighting
window.addEventListener('scroll', () => {
    const sections = document.querySelectorAll('section[id]');
    const navLinks = document.querySelectorAll('.nav-links a');

    let current = '';
    sections.forEach(section => {
        const sectionTop = section.offsetTop - 100;
        if (window.scrollY >= sectionTop) {
            current = section.getAttribute('id');
        }
    });

    navLinks.forEach(link => {
        link.classList.remove('active');
        if (link.getAttribute('href') === `#${current}`) {
            link.classList.add('active');
        }
    });
});

// Demo functionality
document.getElementById('check-btn').addEventListener('click', async () => {
    const url = document.getElementById('demo-url').value;
    const resultsDiv = document.getElementById('demo-results');
    const checkBtn = document.getElementById('check-btn');

    if (!url) {
        alert('Please enter a URL to check');
        return;
    }

    // Show loading state
    checkBtn.innerHTML = '<i class="fas fa-spinner loading"></i> Checking...';
    checkBtn.disabled = true;

    try {
        // Simulate API call (in real implementation, this would call the actual SDK)
        await simulateAccessibilityCheck(url);

        // Show results
        resultsDiv.style.display = 'block';
        resultsDiv.scrollIntoView({ behavior: 'smooth', block: 'center' });

    } catch (error) {
        alert('Error checking accessibility: ' + error.message);
    } finally {
        // Reset button
        checkBtn.innerHTML = '<i class="fas fa-search"></i> Check Accessibility';
        checkBtn.disabled = false;
    }
});

// Simulate accessibility check (mock implementation)
async function simulateAccessibilityCheck(url) {
    // Simulate network delay
    await new Promise(resolve => setTimeout(resolve, 2000));

    // Mock results based on URL
    const mockResults = {
        score: 85,
        complianceStatus: 'Mostly Compliant',
        errorCount: 1,
        warningCount: 2,
        infoCount: 0,
        issues: [
            {
                type: 'Missing Alt Text',
                severity: 'Error',
                description: 'Add alt attribute describing the image content for screen readers'
            },
            {
                type: 'Missing Viewport Meta',
                severity: 'Warning',
                description: 'Add viewport meta tag for proper mobile responsiveness'
            },
            {
                type: 'Low Color Contrast',
                severity: 'Warning',
                description: 'Ensure text meets WCAG contrast requirements (4.5:1 ratio)'
            }
        ]
    };

    // Update UI with results
    document.getElementById('score-text').textContent = mockResults.score;
    document.getElementById('compliance-status').textContent = mockResults.complianceStatus;
    document.getElementById('error-count').textContent = mockResults.errorCount;
    document.getElementById('warning-count').textContent = mockResults.warningCount;
    document.getElementById('info-count').textContent = mockResults.infoCount;

    // Update score circle color based on score
    const scoreCircle = document.getElementById('score-circle');
    if (mockResults.score >= 90) {
        scoreCircle.style.background = '#10b981'; // Green
    } else if (mockResults.score >= 70) {
        scoreCircle.style.background = '#f59e0b'; // Yellow
    } else {
        scoreCircle.style.background = '#ef4444'; // Red
    }

    // Update issues list
    const issuesList = document.getElementById('issues-list');
    issuesList.innerHTML = '';

    mockResults.issues.forEach(issue => {
        const issueElement = document.createElement('div');
        issueElement.className = `issue-item ${issue.severity.toLowerCase()}`;

        issueElement.innerHTML = `
            <div class="issue-header">
                <span class="issue-type">${issue.type}</span>
                <span class="issue-severity">${issue.severity}</span>
            </div>
            <p class="issue-description">${issue.description}</p>
        `;

        issuesList.appendChild(issueElement);
    });
}

// Modal functionality
function showPublishModal(sdk) {
    const modal = document.getElementById('publish-modal');
    const modalTitle = document.getElementById('modal-title');

    modalTitle.textContent = `Publish ${sdk} SDK`;
    modal.style.display = 'block';
}

document.querySelector('.modal-close').addEventListener('click', () => {
    document.getElementById('publish-modal').style.display = 'none';
});

window.addEventListener('click', (e) => {
    const modal = document.getElementById('publish-modal');
    if (e.target === modal) {
        modal.style.display = 'none';
    }
});

// Intersection Observer for animations
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('fade-in');
        }
    });
}, observerOptions);

// Observe elements for animation
document.querySelectorAll('.sdk-card, .feature-card, .doc-card, .stat-card').forEach(card => {
    observer.observe(card);
});

// Typing animation for hero code
function typeWriter(element, text, speed = 50) {
    let i = 0;
    element.innerHTML = '';

    function type() {
        if (i < text.length) {
            element.innerHTML += text.charAt(i);
            i++;
            setTimeout(type, speed);
        }
    }

    type();
}

// Initialize typing animation on page load
document.addEventListener('DOMContentLoaded', () => {
    const codeElement = document.querySelector('.code-content code');
    if (codeElement) {
        const originalText = codeElement.textContent;
        setTimeout(() => {
            typeWriter(codeElement, originalText, 30);
        }, 1000);
    }
});

// Copy to clipboard functionality
document.querySelectorAll('.sdk-install code').forEach(codeElement => {
    codeElement.addEventListener('click', () => {
        const text = codeElement.textContent;
        navigator.clipboard.writeText(text).then(() => {
            // Show temporary feedback
            const originalText = codeElement.textContent;
            codeElement.textContent = 'Copied!';
            codeElement.style.color = '#10b981';

            setTimeout(() => {
                codeElement.textContent = originalText;
                codeElement.style.color = '';
            }, 1000);
        });
    });

    // Add cursor pointer to indicate clickability
    codeElement.style.cursor = 'pointer';
    codeElement.title = 'Click to copy';
});

// Performance monitoring (for demo purposes)
window.addEventListener('load', () => {
    // Log performance metrics
    if ('performance' in window) {
        const perfData = performance.getEntriesByType('navigation')[0];
        console.log('Page load time:', perfData.loadEventEnd - perfData.loadEventStart, 'ms');
    }

    // Add fade-in animation to hero content
    document.querySelector('.hero-content').classList.add('fade-in');
});

// Error handling for demo
window.addEventListener('error', (e) => {
    console.error('JavaScript error:', e.error);
    // In production, you might want to send this to an error tracking service
});

// Service worker registration (for PWA capabilities - optional)
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        // Register service worker for offline capabilities (future enhancement)
        // navigator.serviceWorker.register('/sw.js');
    });
}

// Keyboard navigation support
document.addEventListener('keydown', (e) => {
    // Close modal with Escape key
    if (e.key === 'Escape') {
        document.getElementById('publish-modal').style.display = 'none';
    }

    // Skip to main content with Tab (accessibility)
    if (e.key === 'Tab') {
        const focusableElements = document.querySelectorAll(
            'a[href], button, input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        // Focus management logic can be added here
    }
});

// Responsive navigation toggle (for mobile - future enhancement)
function toggleMobileNav() {
    const navLinks = document.querySelector('.nav-links');
    navLinks.classList.toggle('mobile-open');
}

// Analytics tracking (placeholder for future implementation)
function trackEvent(eventName, properties = {}) {
    // In production, integrate with analytics service
    console.log('Track event:', eventName, properties);
}

// Track demo usage
document.getElementById('check-btn').addEventListener('click', () => {
    trackEvent('demo_check_started', {
        url: document.getElementById('demo-url').value,
        includeEnvironmental: document.getElementById('include-environmental').checked,
        detailedReport: document.getElementById('detailed-report').checked
    });
});

// Export functionality for documentation
window.exportToPDF = function() {
    // Future: Implement PDF export of documentation
    alert('PDF export feature coming soon!');
};

window.exportToMarkdown = function() {
    // Future: Implement Markdown export
    alert('Markdown export feature coming soon!');
};

// Theme toggle (future enhancement)
window.toggleTheme = function() {
    document.body.classList.toggle('dark-theme');
    localStorage.setItem('theme', document.body.classList.contains('dark-theme') ? 'dark' : 'light');
};

// Load saved theme
document.addEventListener('DOMContentLoaded', () => {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
        document.body.classList.add('dark-theme');
    }
});