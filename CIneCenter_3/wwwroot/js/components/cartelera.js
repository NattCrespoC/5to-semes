/**
 * Cartelera.js - Handles movie listings section functionality
 */

document.addEventListener('DOMContentLoaded', function () {
    // Configuración de paginación
    const MOVIES_PER_PAGE = 6;
    let currentPage = 1;
    let currentMovies = [];
    let currentCategory = 'all';

    // Elements
    const categoryButtons = document.querySelectorAll('.category-btn');
    const categoryPreviews = document.querySelectorAll('.category-preview-container');
    const viewMoviesButtons = document.querySelectorAll('.view-movies-btn');
    const movieGrid = document.getElementById('all-movies-container');
    const paginationContainer = document.getElementById('pagination-container');

    // Initialize with "all" category showing movies directly
    showMovieGrid('all');

    // Handle category button clicks
    categoryButtons.forEach(button => {
        button.addEventListener('click', function () {
            const category = this.getAttribute('data-category');

            // Update active button
            categoryButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');

            // If "all" category, show movies directly, otherwise show preview
            if (category === 'all') {
                // Hide all category previews
                categoryPreviews.forEach(preview => preview.classList.remove('active'));
                // Show all movies
                showMovieGrid(category);
            } else {
                // Hide movie grid
                hideMovieGrid();
                // Show the corresponding category preview
                showCategoryPreview(category);
            }
        });
    });

    // Handle view movies button clicks
    viewMoviesButtons.forEach(button => {
        button.addEventListener('click', function () {
            const category = this.getAttribute('data-category');

            // Hide all category previews
            categoryPreviews.forEach(preview => preview.classList.remove('active'));

            // Show the movie grid with the selected category
            showMovieGrid(category);
        });
    });

    /**
     * Shows the preview for a specific category
     * @param {string} category - The category to show
     */
    function showCategoryPreview(category) {
        // Hide all previews
        categoryPreviews.forEach(preview => preview.classList.remove('active'));

        // Show the selected preview
        const selectedPreview = document.getElementById(`preview-${category}`);
        if (selectedPreview) {
            selectedPreview.classList.add('active');
        }
    }

    /**
     * Shows the movie grid for a specific category
     * @param {string} category - The category to filter by
     */
    function showMovieGrid(category) {
        currentCategory = category; // Store the current category
        movieGrid.classList.add('active');
        paginationContainer.classList.add('active');

        // Load movies based on the category with the pagination system
        loadMoviesByCategory(category);
    }

    /**
     * Hides the movie grid
     */
    function hideMovieGrid() {
        movieGrid.classList.remove('active');
        paginationContainer.classList.remove('active');
    }

    /**
     * Loads movies by category
     * @param {string} category - The category to filter by
     */
    async function loadMoviesByCategory(category) {
        try {
            console.log(`Loading movies for category: ${category}`);
            
            // Show loading indicator
            movieGrid.innerHTML = '<div class="loading-indicator">Cargando películas...</div>';
            
            // Reset to first page when changing categories
            currentPage = 1;
            
            // Use fetch API directly - this avoids module import issues
            let url = category === 'all' 
                ? '/Pelicula/Pelis' 
                : `/Pelicula/Genero?genero=${category}`;
                
            const response = await fetch(url);
            
            if (!response.ok) {
                throw new Error(`Error al cargar películas: ${response.status}`);
            }
            
            const data = await response.json();
            console.log(`Fetched ${data.length} movies for category: ${category}`);
            
            // Transform API response to view model
            const movies = data.map(pelicula => ({
                id: pelicula.id_Peliculas,
                title: pelicula.titulo,
                rating: pelicula.rating || 8,
                poster: `/img/peliculas/${pelicula.imagen}`,
                categories: getCategoriesFromGenero(pelicula.genero, pelicula.estreno),
                genero: pelicula.genero,
                synopsis: pelicula.sinopsis,
                duracion: pelicula.duracion,
                formatos: pelicula.formatos,
                estreno: pelicula.estreno
            }));
            
            // Update current movies and display them with pagination
            currentMovies = [...movies]; // Create a copy to avoid reference issues
            displayMovies();
        } catch (error) {
            console.error(`Error loading movies for category ${category}:`, error);
            movieGrid.innerHTML = `<div class="error-message">Error al cargar películas: ${error.message}</div>`;
        }
    }

    /**
     * Extracts categories from genero string and estreno flag
     */
    function getCategoriesFromGenero(genero, esEstreno) {
        if (!genero) return [];
        
        const categories = [];
        const generos = genero.toLowerCase().split(',').map(g => g.trim());
        
        const mappings = {
            'terror': 'terror',
            'romance': 'amor',
            'amor': 'amor',
            'ciencia ficción': 'ciencia-ficcion',
            'ciencia-ficcion': 'ciencia-ficcion',
            'animación': 'animacion',
            'animacion': 'animacion',
            'acción': 'accion',
            'accion': 'accion',
            'comedia': 'comedia',
            'drama': 'drama',
            'aventura': 'aventura',
            'fantasía': 'fantasia',
            'fantasia': 'fantasia'
        };
        
        generos.forEach(g => {
            if (mappings[g]) {
                categories.push(mappings[g]);
            }
        });
        
        if (esEstreno) categories.push('estreno');
        
        return categories;
    }

    /**
     * Displays the current movies with pagination
     * No parameter needed as we use the currentMovies array
     */
    function displayMovies() {
        // Get current movies
        const movies = currentMovies;
        
        // Log for debugging
        console.log(`Displaying page ${currentPage} of movies. Total: ${movies.length}`);
        
        // Calcular el número total de páginas
        const totalPages = Math.ceil(movies.length / MOVIES_PER_PAGE);
        console.log(`Total pages: ${totalPages}`);
        
        // Asegurar que la página currentPage es válida
        if (currentPage > totalPages) {
            currentPage = 1;
        }
        
        // Calcular índices para la página currentPage
        const startIndex = (currentPage - 1) * MOVIES_PER_PAGE;
        const endIndex = Math.min(startIndex + MOVIES_PER_PAGE, movies.length);
        
        console.log(`Showing movies ${startIndex+1}-${endIndex} of ${movies.length}`);
        
        // Limpiar el contenedor
        movieGrid.innerHTML = '';
        
        // Mostrar sólo las películas de la página actual
        const currentPageMovies = movies.slice(startIndex, endIndex);
        
        // Si no hay películas
        if (currentPageMovies.length === 0) {
            movieGrid.innerHTML = '<p class="no-results">No se encontraron películas que coincidan con tu búsqueda.</p>';
            paginationContainer.innerHTML = '';
            return;
        }
        
        // Mostrar las películas de la página actual
        currentPageMovies.forEach(movie => {
            const movieElement = createMovieCard(movie);
            movieGrid.appendChild(movieElement);
        });
        
        // Generar la paginación
        generatePagination(totalPages);
    }

    /**
     * Creates a movie card element
     * @param {Object} movie - The movie data
     * @returns {HTMLElement} - The movie card element
     */
    function createMovieCard(movie) {
        const card = document.createElement('div');
        card.classList.add('movie-card');
        card.dataset.id = movie.id;
        
        // Add categories as classes instead of data attributes
        if (movie.categories && movie.categories.length) {
            movie.categories.forEach(category => {
                // Use setAttribute instead of dataset to avoid property name issues
                card.setAttribute(`data-category-${category}`, 'true');
                // Also add as a class for easier styling/filtering
                card.classList.add(`category-${category}`);
            });
        }
        
        card.innerHTML = `
            <a href="/Home/Detalles/${movie.id}" class="movie-link">
                <div class="movie-poster">
                    <img src="${movie.poster}" alt="${movie.title}" loading="lazy">
                    <div class="movie-rating"><i class="fas fa-star"></i> ${movie.rating}/10</div>
                </div>
                <div class="movie-info">
                    <h3 class="movie-title">${movie.title}</h3>
                    <div class="movie-categories">
                        ${movie.categories.map(cat => `<span class="category-tag">${getCategoryName(cat)}</span>`).join('')}
                    </div>
                    <span class="ver-detalles-btn">Ver detalles</span>
                </div>
            </a>
        `;
        
        return card;
    }

    /**
     * Get friendly category name
     */
    function getCategoryName(categorySlug) {
        const categories = {
            'estreno': 'Estrenos',
            'terror': 'Terror',
            'romance': 'Amor',
            'ciencia-ficcion': 'Ciencia Ficción',
            'animacion': 'Animación',
            'accion': 'Acción',
            'comedia': 'Comedia',
            'drama': 'Drama',
            'aventura': 'Aventura',
            'fantasia': 'Fantasía'
        };
        return categories[categorySlug] || categorySlug;
    }

    /**
     * Generates pagination controls
     * @param {number} totalPages - The total number of pages
     */
    function generatePagination(totalPages) {
        paginationContainer.innerHTML = '';
        
        if (totalPages <= 1) {
            return; // No mostrar paginación si solo hay una página
        }
        
        // Crear botón "Anterior"
        const prevButton = document.createElement('button');
        prevButton.classList.add('pagination-btn', 'prev-btn');
        prevButton.innerHTML = '&laquo;';
        prevButton.disabled = currentPage <= 1;
        prevButton.addEventListener('click', () => {
            if (currentPage > 1) {
                currentPage--;
                console.log(`Clicked Previous. New page: ${currentPage}`);
                displayMovies();
            }
        });
        paginationContainer.appendChild(prevButton);
        
        // Crear botones numerados
        for (let i = 1; i <= totalPages; i++) {
            const pageButton = document.createElement('button');
            pageButton.classList.add('pagination-btn', 'page-btn');
            if (i === currentPage) {
                pageButton.classList.add('active');
            }
            pageButton.textContent = i;
            pageButton.addEventListener('click', function() {
                currentPage = i;
                console.log(`Clicked Page ${i}`);
                displayMovies();
            });
            paginationContainer.appendChild(pageButton);
        }
        
        // Crear botón "Siguiente"
        const nextButton = document.createElement('button');
        nextButton.classList.add('pagination-btn', 'next-btn');
        nextButton.innerHTML = '&raquo;';
        nextButton.disabled = currentPage >= totalPages;
        nextButton.addEventListener('click', () => {
            if (currentPage < totalPages) {
                currentPage++;
                console.log(`Clicked Next. New page: ${currentPage}`);
                displayMovies();
            }
        });
        paginationContainer.appendChild(nextButton);
        
        // Log current state
        console.log(`Pagination generated with ${totalPages} pages. Current page: ${currentPage}`);
    }
});
