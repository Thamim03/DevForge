// api.js - Fetch API client abstraction

const ApiClient = {
    _getBaseUrl() {
        let baseUrl = window.DevForgeConfig?.apiUrl || 'http://localhost:5057';

        // Auto-detect IIS Express vs Kestrel environment based on the host page port
        const currentPort = window.location.port;
        if (currentPort === '44373' || currentPort === '64202') {
            // Running Web under IIS Express. Map to IIS Express API ports
            baseUrl = window.location.protocol === 'https:' ? 'https://localhost:44305' : 'http://localhost:64153';
        } else if (currentPort === '7246' || currentPort === '5251') {
            // Running Web under Kestrel. Map to Kestrel API ports
            baseUrl = window.location.protocol === 'https:' ? 'https://localhost:7172' : 'http://localhost:5057';
        }
        return baseUrl;
    },

    async get(endpoint) {
        const baseUrl = this._getBaseUrl();
        const url = `${baseUrl}${endpoint}`;

        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 5000); // 5s timeout

        try {
            const headers = {
                'Accept': 'application/json'
            };
            if (window.DevForgeConfig?.token) {
                headers['Authorization'] = `Bearer ${window.DevForgeConfig.token}`;
            }

            const response = await fetch(url, {
                method: 'GET',
                headers: headers,
                signal: controller.signal
            });

            clearTimeout(timeoutId);

            if (!response.ok) {
                throw new Error(`Server returned HTTP ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            clearTimeout(timeoutId);
            console.error(`API Fetch failed for [${url}]:`, error);
            throw error;
        }
    },

    async post(endpoint, body) {
        const baseUrl = this._getBaseUrl();
        const url = `${baseUrl}${endpoint}`;

        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 5000); // 5s timeout

        try {
            const headers = {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            };
            if (window.DevForgeConfig?.token) {
                headers['Authorization'] = `Bearer ${window.DevForgeConfig.token}`;
            }

            const response = await fetch(url, {
                method: 'POST',
                headers: headers,
                body: JSON.stringify(body),
                signal: controller.signal
            });

            clearTimeout(timeoutId);

            if (!response.ok) {
                let errMsg = `Server returned HTTP ${response.status}`;
                try {
                    const errObj = await response.json();
                    if (errObj && errObj.message) {
                        errMsg = errObj.message;
                    }
                } catch (_) {}
                throw new Error(errMsg);
            }

            return await response.json();
        } catch (error) {
            clearTimeout(timeoutId);
            console.error(`API Fetch failed for [${url}]:`, error);
            throw error;
        }
    }
};

// Export to window scope
window.ApiClient = ApiClient;
