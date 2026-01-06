/**
 * Confirmation Page JavaScript
 * Handles ticket display and interactive features
 */

document.addEventListener('DOMContentLoaded', function() {
    // Get URL parameters
    const urlParams = new URLSearchParams(window.location.search);
    const seats = urlParams.get('seats') ? urlParams.get('seats').split(',') : [];
    const reference = urlParams.get('reference') || 'CINE-' + Math.floor(Math.random() * 1000000);
    
    // Sample movie data - in a real app, this would come from a database or API
    const movieData = {
        title: urlParams.get('title') || 'THUNDERBOLTS',
        format: urlParams.get('format') || '2D',
        date: urlParams.get('date') || formatDate(new Date()),
        time: urlParams.get('time') || '20:00',
        room: urlParams.get('room') || 'Sala 5',
        poster: urlParams.get('poster') || 'https://lumiere-a.akamaihd.net/v1/images/p_marvel_thunderbolts_2967_b0ec05c6.jpeg'
    };
    
    // Set the reference number
    document.getElementById('reference-number').textContent = reference;
    
    // Display movie information
    displayMovieInfo(movieData, seats);
    
    // Generate digital tickets
    generateTickets(seats, movieData, reference);
    
    // Initialize ticket slider functionality
    initializeTicketSlider();
    
    // Initialize action buttons
    initializeActions(movieData, reference, seats);
    
    // Setup star rating
    setupRating();
    
    // Initialize modals
    initializeModals();
    
    // Add entry animation to elements
    animateEntryElements();
});

/**
 * Formats a date object to a readable string
 */
function formatDate(date) {
    const days = ['domingo', 'lunes', 'martes', 'miércoles', 'jueves', 'viernes', 'sábado'];
    const months = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];
    
    return `${days[date.getDay()]} ${date.getDate()} ${months[date.getMonth()]}`;
}

/**
 * Displays the movie information in the summary section
 */
function displayMovieInfo(movieData, seats) {
    document.getElementById('movie-name').textContent = movieData.title;
    document.getElementById('movie-format').textContent = movieData.format;
    document.getElementById('movie-date').textContent = movieData.date;
    document.getElementById('movie-time').textContent = movieData.time;
    document.getElementById('movie-room').textContent = movieData.room;
    document.getElementById('selected-seats').textContent = seats.join(', ');
    
    // Set movie poster
    document.getElementById('movie-poster').style.backgroundImage = `url(${movieData.poster})`;
}

/**
 * Generates digital tickets for each seat
 */
function generateTickets(seats, movieData, reference) {
    const ticketsSlider = document.getElementById('tickets-slider');
    ticketsSlider.innerHTML = '';
    
    seats.forEach((seat, index) => {
        // Create a unique ticket reference for each seat
        const ticketRef = `${reference}-${index + 1}`;
        
        const ticket = document.createElement('div');
        ticket.className = 'ticket';
        
        ticket.innerHTML = `
            <div class="ticket-inner">
                <div class="ticket-left">
                    <div class="ticket-header">
                        <h3 class="ticket-title">${movieData.title}</h3>
                        <div class="ticket-meta">${movieData.format} | ${movieData.date} | ${movieData.time}</div>
                    </div>
                    <div class="ticket-details">
                        <div>Sala: ${movieData.room}</div>
                        <div class="ticket-seat">ASIENTO ${seat}</div>
                    </div>
                </div>
                <div class="ticket-right">
                    <div class="ticket-qr-container" data-seat="${seat}">
                        <img src="https://api.qrserver.com/v1/create-qr-code/?size=100x100&data=${encodeURIComponent(ticketRef)}" alt="QR Code">
                    </div>
                    <div class="ticket-number">${ticketRef}</div>
                </div>
            </div>
        `;
        
        ticketsSlider.appendChild(ticket);
    });

    // Add click event for QR codes
    document.querySelectorAll('.ticket-qr-container').forEach(qrContainer => {
        qrContainer.addEventListener('click', function() {
            const seat = this.getAttribute('data-seat');
            const qrImage = this.querySelector('img').src;
            showQRModal(qrImage, seat);
        });
    });
}

/**
 * Initializes the ticket slider functionality
 */
