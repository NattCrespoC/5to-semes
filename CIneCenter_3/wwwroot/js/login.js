// Módulo para manejar la apertura del modal de inicio de sesión
document.addEventListener('DOMContentLoaded', function () {
    // Manejo de modales
    const loginBtn = document.getElementById('login-btn');
    const registerBtn = document.getElementById('register-btn');
    const loginModal = document.getElementById('login-modal');
    const registerModal = document.getElementById('register-modal');
    const closeBtns = document.querySelectorAll('.close-modal');

    // Abrir modal de login
    if (loginBtn && loginModal) {
        loginBtn.addEventListener('click', function () {
            loginModal.style.display = 'block';
        });
    }

    // Abrir modal de registro
    if (registerBtn && registerModal) {
        registerBtn.addEventListener('click', function () {
            registerModal.style.display = 'block';
        });
    }

    // Cerrar modales con el botón X
    closeBtns.forEach(function(btn) {
        btn.addEventListener('click', function() {
            const modal = btn.closest('.modal');
            if (modal) modal.style.display = 'none';
        });
    });

    // Cerrar modales haciendo click fuera
    window.addEventListener('click', function(event) {
        if (event.target === loginModal) loginModal.style.display = 'none';
        if (event.target === registerModal) registerModal.style.display = 'none';
    });

    // Formulario de login
    const loginForm = document.getElementById('login-form');
    if (loginForm) {
        loginForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const formData = new FormData(loginForm);
            const loginError = document.getElementById('login-error');
            
            fetch('/Acceso/LoginAjax', {
                method: 'POST',
                body: formData
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    window.location.href = data.redirectUrl;
                } else {
                    loginError.style.display = 'block';
                    loginError.innerHTML = data.errors.join('<br>');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                loginError.style.display = 'block';
                loginError.textContent = 'Ha ocurrido un error. Inténtalo de nuevo más tarde.';
            });
        });
    }

    // Formulario de registro
    const registerForm = document.getElementById('register-form');
    if (registerForm) {
        registerForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            // Limpiar mensajes de error previos
            document.querySelectorAll('.field-validation-error').forEach(el => el.textContent = '');
            const registerError = document.getElementById('register-error');
            registerError.style.display = 'none';
            
            // Validación client-side
            let isValid = true;
            
            // Validar nombre completo
            const nombreCompleto = document.getElementById('NombreCompleto');
            if (!nombreCompleto.value.trim()) {
                document.getElementById('NombreCompleto-error').textContent = 'El nombre es obligatorio';
                isValid = false;
            } else if (nombreCompleto.value.trim().length < 3) {
                document.getElementById('NombreCompleto-error').textContent = 'El nombre debe tener al menos 3 caracteres';
                isValid = false;
            }
            
            // Validar email
            const email = document.getElementById('Email');
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!email.value.trim()) {
                document.getElementById('Email-error').textContent = 'El correo electrónico es obligatorio';
                isValid = false;
            } else if (!emailRegex.test(email.value.trim())) {
                document.getElementById('Email-error').textContent = 'Ingrese un correo electrónico válido';
                isValid = false;
            }
            
            // Validar contraseña
            const password = document.getElementById('Password');
            if (!password.value) {
                document.getElementById('Password-error').textContent = 'La contraseña es obligatoria';
                isValid = false;
            } else if (password.value.length < 6) {
                document.getElementById('Password-error').textContent = 'La contraseña debe tener al menos 6 caracteres';
                isValid = false;
            }
            
            // Validar confirmación de contraseña
            const confirmPassword = document.getElementById('ConfirmPassword');
            if (!confirmPassword.value) {
                document.getElementById('ConfirmPassword-error').textContent = 'Debe confirmar la contraseña';
                isValid = false;
            } else if (password.value !== confirmPassword.value) {
                document.getElementById('ConfirmPassword-error').textContent = 'Las contraseñas no coinciden';
                isValid = false;
            }
            
            if (!isValid) {
                return;
            }
            
            const formData = new FormData(registerForm);
            
            fetch('/Acceso/RegistroAjax', {
                method: 'POST',
                body: formData
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Recargar la página para mostrar la nueva sesión
                    window.location.reload();
                } else {
                    registerError.style.display = 'block';
                    registerError.innerHTML = data.errors.join('<br>');
                    
                    // Mostrar errores específicos de campo si existen
                    if (data.fieldErrors) {
                        for (const [field, errors] of Object.entries(data.fieldErrors)) {
                            const errorSpan = document.getElementById(`${field}-error`);
                            if (errorSpan) {
                                errorSpan.textContent = errors[0];
                            }
                        }
                    }
                }
            })
            .catch(error => {
                console.error('Error:', error);
                registerError.style.display = 'block';
                registerError.textContent = 'Ha ocurrido un error. Inténtalo de nuevo más tarde.';
            });
        });
    }
});
