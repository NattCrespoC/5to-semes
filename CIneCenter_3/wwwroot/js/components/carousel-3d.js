document.addEventListener('DOMContentLoaded', function () {
    // Referencias a los elementos del DOM
    const carousel = document.querySelector('.special-carousel-inner');
    const slides = document.querySelectorAll('.special-slide');
    const prevBtn = document.querySelector('.prev-special');
    const nextBtn = document.querySelector('.next-special');
    const indicators = document.querySelectorAll('.special-indicator');
    const attractionDetails = document.querySelectorAll('.attraction-details');

    // Variables de control
    let currentSlide = 0;
    const totalSlides = slides.length;

    // Inicializar el carrusel
    function updateCarousel() {
        // Actualizar la posición del carrusel
        carousel.style.transform = `translateX(-${currentSlide * 100}%)`;

        // Actualizar las clases activas de los slides
        slides.forEach((slide, index) => {
            slide.classList.toggle('active', index === currentSlide);
        });

        // Actualizar los indicadores
        indicators.forEach((indicator, index) => {
            indicator.classList.toggle('active', index === currentSlide);
        });

        // Actualizar la información de detalles de atracción
        attractionDetails.forEach((detail, index) => {
            detail.classList.toggle('active', index === currentSlide);
        });
    }

    // Avanzar al siguiente slide
    function nextSlide() {
        currentSlide = (currentSlide + 1) % totalSlides;
        updateCarousel();
    }

    // Retroceder al slide anterior
    function prevSlide() {
        currentSlide = (currentSlide - 1 + totalSlides) % totalSlides;
        updateCarousel();
    }

    // Event listeners para botones de navegación
    if (prevBtn) {
        prevBtn.addEventListener('click', function (e) {
            e.preventDefault();
            prevSlide();
            resetAutoSlide(); // Reiniciar el temporizador automático después de interacción manual
        });
    }

    if (nextBtn) {
        nextBtn.addEventListener('click', function (e) {
            e.preventDefault();
            nextSlide();
            resetAutoSlide(); // Reiniciar el temporizador automático después de interacción manual
        });
    }

    // Event listeners para indicadores
    indicators.forEach((indicator, index) => {
        indicator.addEventListener('click', () => {
            currentSlide = index;
            updateCarousel();
            resetAutoSlide(); // Reiniciar el temporizador automático después de interacción manual
        });
    });

    // Event listeners para slides
    slides.forEach((slide, index) => {
        slide.addEventListener('click', () => {
            const attractionId = slide.dataset.attraction;
            // Activar slide y su correspondiente detalle
            currentSlide = index;
            updateCarousel();
            resetAutoSlide(); // Reiniciar el temporizador automático después de interacción manual
        });
    });

    // Auto-rotación del carrusel
    let autoSlideInterval = setInterval(nextSlide, 5000);

    function resetAutoSlide() {
        clearInterval(autoSlideInterval);
        autoSlideInterval = setInterval(nextSlide, 5000);
    }

    // Pausar auto-rotación al hover
    const carouselContainer = document.querySelector('.special-carousel');
    if (carouselContainer) {
        carouselContainer.addEventListener('mouseenter', () => {
            clearInterval(autoSlideInterval);
        });

        carouselContainer.addEventListener('mouseleave', () => {
            autoSlideInterval = setInterval(nextSlide, 5000);
        });
    }

    // Soporte para gestos táctiles
    let touchStartX = 0;
    let touchEndX = 0;

    if (carousel) {
        carousel.addEventListener('touchstart', e => {
            touchStartX = e.changedTouches[0].screenX;
        }, { passive: true });

        carousel.addEventListener('touchend', e => {
            touchEndX = e.changedTouches[0].screenX;
            handleSwipe();
        }, { passive: true });
    }

    function handleSwipe() {
        const swipeThreshold = 50;
        if (touchEndX < touchStartX - swipeThreshold) {
            // Deslizar a la izquierda (siguiente)
            nextSlide();
            resetAutoSlide();
        }
        if (touchEndX > touchStartX + swipeThreshold) {
            // Deslizar a la derecha (anterior)
            prevSlide();
            resetAutoSlide();
        }
    }

    // Crear partículas para el fondo
    function createParticles() {
        const particlesContainer = document.querySelector('.particles');
        if (!particlesContainer) return;

        const particleCount = 25;

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.classList.add('particle');

            // Posición aleatoria
            const posX = Math.random() * 100;
            const delay = Math.random() * 5;
            const duration = 6 + Math.random() * 4;
            const size = 1 + Math.random() * 3;

            particle.style.left = `${posX}%`;
            particle.style.animationDelay = `${delay}s`;
            particle.style.animationDuration = `${duration}s`;
            particle.style.width = `${size}px`;
            particle.style.height = `${size}px`;

            particlesContainer.appendChild(particle);
        }
    }

    // Inicializar carrusel y efectos
    updateCarousel();
    createParticles();

    console.log('Carrusel especial inicializado');
});
