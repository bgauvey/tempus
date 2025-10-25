// Tempus Calendar Drag and Drop Handler
window.TempusCalendar = window.TempusCalendar || {};

(function () {
    let draggedElement = null;
    let draggedEventId = null;
    let draggedEventDuration = 0;
    let originalTop = 0;
    let ghostElement = null;
    let dotNetHelper = null;

    // Initialize drag and drop for calendar events
    TempusCalendar.initializeDragDrop = function (dotNetReference) {
        dotNetHelper = dotNetReference;
        console.log('Drag and drop initialized');
    };

    // Make an event draggable
    TempusCalendar.makeEventDraggable = function (eventId, durationMinutes) {
        const element = document.querySelector(`[data-event-id="${eventId}"]`);
        if (!element) {
            console.warn(`Event element not found: ${eventId}`);
            return;
        }

        element.setAttribute('draggable', 'true');
        element.style.cursor = 'grab';

        // Remove existing listeners to prevent duplicates
        element.ondragstart = null;
        element.ondragend = null;

        element.ondragstart = function (e) {
            handleDragStart(e, eventId, durationMinutes, element);
        };

        element.ondragend = function (e) {
            handleDragEnd(e);
        };
    };

    // Make all events in a container draggable
    TempusCalendar.makeAllEventsDraggable = function () {
        const events = document.querySelectorAll('[data-event-id]');
        events.forEach(element => {
            const eventId = element.getAttribute('data-event-id');
            const duration = parseInt(element.getAttribute('data-duration') || '60');

            element.setAttribute('draggable', 'true');
            element.style.cursor = 'grab';

            element.ondragstart = function (e) {
                handleDragStart(e, eventId, duration, element);
            };

            element.ondragend = function (e) {
                handleDragEnd(e);
            };
        });
    };

    // Setup drop zones (hour cells)
    TempusCalendar.setupDropZones = function () {
        // Day view hour rows
        const hourRows = document.querySelectorAll('.hour-row, .day-hour-cell');
        hourRows.forEach(row => {
            row.ondragover = function (e) {
                e.preventDefault();
                e.currentTarget.style.background = 'rgba(102, 126, 234, 0.1)';
            };

            row.ondragleave = function (e) {
                e.currentTarget.style.background = '';
            };

            row.ondrop = function (e) {
                e.preventDefault();
                e.currentTarget.style.background = '';
                handleDrop(e);
            };
        });
    };

    function handleDragStart(e, eventId, durationMinutes, element) {
        draggedElement = element;
        draggedEventId = eventId;
        draggedEventDuration = durationMinutes;
        originalTop = parseInt(element.style.top) || 0;

        // Set opacity
        element.style.opacity = '0.5';
        element.style.cursor = 'grabbing';

        // Create ghost element
        createGhostElement(element, durationMinutes);

        // Set drag data
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/plain', eventId);
    }

    function handleDragEnd(e) {
        if (draggedElement) {
            draggedElement.style.opacity = '1';
            draggedElement.style.cursor = 'grab';
        }

        // Remove ghost element
        if (ghostElement && ghostElement.parentNode) {
            ghostElement.parentNode.removeChild(ghostElement);
            ghostElement = null;
        }

        // Clear highlights
        const allCells = document.querySelectorAll('.hour-row, .day-hour-cell');
        allCells.forEach(cell => {
            cell.style.background = '';
        });

        draggedElement = null;
        draggedEventId = null;
        draggedEventDuration = 0;
    }

    function handleDrop(e) {
        if (!draggedEventId) return;

        // Calculate the new time based on drop position
        const dropTarget = e.currentTarget;
        const rect = dropTarget.getBoundingClientRect();
        const relativeY = e.clientY - rect.top;

        // Get the drop target's parent container to determine day
        let dayColumn = dropTarget.closest('.day-column, .day-hour-column');
        let dayIndex = 0;

        if (dayColumn && dayColumn.classList.contains('day-hour-column')) {
            // Multi-day view - get the day index
            const allDayColumns = Array.from(document.querySelectorAll('.day-hour-column'));
            dayIndex = allDayColumns.indexOf(dayColumn);
        }

        // Determine the hour based on which hour-row or day-hour-cell was dropped on
        let hourIndex = 0;
        if (dropTarget.classList.contains('hour-row')) {
            const allHourRows = Array.from(dropTarget.parentElement.querySelectorAll('.hour-row'));
            hourIndex = allHourRows.indexOf(dropTarget);
        } else if (dropTarget.classList.contains('day-hour-cell')) {
            const allCells = Array.from(dropTarget.parentElement.querySelectorAll('.day-hour-cell'));
            hourIndex = allCells.indexOf(dropTarget);
        }

        // Calculate minutes based on position within the hour cell (each hour is 60px tall)
        const minutesInHour = Math.floor((relativeY / 60) * 60);
        const totalMinutes = (hourIndex * 60) + minutesInHour;

        // Round to nearest 15 minutes
        const roundedMinutes = Math.round(totalMinutes / 15) * 15;
        const newHour = Math.floor(roundedMinutes / 60);
        const newMinute = roundedMinutes % 60;

        console.log(`Dropped at hour ${newHour}:${newMinute.toString().padStart(2, '0')}, day index: ${dayIndex}`);

        // Call back to Blazor
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnEventDropped', draggedEventId, dayIndex, newHour, newMinute)
                .then(() => {
                    console.log('Event drop processed');
                })
                .catch(err => {
                    console.error('Error processing drop:', err);
                });
        }
    }

    function createGhostElement(originalElement, durationMinutes) {
        // Create a ghost preview element
        ghostElement = originalElement.cloneNode(true);
        ghostElement.style.position = 'fixed';
        ghostElement.style.pointerEvents = 'none';
        ghostElement.style.opacity = '0.7';
        ghostElement.style.zIndex = '9999';
        ghostElement.style.display = 'none'; // Will be shown on drag
        document.body.appendChild(ghostElement);
    }

    // Cleanup function
    TempusCalendar.cleanup = function () {
        if (ghostElement && ghostElement.parentNode) {
            ghostElement.parentNode.removeChild(ghostElement);
            ghostElement = null;
        }
        dotNetHelper = null;
    };
})();
