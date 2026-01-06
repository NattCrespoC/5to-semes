/**
 * Funcionalidades de UI para el sitio Cine Center
 * Incluye gestión del menú móvil, dropdown de usuario y modales
 */

// Inicializar cuando el DOM esté cargado
document.addEventListener('DOMContentLoaded', function() {
    // Código para el botón de menú hamburguesa
    initMobileMenu();
    
    // Inicializar dropdown de usuario
    initUserDropdown();
    
    // Inicializar modales de autenticación
    initAuthModals();
    
    // Configurar confirmación de logout
    setupLogoutConfirmation();
    
    // Mostrar notificaciones si existen
    showNotifications();
});

/**
 * Inicializa la funcionalidad del menú móvil
 */
function initMobileMenu() {
    const menuToggle = document.getElementById('menu-toggle');
    const menuContainer = document.querySelector('.menu-container');
    
    if (menuToggle && menuContainer) {
        menuToggle.addEventListener('click', function() {
            menuContainer.classList.toggle('menu-open');
            // Cambiar el icono según el estado del menú
            const icon = menuToggle.querySelector('i');
            if (menuContainer.classList.contains('menu-open')) {
                icon.classList.remove('fa-bars');
                icon.classList.add('fa-times');
            } else {
                icon.classList.remove('fa-times');
                icon.classList.add('fa-bars');
            }
        });
    }
}

/**
 * Inicializa la funcionalidad del dropdown de usuario
 */
function initUserDropdown() {
    const userMenuButton = document.getElementById('user-menu-button');
    
    if (userMenuButton) {
        userMenuButton.addEventListener('click', function(e) {
            e.preventDefault();
            const dropdownMenu = document.getElementById('user-dropdown-menu');
            if (dropdownMenu) {
                dropdownMenu.classList.toggle('show');
            }
        });
        
        // Cerrar dropdown cuando se hace clic fuera
        document.addEventListener('click', function(event) {
            if (!event.target.closest('#user-profile-dropdown')) {
                const dropdownMenu = document.getElementById('user-dropdown-menu');
                if (dropdownMenu && dropdownMenu.classList.contains('show')) {
                    dropdownMenu.classList.remove('show');
                }
            }
        });
    }
}

/**
 * Inicializa los modales de autenticación
 */
function initAuthModals() {
    // Login button handler
    const loginBtn = document.getElementById('login-btn');
    if (loginBtn) {
        loginBtn.addEventListener('click', function() {
            const loginModal = document.getElementById('login-modal');
            if (loginModal) {
                const returnUrlInput = document.getElementById('returnUrl');
                if (returnUrlInput) {
                    returnUrlInput.value = window.location.href;
                }
                loginModal.style.display = 'block';
            }
        });
    }
    
    // Register button handler
    const registerBtn = document.getElementById('register-btn');
    if (registerBtn) {
        registerBtn.addEventListener('click', function() {
            const registerModal = document.getElementById('register-modal');
            if (registerModal) {
                registerModal.style.display = 'block';
            }
        });
    }
    
    // Close buttons for modals
    const closeButtons = document.querySelectorAll('.close-modal');
    closeButtons.forEach(function(btn) {
        btn.addEventListener('click', function() {
            const modal = btn.closest('.modal');
            if (modal) {
                modal.style.display = 'none';
            }
        });
    });
}

/**
 * Configura la confirmación para cerrar sesión
 */
function setupLogoutConfirmation() {
    const logoutLink = document.getElementById('logout-link');
    if (logoutLink) {
        logoutLink.addEventListener('click', function(e) {
            if (!confirm('¿Estás seguro que deseas cerrar sesión?')) {
                e.preventDefault();
            }
        });
    }
}

/**
 * Muestra notificaciones toast si existen en TempData
 */
function showNotifications() {
    // Esta función debe ser llamada con el mensaje desde el Razor view
    // usando site-ui.showToastMessage('mensaje')
}

/**
 * Muestra un mensaje toast
 * @param {string} message - El mensaje a mostrar
 * @param {string} type - El tipo de mensaje (success, error, etc)
 */
function showToastMessage(message, type = 'success') {
    if (!message) return;
    
    // Create a toast notification
    const toast = document.createElement('div');
    toast.className = 'toast-notification';
    toast.innerHTML = `
        <div class="toast-content">
            <i class="fas fa-check-circle toast-icon"></i>
            <div class="toast-message">${message}</div>
        </div>
    `;
    document.body.appendChild(toast);
    
    // Show then hide the toast after a delay
    setTimeout(() => {
        toast.classList.add('show');
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => {
                document.body.removeChild(toast);
            }, 500);
        }, 3000);
    }, 100);
}