function initializeTicketSlider() {
    const slider = document.getElementById('tickets-slider');
    const tickets = slider.querySelectorAll('.ticket');
    const prevBtn = document.getElementById('prev-ticket');
    const nextBtn = document.getElementById('next-ticket');
    
    let currentIndex = 0;
    
    // Hide navigation buttons if there's only one ticket
    if (tickets.length <= 1) {
        document.querySelector('.tickets-navigation').style.display = 'none';
        return;
    }
    
    function updateSlider() {
        const ticketWidth = tickets[0].offsetWidth + parseInt(window.getComputedStyle(tickets[0]).marginRight);
        slider.scrollLeft = currentIndex * ticketWidth;
    }
    
    prevBtn.addEventListener('click', function() {
        if (currentIndex > 0) {
            currentIndex--;
            updateSlider();
        }
    });
    
    nextBtn.addEventListener('click', function() {
        if (currentIndex < tickets.length - 1) {
            currentIndex++;
            updateSlider();
        }
    });
}

/**
 * Initializes the action buttons (download, print, calendar)
 */
function initializeActions(movieData, reference, seats) {
    const downloadBtn = document.getElementById('download-tickets-btn');
    const printBtn = document.getElementById('print-tickets-btn');
    const calendarBtn = document.getElementById('add-to-calendar-btn');
    
    // Download tickets as PDF
    downloadBtn.addEventListener('click', function() {
        const ticketsContainer = document.getElementById('tickets-slider');
        const tickets = ticketsContainer.querySelectorAll('.ticket');
        
        // Prepare for PDF generation
        const pdfContainer = document.createElement('div');
        pdfContainer.className = 'pdf-container';
        pdfContainer.style.width = '800px';
        pdfContainer.style.background = '#fff';
        pdfContainer.style.padding = '20px';
        pdfContainer.style.position = 'absolute';
        pdfContainer.style.left = '-9999px';
        
        // Add movie info to PDF
        const movieInfo = document.createElement('div');
        movieInfo.innerHTML = `
            <h1 style="color: #6a11cb; margin-bottom: 5px;">${movieData.title}</h1>
            <p style="margin-top: 0; color: #555;">
                ${movieData.format} | ${movieData.date} | ${movieData.time} | Sala: ${movieData.room}
            </p>
            <p style="margin-top: 20px; color: #333;">Referencia de compra: <strong>${reference}</strong></p>
            <h2 style="margin-top: 30px; color: #444;">Tus Entradas:</h2>
        `;
        pdfContainer.appendChild(movieInfo);
        
        // Add all tickets
        tickets.forEach((ticket) => {
            const ticketClone = ticket.cloneNode(true);
            ticketClone.style.margin = '20px 0';
            ticketClone.style.pageBreakInside = 'avoid';
            pdfContainer.appendChild(ticketClone);
        });
        
        document.body.appendChild(pdfContainer);
        
        // Generate PDF
        const { jsPDF } = window.jspdf;
        
        html2canvas(pdfContainer, {
            scale: 2,
            logging: false,
            useCORS: true
        }).then(canvas => {
            const imgData = canvas.toDataURL('image/png');
            const pdf = new jsPDF('p', 'mm', 'a4');
            const pdfWidth = pdf.internal.pageSize.getWidth();
            const pdfHeight = pdf.internal.pageSize.getHeight();
            const imgWidth = canvas.width;
            const imgHeight = canvas.height;
            const ratio = Math.min(pdfWidth / imgWidth, pdfHeight / imgHeight);
            const imgX = (pdfWidth - imgWidth * ratio) / 2;
            const imgY = 30;
            
            pdf.addImage(imgData, 'PNG', imgX, imgY, imgWidth * ratio, imgHeight * ratio);
            pdf.save(`Entradas_${movieData.title.replace(/\s+/g, '_')}.pdf`);
            
            // Clean up
            document.body.removeChild(pdfContainer);
        });
    });
    
    // Print tickets
    printBtn.addEventListener('click', function() {
        const ticketsSlider = document.getElementById('tickets-slider');
        
        // Create a container specifically for printing
        const printContainer = document.createElement('div');
        printContainer.className = 'print-container';
        
        // Add movie info to print
        const movieInfo = document.createElement('div');
        movieInfo.innerHTML = `
            <h1 style="color: #6a11cb; margin-bottom: 5px;">${movieData.title}</h1>
            <p style="margin-top: 0; color: #555;">
                ${movieData.format} | ${movieData.date} | ${movieData.time} | Sala: ${movieData.room}
            </p>
            <p style="margin-top: 20px; color: #333;">Referencia de compra: <strong>${reference}</strong></p>
            <h2 style="margin-top: 30px; color: #444;">Tus Entradas:</h2>
        `;
        printContainer.appendChild(movieInfo);
        
        // Clone each ticket
        const tickets = ticketsSlider.querySelectorAll('.ticket');
        tickets.forEach(ticket => {
            const ticketClone = ticket.cloneNode(true);
            ticketClone.style.marginBottom = '20px';
            printContainer.appendChild(ticketClone);
        });
        
        // Append to body temporarily
        document.body.appendChild(printContainer);
        
        // Print
        window.print();
        
        // Remove the temporary container
        document.body.removeChild(printContainer);
    });
    
    // Calendar integration
    calendarBtn.addEventListener('click', function() {
        showCalendarModal(movieData);
    });
}

