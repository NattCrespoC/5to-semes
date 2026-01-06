document.addEventListener('DOMContentLoaded', function() {
    const carruselWrapper = document.querySelector('.carrusel-wrapper');
    const slides = document.querySelectorAll('.carrusel-slide');
    const prevButton = document.querySelector('.prev-button');
    const nextButton = document.querySelector('.next-button');
    const indicators = document.querySelectorAll('.indicator');
    const comboDescriptions = document.querySelectorAll('.combo-description');
    
    let currentIndex = 0;
    const slideCount = slides.length;
    
    // Función para actualizar el carrusel y las descripciones
    function updateCarousel(index) {
        if (index < 0) index = slideCount - 1;
        if (index >= slideCount) index = 0;
        
        // Actualizar posición del carrusel
        carruselWrapper.style.transform = `translateX(-${index * 25}%)`;
        currentIndex = index;
        
        // Actualizar indicadores
        indicators.forEach((indicator, i) => {
            indicator.classList.toggle('active', i === currentIndex);
        });
        
        // Actualizar descripciones
        comboDescriptions.forEach((description, i) => {
            description.classList.toggle('active', i === currentIndex);
        });
    }
    
    // Event listeners para botones
    prevButton.addEventListener('click', () => {
        updateCarousel(currentIndex - 1);
    });
    
    nextButton.addEventListener('click', () => {
        updateCarousel(currentIndex + 1);
    });
    
    // Event listeners para indicadores
    indicators.forEach((indicator, index) => {
        indicator.addEventListener('click', () => {
            updateCarousel(index);
        });
    });
    
    // Auto-rotación
    let interval = setInterval(() => updateCarousel(currentIndex + 1), 5000);
    
    // Pausar auto-rotación cuando el mouse está sobre el carrusel
    const carruselContainer = document.querySelector('.combos-container');
    carruselContainer.addEventListener('mouseenter', () => {
        clearInterval(interval);
    });
    
    carruselContainer.addEventListener('mouseleave', () => {
        interval = setInterval(() => updateCarousel(currentIndex + 1), 5000);
    });
    
    // También permitir interacción táctil
    let touchStartX = 0;
    let touchEndX = 0;
    
    carruselWrapper.addEventListener('touchstart', (e) => {
        touchStartX = e.changedTouches[0].screenX;
    }, {passive: true});
    
    carruselWrapper.addEventListener('touchend', (e) => {
        touchEndX = e.changedTouches[0].screenX;
        handleSwipe();
    }, {passive: true});
    
    function handleSwipe() {
        const swipeThreshold = 20;
        if (touchEndX < touchStartX - swipeThreshold) {
            // Swipe izquierda (siguiente)
            updateCarousel(currentIndex + 1);
        }
        
        if (touchEndX > touchStartX + swipeThreshold) {
            // Swipe derecha (anterior)
            updateCarousel(currentIndex - 1);
        }
    }
    
    // Inicializar
    updateCarousel(0);

    // Inicializar filtros de categorías
    initCategoryFilters();

    // Inicializar botones de redes sociales si el componente está disponible
    if (typeof SocialButtons !== 'undefined' && document.getElementById('footer-social-buttons')) {
        SocialButtons.createFollowButtons('#footer-social-buttons', {
            size: 'medium',
            shape: 'circle'
        });
    }
});