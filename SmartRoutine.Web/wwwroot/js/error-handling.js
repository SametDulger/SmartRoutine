// Enhanced error handling for the web application

// Global error handler for unhandled promises
window.addEventListener('unhandledrejection', function(event) {
    console.error('Unhandled promise rejection:', event.reason);
    showToast('Beklenmeyen bir hata oluştu. Lütfen sayfayı yenileyip tekrar deneyin.', 'error');
    
    // Prevent the default unhandled rejection error
    event.preventDefault();
});

// Global error handler for JavaScript errors
window.addEventListener('error', function(event) {
    console.error('JavaScript error:', event.error);
    showToast('Bir hata oluştu. Lütfen sayfayı yenileyip tekrar deneyin.', 'error');
});

// Enhanced API error handling
async function handleApiResponse(response) {
    if (!response.ok) {
        let errorMessage = 'Bir hata oluştu.';
        
        try {
            const errorData = await response.json();
            errorMessage = errorData.message || errorMessage;
        } catch (e) {
            // If we can't parse the error response, use the status text
            errorMessage = response.statusText || errorMessage;
        }

        // Handle specific status codes
        switch (response.status) {
            case 400:
                showToast('Geçersiz veri gönderildi: ' + errorMessage, 'warning');
                break;
            case 401:
                showToast('Oturum süreniz doldu. Lütfen tekrar giriş yapın.', 'error');
                localStorage.removeItem('token');
                window.location.href = '/Home/Login';
                return null;
            case 403:
                showToast('Bu işlemi gerçekleştirmek için yetkiniz yok.', 'error');
                break;
            case 404:
                showToast('İstenen kaynak bulunamadı.', 'error');
                break;
            case 429:
                showToast('Çok fazla istek gönderildi. Lütfen bir süre bekleyip tekrar deneyin.', 'warning');
                break;
            case 500:
                showToast('Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.', 'error');
                break;
            default:
                showToast(errorMessage, 'error');
        }
        
        throw new Error(errorMessage);
    }
    
    return response;
}

// Enhanced makeAuthenticatedRequest function with retry logic
async function makeAuthenticatedRequestWithRetry(url, options = {}, maxRetries = 3) {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/Home/Login';
        return null;
    }

    options.headers = {
        ...options.headers,
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };

    let lastError;
    
    for (let attempt = 1; attempt <= maxRetries; attempt++) {
        try {
            const response = await fetch(API_BASE_URL + url, options);
            return await handleApiResponse(response);
        } catch (error) {
            lastError = error;
            
            if (attempt === maxRetries) {
                throw error;
            }
            
            // Exponential backoff: wait longer between retries
            const delay = Math.pow(2, attempt) * 1000;
            await new Promise(resolve => setTimeout(resolve, delay));
            
            console.log(`Request failed, retrying... (${attempt}/${maxRetries})`);
        }
    }
    
    throw lastError;
}

// Network status monitoring
let isOnline = navigator.onLine;

window.addEventListener('online', () => {
    if (!isOnline) {
        showToast('İnternet bağlantısı yeniden kuruldu.', 'success');
        isOnline = true;
        
        // Retry any pending operations
        retryFailedOperations();
    }
});

window.addEventListener('offline', () => {
    showToast('İnternet bağlantısı kesildi. Bazı özellikler çalışmayabilir.', 'warning');
    isOnline = false;
});

// Queue for failed operations to retry when back online
let failedOperations = [];

function addFailedOperation(operation) {
    failedOperations.push(operation);
}

async function retryFailedOperations() {
    const operations = [...failedOperations];
    failedOperations = [];
    
    for (const operation of operations) {
        try {
            await operation();
        } catch (error) {
            console.error('Failed to retry operation:', error);
            // Re-add to queue if still failing
            failedOperations.push(operation);
        }
    }
}

// Enhanced toast function with different types
function showToast(message, type = 'info', duration = 5000) {
    // Remove existing toasts
    const existingToasts = document.querySelectorAll('.toast');
    existingToasts.forEach(toast => toast.remove());

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
        <div class="toast-content">
            <span class="toast-icon">${getToastIcon(type)}</span>
            <span class="toast-message">${message}</span>
            <button class="toast-close" onclick="this.parentElement.parentElement.remove()">&times;</button>
        </div>
    `;
    
    // Add styles if not already present
    if (!document.getElementById('toast-styles')) {
        const style = document.createElement('style');
        style.id = 'toast-styles';
        style.textContent = `
            .toast {
                position: fixed;
                top: 20px;
                right: 20px;
                padding: 12px 16px;
                border-radius: 8px;
                color: white;
                font-weight: 500;
                box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                z-index: 10000;
                max-width: 400px;
                animation: slideIn 0.3s ease-out;
            }
            .toast-content {
                display: flex;
                align-items: center;
                gap: 8px;
            }
            .toast-close {
                background: none;
                border: none;
                color: white;
                font-size: 18px;
                cursor: pointer;
                margin-left: auto;
                opacity: 0.8;
            }
            .toast-close:hover { opacity: 1; }
            .toast-success { background-color: #28a745; }
            .toast-error { background-color: #dc3545; }
            .toast-warning { background-color: #ffc107; color: #212529; }
            .toast-info { background-color: #17a2b8; }
            @keyframes slideIn {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
        `;
        document.head.appendChild(style);
    }
    
    document.body.appendChild(toast);
    
    // Auto remove after duration
    setTimeout(() => {
        if (toast.parentElement) {
            toast.style.animation = 'slideIn 0.3s ease-out reverse';
            setTimeout(() => toast.remove(), 300);
        }
    }, duration);
}

function getToastIcon(type) {
    switch (type) {
        case 'success': return '✓';
        case 'error': return '✕';
        case 'warning': return '⚠';
        case 'info': 
        default: return 'ℹ';
    }
}

// Validation helpers
function validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

function validatePassword(password) {
    // At least 6 characters, 1 uppercase, 1 lowercase, 1 digit
    const re = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$/;
    return re.test(password);
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
} 