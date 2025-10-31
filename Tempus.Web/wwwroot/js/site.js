// Tempus Site JavaScript - Scroll to Top for Radzen Layout

// Scroll to top function targeting Radzen's .rz-body container
window.scrollToTop = function() {

    // Primary target: Radzen body container (this is the main scroll container)
    const radzenBody = document.querySelector('.rz-body');
    if (radzenBody) {
        radzenBody.scrollTop = 0;
    } else {
        console.warn('.rz-body element not found!');
    }

    // Also scroll document as fallback
    document.documentElement.scrollTop = 0;
    document.body.scrollTop = 0;
    window.scrollTo(0, 0);
};

// Download file function for PDF, CSV, Excel, and other file generation
window.downloadFile = function(filename, base64Data, contentType) {

    // Use provided content type or default to PDF
    contentType = contentType || 'application/pdf';

    // Convert base64 to blob
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: contentType });

    // Create download link
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();

    // Cleanup
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};