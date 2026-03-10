window.renderDonutChart = (canvasId, labels, data, colors) => {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    // Destroy existing chart instance if present
    const existing = Chart.getChart(canvas);
    if (existing) existing.destroy();

    new Chart(canvas, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'right' },
                tooltip: {
                    callbacks: {
                        label: (ctx) => {
                            const hrs = ctx.parsed;
                            const h = Math.floor(hrs);
                            const m = Math.round((hrs - h) * 60);
                            return ` ${ctx.label}: ${h}h ${m}m`;
                        }
                    }
                }
            }
        }
    });
};

window.downloadBase64File = (filename, base64) => {
    const link = document.createElement('a');
    link.href = 'data:text/markdown;base64,' + base64;
    link.download = filename;
    link.click();
};

// Ctrl+Shift+S — start/stop timer via a Blazor-callable dotnet reference
window.registerTimerShortcut = (dotnetRef) => {
    document.addEventListener('keydown', (e) => {
        if (e.ctrlKey && e.shiftKey && e.key === 'S') {
            e.preventDefault();
            dotnetRef.invokeMethodAsync('ToggleTimer');
        }
    });
};
// AI Usage Chart (bar and line)
window.renderAiUsageChart = (labels, counts, totals) => {
    const canvas = document.getElementById('aiUsageChart');
    if (!canvas) return;
    const existing = Chart.getChart(canvas);
    if (existing) existing.destroy();
    new Chart(canvas, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'AI Usage Count',
                    data: counts,
                    backgroundColor: '#4f8cff',
                    yAxisID: 'y',
                },
                {
                    label: 'Total Time Saved (min)',
                    data: totals,
                    type: 'line',
                    borderColor: '#00c896',
                    backgroundColor: 'rgba(0,200,150,0.1)',
                    yAxisID: 'y1',
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: { position: 'top' },
                tooltip: { enabled: true }
            },
            scales: {
                y: {
                    type: 'linear',
                    position: 'left',
                    title: { display: true, text: 'Usage Count' },
                    beginAtZero: true
                },
                y1: {
                    type: 'linear',
                    position: 'right',
                    title: { display: true, text: 'Time Saved (min)' },
                    beginAtZero: true,
                    grid: { drawOnChartArea: false }
                }
            }
        }
    });
};
