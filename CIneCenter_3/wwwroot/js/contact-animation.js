/**
 * Contact and Social Media Animation Script
 * Adds interactive animations and effects to the contact section
 */

document.addEventListener('DOMContentLoaded', function() {
    // Add hover effects to social buttons
    const socialButtons = document.querySelectorAll('.social-float-btn');
    
    socialButtons.forEach(button => {
        button.addEventListener('mouseenter', function() {
            // Create ripple effect
            const ripple = document.createElement('span');
            ripple.classList.add('ripple');
            this.appendChild(ripple);
            
            // Position the ripple
            const rect = button.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            
            ripple.style.width = ripple.style.height = `${size * 2}px`;
            ripple.style.left = `${-size / 2}px`;
            ripple.style.top = `${-size / 2}px`;
            
            // Remove ripple after animation completes
            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });
    
    // Add staggered animation to contact items
    const contactItems = document.querySelectorAll('.contact-item');
    contactItems.forEach((item, index) => {
        item.style.opacity = '0';
        item.style.transform = 'translateY(20px)';
        
        setTimeout(() => {
            item.style.transition = 'all 0.5s ease';
            item.style.opacity = '1';
            item.style.transform = 'translateY(0)';
        }, 300 + (index * 100));
    });
});
