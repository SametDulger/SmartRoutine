// Authentication functions

// Toast mesajı gösterme fonksiyonu
function showToast(type, message) {
    const toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) return;

    const toastId = type === 'error' ? 'errorToast' : 'successToast';
    const toast = document.getElementById(toastId);
    if (!toast) return;

    const toastBody = toast.querySelector('.toast-body');
    if (toastBody) {
        toastBody.textContent = message;
    }

    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
}

document.addEventListener('DOMContentLoaded', function() {
    // Register form handler
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.addEventListener('submit', handleRegister);
    }

    // Login form handler  
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }

    // Check if already logged in, redirect to dashboard
    if (localStorage.getItem('token')) {
        window.location.href = '/Home/Dashboard';
    }
});

async function handleRegister(event) {
    event.preventDefault();
    const email = document.getElementById('email');
    const password = document.getElementById('password');
    const confirmPassword = document.getElementById('confirmPassword');
    let valid = true;
    // Temizle
    email.classList.remove('is-invalid');
    password.classList.remove('is-invalid');
    confirmPassword.classList.remove('is-invalid');
    // Validation
    if (!email.value || !email.value.includes('@')) {
        email.classList.add('is-invalid');
        valid = false;
    }
    if (!password.value || password.value.length < 6) {
        password.classList.add('is-invalid');
        valid = false;
    }
    if (password.value !== confirmPassword.value) {
        confirmPassword.classList.add('is-invalid');
        showToast('error', 'Şifreler eşleşmiyor.');
        valid = false;
    }
    if (!valid) return;
    
    const emailValue = email.value;
    const passwordValue = password.value;
    const confirmPasswordValue = confirmPassword.value;
    // displayName input yoksa email'in başını kullan
    let displayNameValue = '';
    const displayNameInput = document.getElementById('displayName');
    if (displayNameInput) {
        displayNameValue = displayNameInput.value.trim();
    } else {
        displayNameValue = emailValue.split('@')[0];
    }

    // Validate passwords match
    if (passwordValue !== confirmPasswordValue) {
        showToast('error', 'Şifreler eşleşmiyor.');
        return;
    }

    try {
        const response = await fetch(window.API_BASE_URL + '/auth/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                email: emailValue,
                password: passwordValue,
                displayName: displayNameValue
            })
        });

        if (response.ok) {
            // Registration successful, redirect to login
            window.location.href = '/Home/Login?message=Kayıt başarılı, giriş yapabilirsiniz.';
        } else {
            let errorMsg = 'Kayıt işlemi başarısız.';
            try {
                const errorData = await response.json();
                errorMsg = errorData.message || errorMsg;
            } catch {}
            showToast('error', errorMsg);
        }
    } catch (error) {
        showToast('error', 'Kayıt sırasında bir hata oluştu.');
    }
}

async function handleLogin(event) {
    event.preventDefault();
    const email = document.getElementById('email');
    const password = document.getElementById('password');
    let valid = true;
    email.classList.remove('is-invalid');
    password.classList.remove('is-invalid');
    if (!email.value || !email.value.includes('@')) {
        email.classList.add('is-invalid');
        valid = false;
    }
    if (!password.value || password.value.length < 6) {
        password.classList.add('is-invalid');
        valid = false;
    }
    if (!valid) return;
    
    const emailValue = email.value;
    const passwordValue = password.value;

    try {
        const response = await fetch(window.API_BASE_URL + '/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                email: emailValue,
                password: passwordValue
            })
        });

        if (response.ok) {
            const data = await response.json();
            // Login successful
            localStorage.setItem('token', data.accessToken);
            localStorage.setItem('refreshToken', data.refreshToken);
            localStorage.setItem('user', JSON.stringify(data.user));
            window.location.href = '/Home/Dashboard';
        } else {
            let errorMsg = 'Giriş işlemi başarısız.';
            try { errorMsg = (await response.json()).message || errorMsg; } catch {}
            showToast('error', errorMsg);
        }
    } catch (error) {
        console.error('Login error:', error);
        showToast('error', 'Bağlantı hatası oluştu.');
    }
} 