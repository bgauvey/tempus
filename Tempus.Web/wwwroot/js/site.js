// Tempus Site JavaScript - Scroll to Top for Radzen Layout

console.log('Tempus site.js loaded successfully');

// Scroll to top function targeting Radzen's .rz-body container
window.scrollToTop = function() {
    console.log('scrollToTop function called from Blazor');

    // Primary target: Radzen body container (this is the main scroll container)
    const radzenBody = document.querySelector('.rz-body');
    if (radzenBody) {
        console.log('Found .rz-body, current scrollTop:', radzenBody.scrollTop);
        radzenBody.scrollTop = 0;
        console.log('Set .rz-body scrollTop to 0, now:', radzenBody.scrollTop);
    } else {
        console.warn('.rz-body element not found!');
    }

    // Also scroll document as fallback
    document.documentElement.scrollTop = 0;
    document.body.scrollTop = 0;
    window.scrollTo(0, 0);

    console.log('scrollToTop function completed');
};

console.log('window.scrollToTop function defined:', typeof window.scrollToTop);

// Download file function for PDF generation
window.downloadFile = function(filename, base64Data) {
    console.log('downloadFile function called for:', filename);

    // Convert base64 to blob
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: 'application/pdf' });

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

    console.log('File download triggered successfully');
};

console.log('window.downloadFile function defined:', typeof window.downloadFile);
