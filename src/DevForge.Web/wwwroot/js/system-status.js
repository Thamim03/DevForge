// system-status.js - Updates status indicators and terminal outputs

document.addEventListener('DOMContentLoaded', () => {
    const statusBadge = document.getElementById('status-badge');
    const badgeText = document.getElementById('badge-text');
    const refreshBtn = document.getElementById('refresh-status-btn');

    // Terminal DOM nodes
    const termTarget = document.getElementById('term-target');
    const termConnection = document.getElementById('term-connection');
    const termAppName = document.getElementById('term-app-name');
    const termVersion = document.getElementById('term-version');
    const termDb = document.getElementById('term-db');
    const termChecks = document.getElementById('term-checks');
    const termLogs = document.getElementById('term-logs');

    const checkStatus = async () => {
        // Render checking states
        if (statusBadge) {
            statusBadge.className = 'status-badge checking';
            if (badgeText) badgeText.textContent = 'Checking API...';
        }
        if (termConnection) termConnection.innerHTML = '<span class="text-amber-500 font-semibold animate-pulse">CHECKING...</span>';

        const apiUrl = window.DevForgeConfig?.apiUrl || 'http://localhost:5057';
        if (termTarget) termTarget.textContent = `${apiUrl}/api/v1/system/status`;

        try {
            const data = await window.ApiClient.get('/api/v1/system/status');

            // 1. Update status badge
            if (statusBadge) {
                statusBadge.className = 'status-badge online';
                if (badgeText) badgeText.textContent = 'API: Online';
            }

            // 2. Populate terminal panel details
            if (termConnection) termConnection.innerHTML = '<span style="color: var(--success-color); font-weight: 600;">● ACTIVE</span>';
            if (termAppName) termAppName.textContent = data.application || 'N/A';
            if (termVersion) termVersion.textContent = data.version || 'N/A';

            if (termDb) {
                if (data.databaseConnection === 'Connected') {
                    termDb.innerHTML = '<span style="color: var(--success-color);">● Connected</span>';
                } else {
                    termDb.innerHTML = `<span style="color: var(--danger-color);" title="${data.databaseError || ''}">● Offline</span>`;
                }
            }

            if (termChecks) termChecks.textContent = data.totalStatusChecks !== undefined ? `${data.totalStatusChecks} checks` : 'N/A';

            if (termLogs) {
                termLogs.innerHTML = `<div class="terminal-log-success" style="color: var(--success-color);">[SUCCESS] API connection established. Status query returned code 200 OK at ${new Date().toLocaleTimeString()}</div>`;
            }

        } catch (error) {
            // Render offline states
            if (statusBadge) {
                statusBadge.className = 'status-badge offline';
                if (badgeText) badgeText.textContent = 'API: Offline';
            }

            if (termConnection) termConnection.innerHTML = '<span style="color: var(--danger-color); font-weight: 600;">● FAILED</span>';
            if (termAppName) termAppName.textContent = '-';
            if (termVersion) termVersion.textContent = '-';
            if (termDb) termDb.innerHTML = '<span style="color: var(--danger-color);">● Offline</span>';
            if (termChecks) termChecks.textContent = '-';

            if (termLogs) {
                termLogs.innerHTML = `<div class="terminal-log-error" style="color: var(--danger-color);">[ERROR] Fetch aborted or endpoint unreachable. Cause: ${error.message || 'Network Refused'}</div>`;
            }
        }
    };

    // Execute check on load
    checkStatus();

    // Loop check every 30 seconds
    setInterval(checkStatus, 30000);

    // Bind manual refresh trigger
    if (refreshBtn) {
        refreshBtn.addEventListener('click', (e) => {
            e.preventDefault();
            checkStatus();
        });
    }
});
