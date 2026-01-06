/**
 * Sistema de pago para la selección de asientos
 * Maneja el modal de pago con sus diferentes pasos y métodos de pago
 */

// Objeto principal para el sistema de pago
const paymentSystem = {
    modal: null,
    currentStep: 1,
    totalSteps: 3,
    selectedMethod: null,
    timerInterval: null,
    timeLeft: 600, // 10 minutos en segundos
    paymentData: null,
    selectedSeats: [],
    customCursor: null,
    
    // Inicializar el sistema de pago
    init: function() {
        this.modal = document.getElementById('payment-modal');
        if (!this.modal) {
            console.error('Error: No se encontró el modal de pago');
            return;
        }
        
        console.log('Sistema de pago inicializado');
        this.setupEventListeners();
        this.initCustomCursor();
    },
    
    // Inicializar el cursor personalizado
    initCustomCursor: function() {
        // Crear el elemento del cursor personalizado
        this.customCursor = document.createElement('div');
        this.customCursor.classList.add('custom-cursor');
        this.customCursor.style.cssText = `
            position: fixed;
            width: 12px;
            height: 12px;
            background: rgba(153, 51, 204, 0.7);
            border-radius: 50%;
            box-shadow: 0 0 10px rgba(0, 204, 255, 0.8);
            pointer-events: none;
            z-index: 1000;
            transform: translate(-50%, -50%);
            mix-blend-mode: difference;
            display: none;
            animation: cursorPulse 1.5s infinite;
        `;
        document.body.appendChild(this.customCursor);
        
        // Agregar evento global para rastrear el cursor en la página
        document.addEventListener('mousemove', this.updateCustomCursor.bind(this));
    },
    
    // Actualizar la posición del cursor personalizado
    updateCustomCursor: function(e) {
        if (this.customCursor) {
            const qrContainer = document.querySelector('.qr-container');
            if (qrContainer) {
                const rect = qrContainer.getBoundingClientRect();
                
                // Verificar si el mouse está dentro del contenedor QR
                if (
                    e.clientX >= rect.left &&
                    e.clientX <= rect.right &&
                    e.clientY >= rect.top &&
                    e.clientY <= rect.bottom
                ) {
                    this.customCursor.style.display = 'block';
                    this.customCursor.style.left = e.clientX + 'px';
                    this.customCursor.style.top = e.clientY + 'px';
                } else {
                    this.customCursor.style.display = 'none';
                }
            }
        }
    },
    
    // Configurar los event listeners
    setupEventListeners: function() {
        // Cerrar modal
        document.querySelectorAll('.close-payment-modal').forEach(btn => {
            btn.addEventListener('click', () => this.closeModal());
        });
        
        // Navegación entre pasos
        document.querySelectorAll('.payment-step-button').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const action = e.target.getAttribute('data-action');
                if (action === 'next') this.nextStep();
                if (action === 'prev') this.prevStep();
                if (action === 'finish' && this.currentStep === 2) this.processPayment();
                if (action === 'finish' && this.currentStep === 3) this.finishPayment();
            });
        });
        
        // Selección de método de pago
        document.querySelectorAll('.payment-method-option').forEach(option => {
            option.addEventListener('click', (e) => {
                const method = e.currentTarget.getAttribute('data-method');
                this.selectPaymentMethod(method);
            });
        });
        
        // Validación de tarjeta de crédito
        if (document.getElementById('card-payment-form')) {
            document.getElementById('card-number').addEventListener('input', this.formatCardNumber);
            document.getElementById('card-expiry').addEventListener('input', this.formatCardExpiry);
            document.getElementById('card-cvv').addEventListener('input', this.validateCardCVV);
            document.getElementById('card-holder').addEventListener('input', this.validateCardHolder);
        }
        
        // Eventos para animación QR
        document.addEventListener('mousemove', (e) => {
            const qrWrapper = document.querySelector('.qr-wrapper');
            if (qrWrapper) {
                const rect = qrWrapper.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;
                
                // Calcular la rotación 3D basada en la posición del mouse
                const centerX = rect.width / 2;
                const centerY = rect.height / 2;
                const rotateX = (y - centerY) / 20;
                const rotateY = (centerX - x) / 20;
                
                qrWrapper.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg)`;
            }
        });
    },
    
    // Abrir el modal de pago
    openPaymentModal: function(selectedSeats) {
        console.log('Abriendo modal de pago', selectedSeats);
        this.selectedSeats = selectedSeats;
        
        // Verificar si el usuario está autenticado
        const isAuthenticated = document.getElementById('is-authenticated').value === 'true';
        if (!isAuthenticated) {
            alert('Debe iniciar sesión para comprar tickets');
            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
            return;
        }
        
        // Mostrar el modal
        this.modal.style.display = 'flex';
        this.resetModal();
        
        // Establecer monto total
        const totalPrice = document.getElementById('total-price').textContent;
        document.querySelectorAll('.modal-total-amount').forEach(el => {
            el.textContent = totalPrice;
        });
        
        // Iniciar temporizador
        this.startTimer();
    },
    
    // Cerrar el modal de pago
    closeModal: function() {
        this.modal.style.display = 'none';
        this.stopTimer();
    },
    
    // Resetear el modal a su estado inicial
    resetModal: function() {
        this.currentStep = 1;
        this.selectedMethod = null;
        this.updateStepIndicators();
        this.showCurrentStep();
        
        // Reset payment method selection
        document.querySelectorAll('.payment-method-option').forEach(option => {
            option.classList.remove('selected');
        });
        document.querySelector('.payment-step-button[data-action="next"]').disabled = true;
        
        // Reset form if exists
        if (document.getElementById('card-payment-form')) {
            document.getElementById('card-payment-form').reset();
        }
    },
    
    // Mostrar el paso actual
    showCurrentStep: function() {
        document.querySelectorAll('.payment-step').forEach(step => {
            step.style.display = 'none';
        });
        document.getElementById(`payment-step-${this.currentStep}`).style.display = 'block';
    },
    
    // Actualizar los indicadores de paso
    updateStepIndicators: function() {
        document.querySelectorAll('.step-indicator').forEach((indicator, index) => {
            indicator.classList.remove('current', 'completed');
            if (index + 1 === this.currentStep) {
                indicator.classList.add('current');
            } else if (index + 1 < this.currentStep) {
                indicator.classList.add('completed');
            }
        });
        
        // Actualizar la barra de progreso
        const progressBar = document.querySelector('.payment-progress-bar > div');
        const progressPercentage = ((this.currentStep - 1) / (this.totalSteps - 1)) * 100;
        progressBar.style.width = `${progressPercentage}%`;
    },
    
    // Ir al siguiente paso
    nextStep: function() {
        if (this.currentStep < this.totalSteps) {
            this.currentStep++;
            this.updateStepIndicators();
            this.showCurrentStep();
            
            // Mostrar contenedor específico según método de pago
            if (this.currentStep === 2) {
                this.showPaymentDetails();
            }
        }
    },
    
    // Ir al paso anterior
    prevStep: function() {
        if (this.currentStep > 1) {
            this.currentStep--;
            this.updateStepIndicators();
            this.showCurrentStep();
        }
    },
    
    // Seleccionar método de pago
    selectPaymentMethod: function(method) {
        this.selectedMethod = method;
        
        // Actualizar interfaz
        document.querySelectorAll('.payment-method-option').forEach(option => {
            option.classList.remove('selected');
        });
        document.querySelector(`.payment-method-option[data-method="${method}"]`).classList.add('selected');
        document.getElementById('selected-payment-method').value = method;
        
        // Habilitar botón de continuar
        document.querySelector('.payment-step-button[data-action="next"]').disabled = false;
    },
    
    // Mostrar detalles según método de pago seleccionado
    showPaymentDetails: function() {
        // Ocultar todos los contenedores de detalles
        document.querySelectorAll('.payment-details-section').forEach(container => {
            container.style.display = 'none';
        });
        
        // Mostrar el contenedor correspondiente
        if (this.selectedMethod === 'qr') {
            document.getElementById('qr-payment-container').style.display = 'block';
            this.generateQRCode();
        } else if (this.selectedMethod === 'card') {
            document.getElementById('card-payment-container').style.display = 'block';
            document.getElementById('card-payment-spinner').style.display = 'none';
        }
    },
    
    // Generar código QR desde el backend
    generateQRCode: function() {
        const qrContainer = document.getElementById('qr-code');
        qrContainer.innerHTML = ''; // Limpiar contenedor
        
        // Mostrar spinner mientras se carga
        qrContainer.innerHTML = `
            <div class="qr-loading">
                <div class="spinner"></div>
                <p>Generando código QR...</p>
            </div>
        `;
        
        // Obtener datos necesarios para la solicitud
        const horarioId = document.getElementById('horario-id').value;
        const butacaIds = window.getSelectedButacaIds();
        
        // Crear la solicitud para generar el QR
        const qrRequest = {
            horarioId: parseInt(horarioId),
            butacaIds: butacaIds,
            asientosIds: this.selectedSeats
        };
        
        // Hacer la solicitud al backend
        fetch('/Tickets/GenerateQrData', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(qrRequest)
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al generar el código QR');
            }
            return response.json();
        })
        .then(data => {
            // Verificar si la respuesta fue exitosa
            if (data.success) {
                // Guardar la referencia de pago
                const paymentRef = data.qrData.paymentReference;
                document.getElementById('payment-reference').textContent = paymentRef;
                
                // Generar QR usando una librería externa o el string devuelto
                this.renderQrCode(data.qrString, qrContainer);
                
                // Guardar los datos para el pago
                this.paymentData = {
                    horarioId: parseInt(horarioId),
                    usuarioId: data.qrData.usuarioId,
                    butacaId: butacaIds,
                    metodoPago: this.selectedMethod,
                    referenciaPago: paymentRef
                };
                
                // Actualizar tiempo restante basado en la expiración
                if (data.expiresAt) {
                    const expiresAt = new Date(data.expiresAt);
                    const now = new Date();
                    const timeLeftInSeconds = Math.floor((expiresAt - now) / 1000);
                    if (timeLeftInSeconds > 0) {
                        this.timeLeft = timeLeftInSeconds;
                        this.updateTimerDisplay();
                    }
                }
            } else {
                throw new Error('No se pudo generar el código QR');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            qrContainer.innerHTML = `
                <div class="qr-error">
                    <i class="fas fa-exclamation-circle"></i>
                    <p>Error al generar el código QR. Por favor, intente nuevamente.</p>
                </div>
            `;
        });
    },
    
    // Renderizar código QR usando una API real con diseño mejorado
    renderQrCode: function(qrString, container) {
        // Obtener los datos para el código QR
        const totalPrice = document.getElementById('total-price').textContent;
        const paymentRef = document.getElementById('payment-reference').textContent || this.generatePaymentReference();
        const currentDate = new Date().toISOString();
        
        // Crear el objeto de datos que será codificado en el QR
        const qrData = {
            amount: parseFloat(totalPrice),
            reference: paymentRef,
            date: currentDate,
            seats: this.selectedSeats.join(',')
        };
        
        // Convertir los datos a una cadena JSON y codificarla para URL
        const qrDataString = encodeURIComponent(JSON.stringify(qrData));
        
        // Generar la URL de la API de QR
        const qrApiUrl = `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${qrDataString}&color=9933cc&bgcolor=ffffff`;
        
        // Mostrar el QR usando la API con diseño mejorado
        container.innerHTML = `
            <div class="qr-wrapper">
                <div class="qr-inner">
                    <div class="qr-corners">
                        <div class="qr-corner qr-corner-tl"></div>
                        <div class="qr-corner qr-corner-tr"></div>
                        <div class="qr-corner qr-corner-bl"></div>
                        <div class="qr-corner qr-corner-br"></div>
                    </div>
                    <img src="${qrApiUrl}" alt="Código QR de pago" class="qr-image">
                </div>
            </div>
            <div class="qr-instructions-detail">
                <p>Escanea este código para completar tu pago</p>
                <div class="qr-reference">${paymentRef}</div>
            </div>
        `;
        
        // Actualizar la referencia de pago en la UI si no estaba establecida
        if (!document.getElementById('payment-reference').textContent) {
            document.getElementById('payment-reference').textContent = paymentRef;
        }
        
        // Añadir efecto de brillo al QR cuando el mouse pase sobre él
        const qrWrapper = container.querySelector('.qr-wrapper');
        if (qrWrapper) {
            qrWrapper.addEventListener('mousemove', function(e) {
                const rect = this.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;
                
                // Calcular el porcentaje de posición para el efecto de brillo
                const percentX = x / rect.width;
                const percentY = y / rect.height;
                
                // Aplicar un efecto de brillo radial que sigue al puntero
                this.style.background = `radial-gradient(circle at ${percentX * 100}% ${percentY * 100}%, rgba(0, 204, 255, 0.8), var(--primary-color))`;
            });
            
            qrWrapper.addEventListener('mouseleave', function() {
                // Restaurar el gradiente original al salir
                this.style.background = 'linear-gradient(135deg, var(--primary-color), var(--secondary-color))';
            });
        }
    },
    
    // Generar referencia de pago aleatoria
    generatePaymentReference: function() {
        return 'REF-' + Math.random().toString(36).substring(2, 10).toUpperCase();
    },
    
    // Procesar el pago
    processPayment: function() {
        // Preparar los datos para enviar al servidor
        const horarioId = document.getElementById('horario-id') ? 
                         document.getElementById('horario-id').value : 
                         1; // Valor por defecto o recuperarlo de otro elemento
        
        const usuarioId = document.getElementById('user-id').value;
        const butacaIds = window.getSelectedButacaIds();
        
        this.paymentData = {
            horarioId: parseInt(horarioId),
            usuarioId: parseInt(usuarioId),
            butacaId: butacaIds,
            metodoPago: this.selectedMethod
        };
        
        console.log('Datos de pago:', this.paymentData);
        
        if (this.selectedMethod === 'card') {
            // Mostrar spinner de carga
            document.getElementById('card-payment-form').style.display = 'none';
            document.getElementById('card-payment-spinner').style.display = 'flex';
        }
        
        // Simular procesamiento de pago
        setTimeout(() => {
            this.sendPaymentToServer();
        }, 2000);
    },
    
    // Enviar información de pago al servidor
    sendPaymentToServer: function() {
        fetch('/Tickets/CreateTicket', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(this.paymentData)
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error en la respuesta del servidor');
            }
            return response.json();
        })
        .then(data => {
            console.log('Respuesta del servidor:', data);
            
            // Mostrar paso de confirmación
            this.currentStep = 3;
            this.updateStepIndicators();
            this.showCurrentStep();
            
            // Actualizar información de confirmación
            document.getElementById('confirmation-reference').textContent = this.generatePaymentReference();
            document.getElementById('confirmation-seats').textContent = this.selectedSeats.join(', ');
            
            // Detener el temporizador
            this.stopTimer();
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Ocurrió un error durante el proceso de pago: ' + error.message);
            
            // Volver al paso anterior en caso de error
            if (this.selectedMethod === 'card') {
                document.getElementById('card-payment-form').style.display = 'block';
                document.getElementById('card-payment-spinner').style.display = 'none';
            }
        });
    }, 
    
    // Finalizar el proceso de pago
    finishPayment: function() {
        this.closeModal();
        // Obtener el ID del horario desde el campo oculto
        const horarioId = document.getElementById('horario-id').value;
        // Redirigir a la página de tickets específicos para este horario
        window.location.href = '/Home/MisTickets/' + horarioId;
    },
    
    // Iniciar temporizador
    startTimer: function() {
        this.timeLeft = 600; // 10 minutos
        this.updateTimerDisplay();
        
        this.timerInterval = setInterval(() => {
            this.timeLeft--;
            this.updateTimerDisplay();
            
            if (this.timeLeft <= 0) {
                this.stopTimer();
                alert('El tiempo para completar el pago ha expirado.');
                this.closeModal();
            }
        }, 1000);
    },
    
    // Detener temporizador
    stopTimer: function() {
        if (this.timerInterval) {
            clearInterval(this.timerInterval);
            this.timerInterval = null;
        }
    },
    
    // Actualizar display del temporizador
    updateTimerDisplay: function() {
        const minutes = Math.floor(this.timeLeft / 60);
        const seconds = this.timeLeft % 60;
        const display = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        document.getElementById('payment-timer').textContent = display;
    },
    
    // Validaciones de tarjeta
    formatCardNumber: function(e) {
        let input = e.target;
        let value = input.value.replace(/\D/g, '');
        let formattedValue = '';
        
        for (let i = 0; i < value.length; i++) {
            if (i > 0 && i % 4 === 0) {
                formattedValue += ' ';
            }
            formattedValue += value[i];
        }
        
        input.value = formattedValue;
        
        // Actualizar visualización de la tarjeta
        document.querySelector('.card-number').textContent = formattedValue || '•••• •••• •••• ••••';
    },
    
    formatCardExpiry: function(e) {
        let input = e.target;
        let value = input.value.replace(/\D/g, '');
        
        if (value.length > 2) {
            input.value = value.substring(0, 2) + '/' + value.substring(2, 4);
        } else {
            input.value = value;
        }
        
        // Actualizar visualización de la tarjeta
        document.querySelector('.card-expiry').textContent = input.value || 'MM/AA';
    },
    
    validateCardCVV: function(e) {
        let input = e.target;
        input.value = input.value.replace(/\D/g, '').substring(0, 4);
    },
    
    validateCardHolder: function(e) {
        let input = e.target;
        // Actualizar visualización de la tarjeta
        document.querySelector('.card-holder').textContent = input.value.toUpperCase() || 'NOMBRE DEL TITULAR';
    }
};

// Exponer al ámbito global
window.paymentSystem = paymentSystem;

// Inicializar cuando el DOM esté cargado
document.addEventListener('DOMContentLoaded', function() {
    console.log('Inicializando sistema de pago...');
    paymentSystem.init();
    
    // Añadir animación de partículas al QR cuando esté visible
    setTimeout(() => {
        const qrContainer = document.querySelector('.qr-container');
        if (qrContainer) {
            // Crear una capa de partículas detrás del QR
            const particlesLayer = document.createElement('div');
            particlesLayer.classList.add('qr-particles-layer');
            particlesLayer.style.cssText = `
                position: absolute;
                width: 100%;
                height: 100%;
                top: 0;
                left: 0;
                z-index: -1;
                overflow: hidden;
                border-radius: 20px;
            `;
            
            // Agregar partículas al azar
            for (let i = 0; i < 20; i++) {
                const particle = document.createElement('div');
                const size = Math.random() * 4 + 2;
                particle.style.cssText = `
                    position: absolute;
                    width: ${size}px;
                    height: ${size}px;
                    background: rgba(${Math.random() * 255}, ${Math.random() * 255}, 255, 0.6);
                    border-radius: 50%;
                    top: ${Math.random() * 100}%;
                    left: ${Math.random() * 100}%;
                    animation: floatParticle ${Math.random() * 10 + 5}s linear infinite;
                    box-shadow: 0 0 ${Math.random() * 10 + 5}px rgba(0, 204, 255, 0.8);
                `;
                particlesLayer.appendChild(particle);
            }
            
            // Agregar las partículas al contenedor del QR
            const qrWrapper = qrContainer.querySelector('.qr-wrapper');
            if (qrWrapper) {
                qrWrapper.appendChild(particlesLayer);
            }
        }
    }, 1000);
});

// Definir la animación de flotación para las partículas
const style = document.createElement('style');
style.textContent = `
@keyframes floatParticle {
    0% { transform: translateY(0) rotate(0deg); opacity: 0; }
    10% { opacity: 1; }
    90% { opacity: 1; }
    100% { transform: translateY(-100px) rotate(360deg); opacity: 0; }
}

@keyframes cursorPulse {
    0% { transform: translate(-50%, -50%) scale(1); opacity: 0.7; }
    50% { transform: translate(-50%, -50%) scale(1.2); opacity: 1; }
    100% { transform: translate(-50%, -50%) scale(1); opacity: 0.7; }
}
`;
document.head.appendChild(style);
