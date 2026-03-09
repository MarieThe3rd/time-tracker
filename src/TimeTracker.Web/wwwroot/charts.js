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
