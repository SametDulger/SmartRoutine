// Routines management functions

document.addEventListener('DOMContentLoaded', function() {
    if (!localStorage.getItem('token')) {
        window.location.href = '/Home/Login';
        return;
    }

    loadTodayRoutines();
});

let currentView = 'today';

async function loadTodayRoutines() {
    currentView = 'today';
    updateActiveTab('today');
    await loadRoutines('/routines/today');
}

async function loadWeekRoutines() {
    currentView = 'week';  
    updateActiveTab('week');
    await loadRoutines('/routines/week');
}

function updateActiveTab(activeTab) {
    document.querySelectorAll('.nav-pills .nav-link').forEach(link => {
        link.classList.remove('active');
    });
    
    if (activeTab === 'today') {
        document.querySelector('.nav-pills .nav-link:first-child').classList.add('active');
    } else {
        document.querySelector('.nav-pills .nav-link:last-child').classList.add('active');
    }
}

async function loadRoutines(endpoint) {
    try {
        const container = document.getElementById('routinesList');
        container.innerHTML = '<div class="text-center"><div class="spinner-border" role="status"></div></div>';
        
        const response = await makeAuthenticatedRequest(endpoint);
        if (!response || !response.ok) return;
        
        const routines = await response.json();
        
        if (routines.length === 0) {
            container.innerHTML = `
                <div class="text-center text-muted">
                    <p>Henüz rutin bulunmuyor.</p>
                    <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addRoutineModal">
                        İlk Rutini Ekleyin
                    </button>
                </div>
            `;
            return;
        }
        
        container.innerHTML = routines.map(routine => `
            <div class="routine-item ${routine.isCompletedToday ? 'completed' : ''} p-3 mb-3 rounded">
                <div class="d-flex justify-content-between align-items-start">
                    <div class="flex-grow-1">
                        <h5 class="mb-1">${routine.title}</h5>
                        ${routine.description ? `<p class="text-muted mb-2">${routine.description}</p>` : ''}
                        <div class="d-flex align-items-center text-sm text-muted">
                            <span class="me-3">🕐 ${formatTime(routine.timeOfDay)}</span>
                            <span class="me-3">${getRepeatTypeText(routine.repeatType)}</span>
                            ${!routine.isActive ? '<span class="badge bg-secondary">Pasif</span>' : ''}
                        </div>
                    </div>
                    <div class="d-flex gap-2">
                        ${routine.isCompletedToday ? 
                            '<span class="badge bg-success">✓ Tamamlandı</span>' :
                            `<button class="btn btn-sm btn-success" onclick="completeRoutine('${routine.id}')">Tamamla</button>`
                        }
                        <button class="btn btn-sm btn-outline-primary" onclick="editRoutine('${routine.id}')">Düzenle</button>
                        <button class="btn btn-sm btn-outline-danger" onclick="deleteRoutine('${routine.id}', '${routine.title}')">Sil</button>
                    </div>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Error loading routines:', error);
        document.getElementById('routinesList').innerHTML = 
            '<div class="alert alert-danger">Rutinler yüklenirken hata oluştu.</div>';
    }
}

function getRepeatTypeText(repeatType) {
    switch(repeatType) {
        case 1: return '📅 Günlük';
        case 2: return '📅 Haftalık';
        case 3: return '📅 Özel Günler';
        case 4: return '📅 Aralıklı';
        default: return '📅 Bilinmeyen';
    }
}

async function addRoutine() {
    const title = document.getElementById('modalTitle').value;
    const description = document.getElementById('modalDescription').value;
    const timeOfDay = document.getElementById('modalTimeOfDay').value;
    const repeatType = document.getElementById('modalRepeatType').value;

    try {
        const response = await makeAuthenticatedRequest('/routines', {
            method: 'POST',
            body: JSON.stringify({
                title,
                description,
                timeOfDay,
                repeatType
            })
        });

        if (response && response.ok) {
            showToast('success', 'Rutin başarıyla eklendi.');
            bootstrap.Modal.getInstance(document.getElementById('addRoutineModal')).hide();
            await refreshCurrentView();
        } else {
            const error = await response.json();
            showToast('error', error.message || 'Rutin eklenirken hata oluştu.');
        }
    } catch (error) {
        console.error('Error adding routine:', error);
        showToast('error', 'Bağlantı hatası oluştu.');
    }
}

async function editRoutine(routineId) {
    try {
        const response = await makeAuthenticatedRequest(`/routines/${currentView}`);
        if (!response || !response.ok) return;
        
        const routines = await response.json();
        const routine = routines.find(r => r.id === routineId);
        
        if (routine) {
            document.getElementById('editRoutineId').value = routine.id;
            document.getElementById('editTitle').value = routine.title;
            document.getElementById('editDescription').value = routine.description || '';
            document.getElementById('editTimeOfDay').value = formatTime(routine.timeOfDay);
            document.getElementById('editRepeatType').value = routine.repeatType;
            document.getElementById('editIsActive').checked = routine.isActive;
            
            new bootstrap.Modal(document.getElementById('editRoutineModal')).show();
        }
    } catch (error) {
        console.error('Error loading routine for edit:', error);
        showToast('error', 'Rutin bilgileri yüklenemedi.');
    }
}

async function updateRoutine() {
    const routineId = document.getElementById('editRoutineId').value;
    const title = document.getElementById('editTitle').value;
    const description = document.getElementById('editDescription').value;
    const timeOfDay = document.getElementById('editTimeOfDay').value;
    const repeatTypeValue = parseInt(document.getElementById('editRepeatType').value);
    const repeatType = getRepeatTypeString(repeatTypeValue);
    const isActive = document.getElementById('editIsActive').checked;

    try {
        const response = await makeAuthenticatedRequest(`/routines/${routineId}`, {
            method: 'PUT',
            body: JSON.stringify({
                title,
                description,
                timeOfDay,
                repeatType,
                isActive
            })
        });

        if (response && response.ok) {
            showToast('success', 'Rutin başarıyla güncellendi.');
            bootstrap.Modal.getInstance(document.getElementById('editRoutineModal')).hide();
            await refreshCurrentView();
        } else {
            const error = await response.json();
            showToast('error', error.message || 'Rutin güncellenirken hata oluştu.');
        }
    } catch (error) {
        console.error('Error updating routine:', error);
        showToast('error', 'Bağlantı hatası oluştu.');
    }
}

async function deleteRoutine(routineId, title) {
    if (!confirm(`"${title}" rutinini silmek istediğinizden emin misiniz?`)) {
        return;
    }

    try {
        const response = await makeAuthenticatedRequest(`/routines/${routineId}`, {
            method: 'DELETE'
        });

        if (response && response.ok) {
            showToast('success', 'Rutin başarıyla silindi.');
            await refreshCurrentView();
        } else {
            const error = await response.json();
            showToast('error', error.message || 'Rutin silinirken hata oluştu.');
        }
    } catch (error) {
        console.error('Error deleting routine:', error);
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
            await refreshCurrentView();
        } else {
            const error = await response.json();
            showToast('error', error.message || 'Rutin tamamlanırken hata oluştu.');
        }
    } catch (error) {
        console.error('Error completing routine:', error);
        showToast('error', 'Bağlantı hatası oluştu.');
    }
}

async function refreshCurrentView() {
    if (currentView === 'today') {
        await loadTodayRoutines();
    } else {
        await loadWeekRoutines();
    }
} 