// Statistics page functions

document.addEventListener('DOMContentLoaded', function() {
    if (!localStorage.getItem('token')) {
        window.location.href = '/Home/Login';
        return;
    }

    loadStatsData();
});

async function loadStatsData() {
    try {
        await loadSummaryStats();
        await loadWeeklyChart();
    } catch (error) {
        console.error('Error loading stats data:', error);
    }
}

async function loadSummaryStats() {
    try {
        const response = await makeAuthenticatedRequest('/stats/summary');
        if (!response || !response.ok) return;
        
        const stats = await response.json();
        
        // Null/NaN koruması
        const completionRate = isNaN(stats.completionRate) ? 0 : stats.completionRate;
        const weeklyStats = stats.weeklyStats || {};

        // Update summary cards
        document.getElementById('totalRoutines').textContent = stats.totalRoutines ?? '-';
        document.getElementById('completedToday').textContent = stats.completedToday ?? '-';
        document.getElementById('completionRate').textContent = completionRate + '%';
        document.getElementById('currentStreak').textContent = stats.currentStreak ?? '-';
        
        // Update additional stats
        document.getElementById('bestStreak').textContent = stats.bestStreak ?? '-';
        
        // Calculate week completed
        const weekCompleted = Object.values(weeklyStats).reduce((sum, count) => sum + count, 0);
        document.getElementById('weekCompleted').textContent = weekCompleted;
        
        // Update progress bar
        const progressBar = document.getElementById('progressBar');
        progressBar.style.width = completionRate + '%';
        progressBar.setAttribute('aria-valuenow', completionRate);
        
    } catch (error) {
        console.error('Error loading summary stats:', error);
    }
}

async function loadWeeklyChart() {
    try {
        const response = await makeAuthenticatedRequest('/stats/summary');
        if (!response || !response.ok) return;
        
        const stats = await response.json();
        const weeklyStats = stats.weeklyStats || {};
        
        // Prepare chart data
        const labels = Object.keys(weeklyStats).map(date => {
            return new Date(date).toLocaleDateString('tr-TR', { 
                weekday: 'short',
                day: 'numeric',
                month: 'short'
            });
        });
        
        const data = Object.values(weeklyStats);
        
        // Create chart
        const ctx = document.getElementById('weeklyChart').getContext('2d');
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Tamamlanan Rutinler',
                    data: data,
                    borderColor: 'rgb(75, 192, 192)',
                    backgroundColor: 'rgba(75, 192, 192, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return `${context.parsed.y} rutin tamamlandı`;
                            }
                        }
                    }
                }
            }
        });
        
    } catch (error) {
        console.error('Error loading weekly chart:', error);
    }
} 