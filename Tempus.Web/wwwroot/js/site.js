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
    console.log('[Tempus.Notifications] Checking if notifications are supported...');
    const supported = 'Notification' in window;
    console.log('[Tempus.Notifications] Notifications supported:', supported);
    return supported;
};

// Request notification permission
window.requestNotificationPermission = async function() {
    console.log('[Tempus.Notifications] requestNotificationPermission called');

    if (!window.notificationsSupported()) {
        console.warn('[Tempus.Notifications] Browser notifications are not supported');
        return 'denied';
    }

    console.log('[Tempus.Notifications] Current permission:', Notification.permission);

    if (Notification.permission === 'granted') {
        console.log('[Tempus.Notifications] Permission already granted');
        return 'granted';
    }

    if (Notification.permission !== 'denied') {
        try {
            console.log('[Tempus.Notifications] Requesting permission from user...');
            const permission = await Notification.requestPermission();
            console.log('[Tempus.Notifications] User response:', permission);
            return permission;
        } catch (error) {
            console.error('[Tempus.Notifications] Error requesting notification permission:', error);
            return 'denied';
        }
    }

    console.log('[Tempus.Notifications] Permission denied');
    return Notification.permission;
};

// Show a browser notification
window.showNotification = function(title, options) {
    console.log('[Tempus.Notifications] showNotification called');
    console.log('[Tempus.Notifications]   Title:', title);
    console.log('[Tempus.Notifications]   Options:', options);

    if (!window.notificationsSupported()) {
        console.warn('[Tempus.Notifications] Browser notifications are not supported');
        return null;
    }

    console.log('[Tempus.Notifications]   Current permission:', Notification.permission);

    if (Notification.permission !== 'granted') {
        console.warn('[Tempus.Notifications] ❌ Notification permission not granted!');
        console.warn('[Tempus.Notifications]   Permission status:', Notification.permission);
        return null;
    }

    try {
        console.log('[Tempus.Notifications] ✅ Creating notification...');
        const notification = new Notification(title, {
            body: options?.body || '',
            icon: options?.icon || '/favicon-192.png',
            badge: options?.badge || '/favicon-192.png',
            tag: options?.tag || 'tempus-notification',
            requireInteraction: options?.requireInteraction || false,
            silent: options?.silent || false,
            data: options?.data || null
        });

        console.log('[Tempus.Notifications] ✅ Notification created successfully!');

        // Handle notification click
        if (options?.clickUrl) {
            notification.onclick = function() {
                console.log('[Tempus.Notifications] Notification clicked, navigating to:', options.clickUrl);
                window.focus();
                window.location.href = options.clickUrl;
                notification.close();
            };
        }

        return notification;
    } catch (error) {
        console.error('[Tempus.Notifications] ❌ Error showing notification:', error);
        return null;
    }
};

// Get current notification permission status
window.getNotificationPermission = function() {
    if (!window.notificationsSupported()) {
        console.log('[Tempus.Notifications] getNotificationPermission: unsupported');
        return 'unsupported';
    }
    const permission = Notification.permission;
    console.log('[Tempus.Notifications] getNotificationPermission:', permission);
    return permission;
};