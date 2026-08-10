// system-status.js - Updates the API status badge in the header

document.addEventListener('DOMContentLoaded', () => {
    const statusBadge = document.getElementById('status-badge');
    const badgeText = document.getElementById('badge-text');

    const checkStatus = async () => {
        // Set checking state
        if (statusBadge) {
            statusBadge.className = 'status-badge checking';
            if (badgeText) badgeText.textContent = 'Checking API...';
        }

        try {
            await window.ApiClient.get('/api/system/status');

            // Update status badge to online
            if (statusBadge) {
                statusBadge.className = 'status-badge online';
                if (badgeText) badgeText.textContent = 'API: Online';
            }
        } catch (error) {
            // Update status badge to offline
            if (statusBadge) {
                statusBadge.className = 'status-badge offline';
                if (badgeText) badgeText.textContent = 'API: Offline';
            }
        }
    };

    // Execute check on page load
    checkStatus();

    // Query status every 30 seconds
    setInterval(checkStatus, 30000);
});
