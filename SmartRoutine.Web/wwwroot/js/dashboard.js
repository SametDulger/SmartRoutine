// Dashboard functions

document.addEventListener('DOMContentLoaded', function() {
    // Check authentication
    if (!localStorage.getItem('token')) {
        window.location.href = '/Home/Login';
        return;
    }

    loadDashboardData();
    
    // Quick add form handler
    const quickAddForm = document.getElementById('quickAddForm');
    if (quickAddForm) {
        quickAddForm.addEventListener('submit', handleQuickAdd);
    }
});

async function loadDashboardData() {
    try {
        // Load stats
        await loadStats();
        
        // Load today's routines
        await loadTodayRoutines();
    } catch (error) {
        console.error('Error loading dashboard data:', error);
    }
}

async function loadStats() {
    try {
        const response = await makeAuthenticatedRequest('/stats/summary');
        if (!response || !response.ok) return;
        
        const stats = await response.json();
        
        document.getElementById('totalRoutines').textContent = stats.totalRoutines;
        document.getElementById('completedToday').textContent = stats.completedToday;
        document.getElementById('completionRate').textContent = stats.completionRate + '%';
        document.getElementById('currentStreak').textContent = stats.currentStreak;
    } catch (error) {
        console.error('Error loading stats:', error);
    }
}

async function loadTodayRoutines() {
    try {
        const response = await makeAuthenticatedRequest('/routines/today');
        if (!response || !response.ok) return;
        
        const routines = await response.json();
        const container = document.getElementById('todayRoutines');
        
        if (routines.length === 0) {
            container.innerHTML = `
                <div class="text-center text-muted">
                    <p>Henüz bugün için rutin bulunmuyor.</p>
                    <p>Yan panelden hızlıca rutin ekleyebilirsiniz.</p>
                </div>
            `;
            return;
        }
        
        container.innerHTML = routines.map(routine => `
            <div class="d-flex justify-content-between align-items-center p-3 mb-2 bg-light rounded">
                <div>
                    <h6 class="mb-1">${routine.title}</h6>
                    <small class="text-muted">${formatTime(routine.timeOfDay)}</small>
                </div>
                <div>
                    ${routine.isCompletedToday ? 
                        '<span class="badge bg-success">✓ Tamamlandı</span>' :
                        `<button class="btn btn-sm btn-outline-primary" onclick="completeRoutine('${routine.id}')">Tamamla</button>`
                    }
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Error loading routines:', error);
    }
}

async function handleQuickAdd(event) {
    event.preventDefault();
    
    const title = document.getElementById('title').value;
    const timeOfDay = document.getElementById('timeOfDay').value;

    try {
        const response = await makeAuthenticatedRequest('/routines', {
            method: 'POST',
            body: JSON.stringify({
                title: title,
                timeOfDay: timeOfDay,
                repeatType: "Daily"
            })
        });

        if (response && response.ok) {
            showToast('success', 'Rutin başarıyla eklendi.');
            document.getElementById('quickAddForm').reset();
            await loadDashboardData(); // Refresh data
        } else {
            const error = await response.json();
            showToast('error', error.message || 'Rutin eklenirken hata oluştu.');
        }
    } catch (error) {
        console.error('Error adding routine:', error);
        showToast('error', 'Bağlantı hatası oluştu.');
    }
}

async function completeRoutine(routineId) {
    try {
        const response = await makeAuthenticatedRequest(`/routines/${routineId}/complete`, {
            method: 'POST'
        });

        if (response && response.ok) {
            showToast('success', 'Rutin tamamlandı! 🎉');
            await loadDashboardData(); // Refresh data
        } else {
            const error = await response.json();
            showToast('error', error.message || 'Rutin tamamlanırken hata oluştu.');
        }
    } catch (error) {
        console.error('Error completing routine:', error);
        showToast('error', 'Bağlantı hatası oluştu.');
    }
} 