/**
 * Sets up the rating functionality
 */
function setupRating() {
    const stars = document.querySelectorAll('.rating-stars i');
    const ratingMessage = document.getElementById('rating-message');
    const feedbackText = document.getElementById('feedback-text');
    const sendFeedbackBtn = document.getElementById('send-feedback-btn');
    
    const messages = [
        'Muy malo',
        'Malo',
        'Regular',
        'Bueno',
        '¡Excelente!'
    ];
    
    let currentRating = 0;
    
    stars.forEach(star => {
        star.addEventListener('mouseover', function() {
            const rating = parseInt(this.dataset.rating);
            
            // Update stars
            stars.forEach((s, index) => {
                if (index < rating) {
                    s.className = 'fas fa-star';
                } else {
                    s.className = 'far fa-star';
                }
            });
            
            // Update message
            ratingMessage.textContent = messages[rating - 1];
        });
        
        star.addEventListener('mouseleave', function() {
            // Restore current rating
            stars.forEach((s, index) => {
                if (index < currentRating) {
                    s.className = 'fas fa-star active';
                } else {
                    s.className = 'far fa-star';
                }
            });
            
            // Restore message
            ratingMessage.textContent = currentRating > 0 ? messages[currentRating - 1] : 'Califica tu experiencia';
        });
        
        star.addEventListener('click', function() {
            currentRating = parseInt(this.dataset.rating);
            
            // Update stars with active class
            stars.forEach((s, index) => {
                if (index < currentRating) {
                    s.className = 'fas fa-star active';
                } else {
                    s.className = 'far fa-star';
                }
            });
            
            // Update message
            ratingMessage.textContent = messages[currentRating - 1];
        });
    });
    
    sendFeedbackBtn.addEventListener('click', function() {
        if (currentRating > 0) {
            // In a real app, send feedback to server
            console.log('Rating:', currentRating);
            console.log('Feedback:', feedbackText.value);
            
            // Show thank you modal
            showThankYouModal();
            
            // Reset form
            currentRating = 0;
            stars.forEach(s => s.className = 'far fa-star');
            ratingMessage.textContent = 'Califica tu experiencia';
            feedbackText.value = '';
        } else {
            alert('Por favor selecciona una calificación antes de enviar.');
        }
    });
}

/**
 * Shows the QR code modal with enlarged QR
 */
function showQRModal(qrImage, seat) {
    const modal = document.getElementById('qr-modal');
    const enlargedQR = document.getElementById('enlarged-qr');
    
    enlargedQR.innerHTML = `
        <img src="${qrImage}" alt="QR Code para asiento ${seat}">
        <p>Asiento ${seat}</p>
    `;
    
    modal.classList.add('show');
}

/**
 * Shows the calendar options modal
 */
