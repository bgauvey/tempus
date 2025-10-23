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
