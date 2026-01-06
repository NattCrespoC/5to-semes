/**
 * Digital Clock Component for Cine Center
 * Features:
 * - Real-time display of hours, minutes, seconds
 * - Date display in format: Day, DD Month YYYY
 * - Special visual effect at full hours
 * - Smooth animations on time changes
 */

class DigitalClock {
    constructor() {
        this.timeElement = document.getElementById('digital-clock-time');
        this.dateElement = document.getElementById('digital-clock-date');
        this.previousTime = '';
        this.init();
    }

    init() {
        // Cargar fuente Orbitron para el reloj
        const fontLink = document.createElement('link');
        fontLink.href = 'https://fonts.googleapis.com/css2?family=Orbitron:wght@400;700&display=swap';
        fontLink.rel = 'stylesheet';
        document.head.appendChild(fontLink);
        
        // Iniciar el reloj
        this.updateClock();
        setInterval(() => this.updateClock(), 1000);
    }

    updateClock() {
        const now = new Date();
        this.updateTime(now);
        this.updateDate(now);
    }

    updateTime(now) {
        const hours = String(now.getHours()).padStart(2, '0');
        const minutes = String(now.getMinutes()).padStart(2, '0');
        const seconds = String(now.getSeconds()).padStart(2, '0');
        
        const currentTime = `${hours}:${minutes}:${seconds}`;
        
        // Verificar si el tiempo ha cambiado para animar
        if (this.previousTime !== currentTime) {
            this.timeElement.classList.remove('time-changed');
            void this.timeElement.offsetWidth; // Truco para reiniciar la animación
            this.timeElement.classList.add('time-changed');
            
            // Efecto especial en horas exactas
            if (minutes === '00' && seconds === '00') {
                this.timeElement.classList.add('time-special');
                setTimeout(() => {
                    this.timeElement.classList.remove('time-special');
                }, 10000); // Mantiene el efecto por 10 segundos
            }
            
            this.previousTime = currentTime;
        }
        
        this.timeElement.textContent = currentTime;
    }

    updateDate(now) {
        const options = { 
            weekday: 'long', 
            year: 'numeric', 
            month: 'long', 
            day: 'numeric' 
        };
        const dateString = now.toLocaleDateString('es-ES', options);
        
        // Capitalizar primera letra
        const formattedDate = dateString.charAt(0).toUpperCase() + dateString.slice(1);
        this.dateElement.textContent = formattedDate;
    }
}

// Iniciar el reloj cuando se carga la página
document.addEventListener('DOMContentLoaded', () => {
    new DigitalClock();
});
