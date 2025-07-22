// Main site JavaScript functions

window.API_BASE_URL = window.apiBaseUrl || '/api';

// Check authentication status and update navigation
function checkAuthStatus() {
    const token = localStorage.getItem('token');
    const navLogin = document.getElementById('nav-login');
    const navRegister = document.getElementById('nav-register');
    const navLogout = document.getElementById('nav-logout');
    const navDashboard = document.getElementById('nav-dashboard');
    const navRoutines = document.getElementById('nav-routines');
    const navStats = document.getElementById('nav-stats');

    if (token) {
        // User is logged in
        if (navLogin) navLogin.style.display = 'none';
        if (navRegister) navRegister.style.display = 'none';
        if (navLogout) navLogout.style.display = 'block';
        if (navDashboard) navDashboard.style.display = 'block';
        if (navRoutines) navRoutines.style.display = 'block';
        if (navStats) navStats.style.display = 'block';
    } else {
        // User is not logged in
        if (navLogin) navLogin.style.display = 'block';
        if (navRegister) navRegister.style.display = 'block';
        if (navLogout) navLogout.style.display = 'none';
        if (navDashboard) navDashboard.style.display = 'none';
        if (navRoutines) navRoutines.style.display = 'none';
        if (navStats) navStats.style.display = 'none';
    }
}

// Logout function
function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/';
}

async function refreshAccessToken() {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) return false;
    try {
        const response = await fetch(window.API_BASE_URL + '/auth/refresh', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken })
        });
        if (!response.ok) {
            return false;
        }
        const data = await response.json();
        localStorage.setItem('token', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('user', JSON.stringify(data.user));
        return true;
    } catch {
        return false;
    }
}

// Make authenticated API request
async function makeAuthenticatedRequest(url, options = {}) {
    let token = localStorage.getItem('token');
    
    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
            ...(token && { 'Authorization': `Bearer ${token}` })
        }
    };

    const mergedOptions = {
        ...defaultOptions,
        ...options,
        headers: {
            ...defaultOptions.headers,
            ...options.headers
        }
    };

    try {
        let response = await fetch(window.API_BASE_URL + url, mergedOptions);
        
        if (response.status === 401) {
            // Token expired or invalid, try refresh
            const refreshed = await refreshAccessToken();
            if (refreshed) {
                token = localStorage.getItem('token');
                mergedOptions.headers['Authorization'] = `Bearer ${token}`;
                response = await fetch(window.API_BASE_URL + url, mergedOptions);
                if (response.status !== 401) {
                    return response;
                }
            }
            logout();
            return null;
        }

        return response;
    } catch (error) {
        console.error('API request failed:', error);
        return null;
    }
}

function getCsrfToken() {
    const input = document.querySelector('input[name=__RequestVerificationToken]');
    return input ? input.value : '';
}

async function csrfFetch(url, options = {}) {
    const token = getCsrfToken();
    options.headers = {
        ...(options.headers || {}),
        'X-XSRF-TOKEN': token
    };
    return fetch(url, options);
}

// Format date for display
function formatDate(date) {
    return new Date(date).toLocaleDateString('tr-TR', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

// Format time for display
function formatTime(timeString) {
    return timeString.substring(0, 5); // HH:MM format
}

// Show toast notification
function showToast(type, message) {
    const toastElement = document.getElementById(type + 'Toast');
    const messageElement = document.getElementById(type + 'Message');
    
    if (toastElement && messageElement) {
        messageElement.textContent = message;
        const toast = new bootstrap.Toast(toastElement);
        toast.show();
    }
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    checkAuthStatus();
    
    // Set today's date if element exists
    const todayDateElement = document.getElementById('todayDate');
    if (todayDateElement) {
        todayDateElement.textContent = formatDate(new Date());
    }
}); 