/**
 * Social Media Buttons Component
 * 
 * A reusable module for adding social media buttons to any page.
 * Usage:
 * 1. Include this script on your page
 * 2. Call SocialButtons.create(containerElement, options) to add buttons
 */

const SocialButtons = (function() {
    // Default configuration
    const defaults = {
        showLabels: false,
        size: 'medium', // small, medium, large
        shape: 'circle', // circle, rounded, square
        platforms: ['facebook', 'instagram', 'twitter', 'youtube', 'whatsapp', 'tiktok'],
        links: {
            facebook: 'https://www.facebook.com/CineCenterBolivia',
            instagram: 'https://www.instagram.com/cinecenterbo',
            twitter: 'https://x.com/cine_center',
            youtube: 'https://www.youtube.com/@cinecenterbolivia860',
            whatsapp: 'https://wa.me/59170721092',
            tiktok: 'https://www.tiktok.com/@cinecenterbo'
        },
        shareData: null, // For share buttons mode
        shareMode: false // If true, becomes a share button instead of profile link
    };
    
    // Icons map with platform name to FontAwesome icon class
    const icons = {
        facebook: 'fab fa-facebook-f',
        instagram: 'fab fa-instagram',
        twitter: 'fab fa-x-twitter',
        youtube: 'fab fa-youtube',
        whatsapp: 'fab fa-whatsapp',
        tiktok: 'fab fa-tiktok',
        email: 'fas fa-envelope',
        print: 'fas fa-print',
        copy: 'fas fa-copy'
    };
    
    // Labels for platforms
    const labels = {
        facebook: 'Facebook',
        instagram: 'Instagram',
        twitter: 'Twitter',
        youtube: 'YouTube',
        whatsapp: 'WhatsApp',
        tiktok: 'TikTok',
        email: 'Email',
        print: 'Imprimir',
        copy: 'Copiar'
    };
    
    /**
     * Create social media buttons in a container
     * @param {HTMLElement|string} container - Container element or CSS selector
     * @param {Object} options - Configuration options
     */
    function create(container, options = {}) {
        // Merge default options with provided options
        const settings = { ...defaults, ...options };
        
        // Get container element if string was provided
        if (typeof container === 'string') {
            container = document.querySelector(container);
        }
        
        if (!container) {
            console.error('Social Buttons: Container element not found');
            return;
        }
        
        // Create buttons container
        const buttonsContainer = document.createElement('div');
        buttonsContainer.className = `social-buttons ${settings.size} ${settings.shape}`;
        
        // Create buttons for each platform
        settings.platforms.forEach(platform => {
            const button = createButton(platform, settings);
            buttonsContainer.appendChild(button);
        });
        
        // Add buttons to container
        container.appendChild(buttonsContainer);
    }
    
    /**
     * Create a single social media button
     * @param {string} platform - Platform name
     * @param {Object} settings - Configuration settings
     * @returns {HTMLElement} - Button element
     */
    function createButton(platform, settings) {
        const button = document.createElement('a');
        button.className = `social-btn ${platform}`;
        button.setAttribute('aria-label', labels[platform] || platform);
        
        // Set target and href based on mode
        if (settings.shareMode) {
            button.href = '#';
            button.addEventListener('click', (e) => {
                e.preventDefault();
                shareContent(platform, settings.shareData);
            });
        } else {
            button.href = settings.links[platform] || '#';
            button.target = '_blank';
            button.rel = 'noopener noreferrer';
        }
        
        // Create icon
        const icon = document.createElement('i');
        icon.className = icons[platform] || 'fas fa-share';
        button.appendChild(icon);
        
        // Add label if needed
        if (settings.showLabels) {
            const label = document.createElement('span');
            label.className = 'social-btn-label';
            label.textContent = labels[platform] || platform;
            button.appendChild(label);
        }
        
        return button;
    }
    
    /**
     * Share content on different platforms
     * @param {string} platform - Platform to share on
     * @param {Object} data - Data to share (title, text, url)
     */
    function shareContent(platform, data = {}) {
        const title = data.title || document.title;
        const text = data.text || '';
        const url = data.url || window.location.href;
        
        switch (platform) {
            case 'facebook':
                window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(url)}`, '_blank');
                break;
            case 'twitter':
                window.open(`https://twitter.com/intent/tweet?text=${encodeURIComponent(title)}&url=${encodeURIComponent(url)}`, '_blank');
                break;
            case 'whatsapp':
                window.open(`https://wa.me/?text=${encodeURIComponent(title + ' ' + url)}`, '_blank');
                break;
            case 'email':
                window.location.href = `mailto:?subject=${encodeURIComponent(title)}&body=${encodeURIComponent(text + ' ' + url)}`;
                break;
            case 'copy':
                navigator.clipboard.writeText(url)
                    .then(() => alert('Enlace copiado al portapapeles'))
                    .catch(err => console.error('Error copiando al portapapeles:', err));
                break;
            default:
                console.warn(`Sharing on ${platform} is not implemented`);
        }
    }
    
    /**
     * Create share buttons with predefined platforms for sharing
     * @param {HTMLElement|string} container - Container element or CSS selector
     * @param {Object} options - Configuration options
     */
    function createShareButtons(container, options = {}) {
        // Default share platforms
        const sharePlatforms = ['facebook', 'twitter', 'whatsapp', 'email', 'copy'];
        
        // Create with share mode enabled
        create(container, {
            platforms: options.platforms || sharePlatforms,
            shareMode: true,
            shareData: options.shareData || {},
            ...options
        });
    }
    
    /**
     * Create follow buttons with predefined platforms for following
     * @param {HTMLElement|string} container - Container element or CSS selector
     * @param {Object} options - Configuration options
     */
    function createFollowButtons(container, options = {}) {
        create(container, options);
    }
    
    // Public API
    return {
        create,
        createShareButtons,
        createFollowButtons
    };
})();
