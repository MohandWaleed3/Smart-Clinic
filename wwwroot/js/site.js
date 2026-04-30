// ═══════════════════════════════════════════════════════════
// SmartClinic — Client-Side Enhancements
// ═══════════════════════════════════════════════════════════

document.addEventListener('DOMContentLoaded', function () {

    // ── Password Show/Hide Toggle (Event Delegation) ──
    // Moved to _Layout.cshtml to prevent browser caching issues.

    // ── Auto-dismiss alerts after 5 seconds ──
    document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
        setTimeout(function () {
            var bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 5000);
    });

    // ── Active nav-link highlighting (fallback) ──
    var currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.navbar-nav .nav-link').forEach(function (link) {
        var href = link.getAttribute('href');
        if (href && href !== '/' && currentPath.startsWith(href.toLowerCase())) {
            link.classList.add('active-link');
        }
    });

    // ── Table row click-to-highlight ──
    document.querySelectorAll('.table-hover tbody tr').forEach(function (row) {
        row.style.cursor = 'default';
    });

    // ── Form validation visual feedback ──
    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function () {
            var btn = form.querySelector('button[type="submit"]');
            if (btn && form.checkValidity()) {
                btn.disabled = true;
                var originalText = btn.innerHTML;
                btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status"></span>Processing...';
                // Re-enable after 3s in case of server-side redirect back
                setTimeout(function () {
                    btn.disabled = false;
                    btn.innerHTML = originalText;
                }, 3000);
            }
        });
    });

    // ── Smooth scroll to validation errors ──
    var firstError = document.querySelector('.text-danger:not(:empty), .validation-summary-errors');
    if (firstError) {
        firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

});