function showCalendarModal(movieData) {
    const modal = document.getElementById('calendar-modal');
    modal.classList.add('show');
    
    const calendarOptions = document.querySelectorAll('.calendar-option');
    
    calendarOptions.forEach(option => {
        option.addEventListener('click', function() {
            const calendarType = this.getAttribute('data-calendar');
            
            // Parse date and time to create event date
            const dateParts = movieData.date.split(' ');
            const day = parseInt(dateParts[1]);
            
            const monthMap = {
                'enero': 0, 'febrero': 1, 'marzo': 2, 'abril': 3,
                'mayo': 4, 'junio': 5, 'julio': 6, 'agosto': 7,
                'septiembre': 8, 'octubre': 9, 'noviembre': 10, 'diciembre': 11
            };
            
            const month = monthMap[dateParts[2]];
            const year = new Date().getFullYear();
            
            const timeParts = movieData.time.split(':');
            const hour = parseInt(timeParts[0]);
            const minute = parseInt(timeParts[1]);
            
            const startDate = new Date(year, month, day, hour, minute);
            const endDate = new Date(year, month, day, hour + 2, minute); // Assuming 2 hours for movie
            
            // Format dates for calendar URLs
            const formatDate = (date) => {
                return date.toISOString().replace(/-|:|\.\d+/g, '');
            };
            
            const start = formatDate(startDate);
            const end = formatDate(endDate);
            const title = encodeURIComponent(`Película: ${movieData.title}`);
            const description = encodeURIComponent(`Formato: ${movieData.format}, Sala: ${movieData.room}`);
            const location = encodeURIComponent('Cine Center');
            
            let calendarUrl;
            
            switch (calendarType) {
                case 'google':
                    calendarUrl = `https://calendar.google.com/calendar/render?action=TEMPLATE&text=${title}&dates=${start}/${end}&details=${description}&location=${location}`;
                    break;
                case 'outlook':
                    calendarUrl = `https://outlook.office.com/calendar/0/deeplink/compose?subject=${title}&startdt=${startDate.toISOString()}&enddt=${endDate.toISOString()}&body=${description}&location=${location}`;
                    break;
                case 'apple':
                    // For Apple Calendar, we typically use an .ics file, but here's a workaround
                    calendarUrl = `data:text/calendar;charset=utf-8,BEGIN:VCALENDAR%0AVERSION:2.0%0ABEGIN:VEVENT%0ADTSTART:${start}%0ADTEND:${end}%0ASUMMARY:${title}%0ADESCRIPTION:${description}%0ALOCATION:${location}%0AEND:VEVENT%0AEND:VCALENDAR`;
                    break;
                case 'yahoo':
                    calendarUrl = `https://calendar.yahoo.com/?v=60&title=${title}&st=${start}&et=${end}&desc=${description}&in_loc=${location}`;
                    break;
            }
            
            // Open calendar in new tab
            window.open(calendarUrl, '_blank');
            
            // Close modal
            modal.classList.remove('show');
        });
    });
}

/**
 * Shows the thank you modal after feedback submission
 */
function showThankYouModal() {
    const modal = document.getElementById('thank-you-modal');
    modal.classList.add('show');
    
    // Generate random discount code
    const discountCode = 'GRACIAS' + Math.floor(Math.random() * 1000);
    document.getElementById('discount-code').textContent = discountCode;
    
    // Copy code functionality
    document.getElementById('copy-code-btn').addEventListener('click', function() {
        const code = document.getElementById('discount-code').textContent;
        navigator.clipboard.writeText(code).then(() => {
            this.innerHTML = '<i class="fas fa-check"></i>';
            setTimeout(() => {
                this.innerHTML = '<i class="fas fa-copy"></i>';
            }, 2000);
        });
    });
}

/**
 * Initializes all modals
 */
function initializeModals() {
    const modals = document.querySelectorAll('.modal');
    const closeButtons = document.querySelectorAll('.close-modal');
    
    // Close modal when clicking X button
    closeButtons.forEach(button => {
        button.addEventListener('click', function() {
            const modal = this.closest('.modal');
            modal.classList.remove('show');
        });
    });
    
    // Close modal when clicking outside
    modals.forEach(modal => {
        modal.addEventListener('click', function(e) {
            if (e.target === this) {
                modal.classList.remove('show');
            }
        });
    });
}

/**
 * Animates elements when the page loads
 */
function animateEntryElements() {
    const elements = [
        '.confirmation-title',
        '.confirmation-details',
        '.movie-summary',
        '.section-title',
        '.tickets-container',
        '.actions-container',
        '.share-container',
        '.feedback-container',
        '.navigation-buttons'
    ];
    
    elements.forEach((selector, index) => {
        const element = document.querySelector(selector);
        if (element) {
            element.style.opacity = '0';
            element.style.transform = 'translateY(20px)';
            element.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
            
            setTimeout(() => {
                element.style.opacity = '1';
                element.style.transform = 'translateY(0)';
            }, 100 + (index * 100));
        }
    });
}
