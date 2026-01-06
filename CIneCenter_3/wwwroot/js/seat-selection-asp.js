// Variable global para manejar los asientos seleccionados
let selectedSeats = [];
let selectedButacaIds = []; // Array para guardar los IDs de butacas seleccionadas
const maxSeats = 4; // Máximo número de asientos por compra

document.addEventListener('DOMContentLoaded', function() {
    // Añadir eventos click a todos los asientos
    initializeSeatEvents();
    
    // Manejo del botón de compra
    document.getElementById('buy-button').addEventListener('click', function() {
        if (selectedSeats.length === 0) {
            alert('Por favor seleccione al menos un asiento.');
            return;
        }
        
        proceedToPayment();
    });
    
    // Inicializar el resumen de compra
    updatePurchaseSummary();
});

// Función para inicializar los eventos de los asientos
function initializeSeatEvents() {
    document.querySelectorAll('.seat:not([disabled])').forEach(seat => {
        seat.addEventListener('click', function() {
            toggleSeatSelection(this);
        });
    });
}

// Función para alternar selección de asientos
function toggleSeatSelection(seatElement) {
    const seatId = seatElement.getAttribute('data-seat');
    const butacaId = parseInt(seatElement.getAttribute('data-butaca-id')) || 0;
    const isSpecial = seatElement.hasAttribute('data-special');
    
    // Alternar selección
    if (seatElement.classList.contains('selected')) {
        // Deseleccionar asiento
        seatElement.classList.remove('selected');
        
        // Actualizar lista de asientos seleccionados
        const index = selectedSeats.indexOf(seatId);
        if (index > -1) {
            selectedSeats.splice(index, 1);
            selectedButacaIds.splice(index, 1); // También remover el ID de butaca
        }
    } else {
        // Verificar si ya se alcanzó el máximo de asientos
        if (selectedSeats.length >= maxSeats) {
            alert(`No puede seleccionar más de ${maxSeats} asientos por compra.`);
            return;
        }
        
        // Seleccionar asiento
        seatElement.classList.add('selected');
        selectedSeats.push(seatId);
        selectedButacaIds.push(butacaId); // Guardar el ID de butaca
        
        // Informar al usuario si seleccionó un asiento para personas con discapacidad
        if (isSpecial) {
            setTimeout(() => {
                alert("Ha seleccionado un asiento accesible para persona con discapacidad. Este asiento está destinado a personas con movilidad reducida o sus acompañantes.");
            }, 200);
        }
    }
    
    // Actualizar información de compra
    updatePurchaseSummary();
    
    // Habilitar/deshabilitar botón de compra
    document.getElementById('buy-button').disabled = selectedSeats.length === 0;
}

// Función para actualizar el resumen de compra
function updatePurchaseSummary() {
    const selectedSeatsCounter = document.getElementById('selected-seats');
    const ticketCount = document.getElementById('ticket-count');
    const totalPrice = document.getElementById('total-price');
    const seatPrice = parseFloat(document.getElementById('seat-price').textContent);
    
    // Preparar lista de asientos para mostrar
    let seatsToDisplay = [...selectedSeats];
    
    // Marcar asientos especiales con un símbolo para diferenciarlos
    seatsToDisplay = seatsToDisplay.map(seatId => {
        const seatElement = document.querySelector(`.seat[data-seat="${seatId}"]`);
        if (seatElement && seatElement.hasAttribute('data-special')) {
            return `${seatId} ♿`;
        }
        return seatId;
    });
    
    // Actualizar información de compra
    selectedSeatsCounter.textContent = selectedSeats.length > 0 ? selectedSeats.join(', ') : 'ninguno';
    ticketCount.textContent = selectedSeats.length;
    totalPrice.textContent = (selectedSeats.length * seatPrice).toFixed(2);
}

// Función para proceder al pago
function proceedToPayment() {
    // Verificar si estamos usando el modal de pago
    if (window.paymentSystem) {
        // Usar el sistema de modal de pago
        window.paymentSystem.openPaymentModal(selectedSeats);
        return;
    }
    
    // Si no está disponible el sistema de modal, usar el método antiguo
    // Verificar si hay asientos seleccionados sin ID de butaca
    const asientosInvalidos = selectedButacaIds.filter(id => !id).length;
    if (asientosInvalidos > 0) {
        alert('Algunos asientos seleccionados no tienen un código válido. Por favor, contacte al administrador.');
        return;
    }
    
    // Crear los datos para enviar según el modelo CreateTickets
    const paymentData = {
        horarioId: document.getElementById('horario-id')?.value || 8, // Obtener el ID del horario de un campo oculto o usar valor por defecto
        usuarioId: document.getElementById('user-id').value,
        butacaId: selectedButacaIds,
    };
    
    // Mostrar una ventana de confirmación
    if (confirm(`¿Desea confirmar la compra de los asientos: ${selectedSeats.join(', ')}?`)) {
        // Cambiar el botón a estado de carga
        const buyButton = document.getElementById('buy-button');
        buyButton.disabled = true;
        buyButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Procesando...';
        
        // Enviar la solicitud al controlador de tickets
        fetch('/Tickets/CreateTicket', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(paymentData)
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error en la respuesta del servidor');
            }
            return response.json();
        })
        .then(data => {
            alert('¡Compra realizada con éxito!');
            // Redirigir a la página de tickets del usuario
            window.location.href = data.redirectUrl || '/Home/MisTickets'; 
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Ocurrió un error durante el proceso de pago: ' + error.message);
            // Restaurar el botón
            buyButton.disabled = false;
            buyButton.innerHTML = '<i class="fas fa-ticket-alt"></i> Pagar entradas';
        });
    }
}

// Función para obtener los IDs de butacas seleccionadas
window.getSelectedButacaIds = function() {
    return selectedButacaIds;
};
