// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Initialize tooltips
document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Handle form validation
    var forms = document.querySelectorAll('.needs-validation');
    Array.prototype.slice.call(forms).forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });

    // Handle delete confirmations
    var deleteButtons = document.querySelectorAll('[data-delete-url]');
    deleteButtons.forEach(function (button) {
        button.addEventListener('click', function (event) {
            event.preventDefault();
            var url = this.getAttribute('data-delete-url');
            var message = this.getAttribute('data-delete-message') || 'Are you sure you want to delete this item?';
            
            if (confirm(message)) {
                fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    }
                }).then(function (response) {
                    if (response.ok) {
                        window.location.reload();
                    } else {
                        alert('An error occurred while deleting the item.');
                    }
                });
            }
        });
    });

    // Handle status updates
    var statusButtons = document.querySelectorAll('[data-status-url]');
    statusButtons.forEach(function (button) {
        button.addEventListener('click', function (event) {
            event.preventDefault();
            var url = this.getAttribute('data-status-url');
            var status = this.getAttribute('data-status');
            var message = this.getAttribute('data-status-message') || 'Are you sure you want to update the status?';
            
            if (confirm(message)) {
                fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify({ status: status })
                }).then(function (response) {
                    if (response.ok) {
                        window.location.reload();
                    } else {
                        alert('An error occurred while updating the status.');
                    }
                });
            }
        });
    });

    // Handle feedback rating
    var ratingInputs = document.querySelectorAll('.rating-input');
    ratingInputs.forEach(function (input) {
        input.addEventListener('change', function () {
            var rating = this.value;
            var stars = this.parentElement.querySelectorAll('.rating-star');
            stars.forEach(function (star, index) {
                if (index < rating) {
                    star.classList.add('text-warning');
                } else {
                    star.classList.remove('text-warning');
                }
            });
        });
    });

    // Handle calendar event clicks
    var calendarEvents = document.querySelectorAll('.fc-event');
    calendarEvents.forEach(function (event) {
        event.addEventListener('click', function () {
            var eventId = this.getAttribute('data-event-id');
            if (eventId) {
                window.location.href = '/Instructor/Reservations/Details/' + eventId;
            }
        });
    });

    // Handle search functionality
    var searchInput = document.querySelector('#searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            var searchTerm = this.value.toLowerCase();
            var tableRows = document.querySelectorAll('tbody tr');
            
            tableRows.forEach(function (row) {
                var text = row.textContent.toLowerCase();
                if (text.includes(searchTerm)) {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            });
        });
    }

    // Handle date range picker
    var dateRangeInput = document.querySelector('#dateRange');
    if (dateRangeInput) {
        flatpickr(dateRangeInput, {
            mode: "range",
            dateFormat: "Y-m-d",
            onChange: function (selectedDates, dateStr) {
                if (selectedDates.length === 2) {
                    var startDate = selectedDates[0];
                    var endDate = selectedDates[1];
                    // Trigger any necessary updates based on the date range
                }
            }
        });
    }
});

// Add fade-in animation to elements with the fade-in class
document.addEventListener('DOMContentLoaded', function() {
    const fadeElements = document.querySelectorAll('.fade-in');
    fadeElements.forEach(element => {
        element.style.opacity = '1';
    });
});

// Form validation enhancement
function enhanceFormValidation() {
    const forms = document.querySelectorAll('form[data-validate]');
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
}

// Password visibility toggle
function setupPasswordToggle() {
    const passwordToggles = document.querySelectorAll('.password-toggle');
    passwordToggles.forEach(toggle => {
        toggle.addEventListener('click', function() {
            const input = this.previousElementSibling;
            const type = input.getAttribute('type') === 'password' ? 'text' : 'password';
            input.setAttribute('type', type);
            this.querySelector('i').classList.toggle('fa-eye');
            this.querySelector('i').classList.toggle('fa-eye-slash');
        });
    });
}

// Initialize all features
document.addEventListener('DOMContentLoaded', function() {
    enhanceFormValidation();
    setupPasswordToggle();
});
