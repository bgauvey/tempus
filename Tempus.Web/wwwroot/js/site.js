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

// Browser Notification Support

// Check if browser notifications are supported
window.notificationsSupported = function() {
    return 'Notification' in window;
};

// Request notification permission
window.requestNotificationPermission = async function() {
    if (!window.notificationsSupported()) {
        console.warn('Browser notifications are not supported');
        return 'denied';
    }

    if (Notification.permission === 'granted') {
        return 'granted';
    }

    if (Notification.permission !== 'denied') {
        try {
            const permission = await Notification.requestPermission();
            return permission;
        } catch (error) {
            console.error('Error requesting notification permission:', error);
            return 'denied';
        }
    }

    return Notification.permission;
};

// Show a browser notification
window.showNotification = function(title, options) {
    if (!window.notificationsSupported()) {
        console.warn('Browser notifications are not supported');
        return null;
    }

    if (Notification.permission !== 'granted') {
        console.warn('Notification permission not granted');
        return null;
    }

    try {
        const notification = new Notification(title, {
            body: options?.body || '',
            icon: options?.icon || '/favicon-192.png',
            badge: options?.badge || '/favicon-192.png',
            tag: options?.tag || 'tempus-notification',
            requireInteraction: options?.requireInteraction || false,
            silent: options?.silent || false,
            data: options?.data || null
        });

        // Handle notification click
        if (options?.clickUrl) {
            notification.onclick = function() {
                window.focus();
                window.location.href = options.clickUrl;
                notification.close();
            };
        }

        return notification;
    } catch (error) {
        console.error('Error showing notification:', error);
        return null;
    }
};

// Get current notification permission status
window.getNotificationPermission = function() {
    if (!window.notificationsSupported()) {
        return 'unsupported';
    }
    return Notification.permission;
};