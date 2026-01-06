document.addEventListener('DOMContentLoaded', function() {
    // Inicializar la funcionalidad de selección de formato
    initFormatSelection();
    // Inicializar la funcionalidad de selección de día
    initDaySelection();
});

// Variables para almacenar el formato y día seleccionados
let selectedFormat = null;
let selectedDate = null;

function initFormatSelection() {
    // Seleccionar todos los botones de formato
    const formatButtons = document.querySelectorAll('.format-btn, .format-btn2');
    
    // Añadir evento de clic a cada botón
    formatButtons.forEach(button => {
        button.addEventListener('click', function() {
            // Obtener el formato seleccionado
            const format = this.dataset.format;
            
            // Remover clase 'selected' o 'active' de todos los botones
            formatButtons.forEach(btn => {
                btn.classList.remove('selected', 'active');
            });
            
            // Añadir clase 'selected' al botón clickeado
            this.classList.add('selected');
            
            // Actualizar variable global
            selectedFormat = format;
            
            // Ocultar todas las secciones de formato
            document.querySelectorAll('.format-section').forEach(section => {
                section.style.display = 'none';
            });
            
            // Mostrar la sección correspondiente al formato seleccionado
            const selectedFormatSection = document.getElementById(`schedule-${format}`);
            if (selectedFormatSection) {
                selectedFormatSection.style.display = 'block';
                
                // Si ya hay un día seleccionado, mostrar su contenido
                if (selectedDate) {
                    updateVisibleSchedules();
                } else {
                    // Si no hay día seleccionado, seleccionar el primero por defecto
                    const firstDayBtn = document.querySelector('.day-btn, .day-btn2');
                    if (firstDayBtn) {
                        firstDayBtn.click();
                    }
                }
            }
        });
    });
    
    // Seleccionar automáticamente el primer formato disponible
    const firstFormatBtn = formatButtons[0];
    if (firstFormatBtn) {
        firstFormatBtn.click();
    }
}

function initDaySelection() {
    // Seleccionar todos los botones de día
    const dayButtons = document.querySelectorAll('.day-btn, .day-btn2');
    
    // Añadir evento de clic a cada botón
    dayButtons.forEach(button => {
        button.addEventListener('click', function() {
            // Obtener la fecha seleccionada
            const date = this.dataset.date;
            
            // Remover clase 'selected' o 'active' de todos los botones de día
            dayButtons.forEach(btn => {
                btn.classList.remove('selected', 'active');
            });
            
            // Añadir clase 'selected' al botón clickeado
            this.classList.add('selected');
            
            // Actualizar variable global
            selectedDate = date;
            
            // Actualizar las secciones visibles
            updateVisibleSchedules();
        });
    });
}

function updateVisibleSchedules() {
    // Verificar que ambos formato y fecha estén seleccionados
    if (!selectedFormat || !selectedDate) return;
    
    // Ocultar todas las secciones de fecha dentro del formato seleccionado
    const formatSection = document.getElementById(`schedule-${selectedFormat}`);
    if (formatSection) {
        const dateSections = formatSection.querySelectorAll('.date-section, .date-section2');
        dateSections.forEach(section => {
            section.style.display = 'none';
        });
        
        // Mostrar solo la sección de la fecha seleccionada
        const selectedDateSection = formatSection.querySelector(`[data-date="${selectedDate}"]`);
        if (selectedDateSection) {
            selectedDateSection.style.display = 'block';
        }
    }
}

// Función para añadir estilos personalizados en runtime
function addCustomStyles() {
    const style = document.createElement('style');
    style.textContent = `
        /* Estilos para los filtros de día */
        .booking-filters, .booking-filters2 {
            margin-bottom: 20px;
        }
        
        .day-options, .day-options2 {
            display: flex;
            gap: 10px;
            margin-top: 15px;
        }
        
        .day-btn, .day-btn2 {
            padding: 8px 16px;
            background-color: rgba(0, 0, 0, 0.3);
            color: var(--text-color);
            border: 1px solid var(--gradient-start);
            border-radius: 4px;
            cursor: pointer;
            transition: all 0.3s ease;
        }
        
        .day-btn.selected, .day-btn.active,
        .day-btn2.selected, .day-btn2.active {
            background: linear-gradient(to right, var(--gradient-start), var(--gradient-end));
            color: #000;
            font-weight: bold;
        }
        
        .day-btn:hover, .day-btn2:hover {
            transform: translateY(-2px);
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.3);
        }
        
        /* Estilos para las secciones de tiempo */
        .time-section, .time-section2 {
            margin-bottom: 15px;
        }
        
        .time-heading, .time-heading2 {
            color: #ccc;
            font-size: 0.9rem;
            margin-bottom: 8px;
            font-weight: 500;
        }
        
        .date-heading, .date-heading2 {
            color: var(--accent-color);
            font-size: 1.1rem;
            margin-bottom: 15px;
            padding-bottom: 5px;
            border-bottom: 1px solid rgba(255, 255, 255, 0.1);
        }
    `;
    document.head.appendChild(style);
}

// Añadir estilos personalizados cuando se cargue el documento
document.addEventListener('DOMContentLoaded', addCustomStyles);
