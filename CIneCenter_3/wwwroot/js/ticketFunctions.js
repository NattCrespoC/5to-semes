/**
 * Ticket functionality for CineCenter
 */

function printTicket(ticketId) {
    const element = document.getElementById(ticketId);
    
    // Crear una ventana de impresión y añadir estilos
    const printWindow = window.open('', '_blank');
    printWindow.document.write('<html><head><title>Ticket</title>');
    printWindow.document.write('<style>');
    printWindow.document.write(`
        body { font-family: Arial, sans-serif; margin: 20px; }
        .ticket { 
            max-width: 500px; 
            border: 2px dashed #333; 
            border-radius: 8px; 
            padding: 20px; 
            margin: 0 auto; 
        }
        .ticket-header { 
            display: flex; 
            justify-content: space-between; 
            border-bottom: 1px dashed #999; 
            padding-bottom: 10px; 
            margin-bottom: 15px; 
        }
        .ticket-title { 
            font-size: 1.5rem; 
            font-weight: bold; 
            color: #333; 
            margin: 0; 
        }
        .ticket-stub { 
            background: #f0f0f0; 
            padding: 5px 10px; 
            border-radius: 15px; 
            font-size: 0.8rem; 
            font-weight: bold; 
            color: #555; 
        }
        .ticket-content { 
            display: flex; 
            flex-direction: column; 
            gap: 15px; 
        }
        .ticket-divider { 
            border-top: 1px dashed #ccc; 
            margin: 10px 0; 
        }
        .ticket-info { 
            display: flex; 
            justify-content: space-between; 
            margin-bottom: 5px; 
        }
        .ticket-label { font-weight: bold; color: #666; }
        .ticket-value { color: #333; }
        .ticket-qr { 
            align-self: center; 
            margin: 15px 0; 
        }
        .ticket-footer { 
            text-align: center; 
            font-size: 0.8rem; 
            color: #777; 
            margin-top: 15px; 
            border-top: 1px dashed #ccc; 
            padding-top: 10px; 
        }
        .cinema-logo { 
            text-align: center; 
            margin-bottom: 10px; 
            font-size: 1.2rem; 
            font-weight: bold; 
            color: #333; 
        }
    `);
    printWindow.document.write('</style></head><body>');
    printWindow.document.write(element.outerHTML);
    printWindow.document.write('</body></html>');
    printWindow.document.close();
    
    // Log para depurar
    console.log("Imprimiendo ticket:", ticketId);
    
    // Esperar a que se carguen las imágenes antes de imprimir
    setTimeout(function() {
        printWindow.print();
        printWindow.close();
    }, 500); // Aumentado a 500ms para dar más tiempo a cargar
}

function downloadTicketAsImage(ticketId) {
    const element = document.getElementById(ticketId);
    
    // Log para depurar
    console.log("Descargando ticket como imagen:", ticketId);
    
    // 1. Primero, obtener todas las imágenes y comprobar si están cargadas
    const images = element.querySelectorAll('img');
    let imagesLoaded = 0;
    const totalImages = images.length;
    
    // Si no hay imágenes, proceder directamente a la captura
    if (totalImages === 0) {
        captureAndDownload();
        return;
    }
    
    // Verificar si todas las imágenes ya están cargadas
    let allLoaded = true;
    images.forEach(img => {
        if (!img.complete) {
            allLoaded = false;
        }
    });
    
    if (allLoaded) {
        captureAndDownload();
        return;
    }
    
    // Esperar a que se carguen todas las imágenes
    images.forEach(img => {
        // Agregar un atributo crossorigin para permitir la captura de imágenes externas
        if (img.src.startsWith('http')) {
            img.crossOrigin = "anonymous";
        }
        
        if (img.complete) {
            imagesLoaded++;
            checkIfAllLoaded();
        } else {
            img.addEventListener('load', () => {
                imagesLoaded++;
                checkIfAllLoaded();
            });
            
            img.addEventListener('error', () => {
                console.warn('Error cargando imagen para captura', img.src);
                // Intentar cargar nuevamente la imagen con un proxy CORS
                if (img.src.includes('api.qrserver.com')) {
                    const originalSrc = img.src;
                    // Usar un proxy CORS o un método alternativo para cargar la imagen
                    img.src = `https://cors-anywhere.herokuapp.com/${originalSrc}`;
                }
                imagesLoaded++;
                checkIfAllLoaded();
            });
        }
    });
    
    function checkIfAllLoaded() {
        if (imagesLoaded === totalImages) {
            captureAndDownload();
        }
    }
    
    function captureAndDownload() {
        try {
            // Configurar opciones avanzadas para html2canvas
            const options = {
                allowTaint: true,
                useCORS: true,
                logging: true,
                imageTimeout: 0,
                scale: 2, // Mejor calidad para la imagen
                onclone: function(clonedDoc) {
                    // Asegurar que las imágenes externas se carguen correctamente en el clon
                    const clonedImages = clonedDoc.querySelectorAll('img');
                    clonedImages.forEach(img => {
                        if (img.src.startsWith('http')) {
                            img.crossOrigin = "anonymous";
                        }
                    });
                }
            };
            
            // Alternativa: Genera un nuevo QR local para asegurar que se capture
            const qrImage = element.querySelector('.ticket-qr img');
            if (qrImage && qrImage.src.includes('api.qrserver.com')) {
                console.log("Procesando QR para descarga...");
                
                // Obtener los datos del QR de la URL
                const qrUrl = new URL(qrImage.src);
                const qrData = qrUrl.searchParams.get('data');
                const qrSize = qrUrl.searchParams.get('size') || "200x200";
                
                // Guardar la URL original
                const originalQrSrc = qrImage.src;
                
                // Intentar usar un formato de datos en lugar de URL externa
                html2canvas(element, options).then(canvas => {
                    const link = document.createElement('a');
                    link.download = `ticket-${ticketId.split('-')[1]}.png`;
                    link.href = canvas.toDataURL("image/png");
                    link.click();
                }).catch(error => {
                    console.error("Error al crear la imagen:", error);
                    alert("Hubo un problema al descargar el ticket. Por favor intenta de nuevo.");
                });
            } else {
                // Proceder con la captura normal
                html2canvas(element, options).then(canvas => {
                    const link = document.createElement('a');
                    link.download = `ticket-${ticketId.split('-')[1]}.png`;
                    link.href = canvas.toDataURL("image/png");
                    link.click();
                }).catch(error => {
                    console.error("Error al crear la imagen:", error);
                    alert("Hubo un problema al descargar el ticket. Por favor intenta de nuevo.");
                });
            }
        } catch (error) {
            console.error("Error en downloadTicketAsImage:", error);
            alert("Hubo un error al descargar el ticket. Por favor, intenta de nuevo.");
        }
    }
}
