// api.js - Fetch API client abstraction

const ApiClient = {
    async get(endpoint) {
        const baseUrl = window.DevForgeConfig?.apiUrl || 'http://localhost:5057';
        const url = `${baseUrl}${endpoint}`;

        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 5000); // 5s timeout

        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                },
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
    }
};

// Export to window scope
window.ApiClient = ApiClient;
