// API base URL'yi ViewBag'den al

export async function apiRequest(endpoint, options = {}) {
    const token = localStorage.getItem('token');
    const headers = {
        'Content-Type': 'application/json',
        ...(token && { 'Authorization': `Bearer ${token}` })
    };
    const mergedOptions = {
        ...options,
        headers: {
            ...headers,
            ...(options.headers || {})
        }
    };
    const response = await fetch(`${window.API_BASE_URL}${endpoint}`, mergedOptions);
    if (!response.ok) {
        let errorMessage = 'Bir hata oluştu.';
        try {
            const errorData = await response.json();
            errorMessage = errorData.message || errorMessage;
        } catch {}
        throw new Error(errorMessage);
    }
    return response.json();
} 