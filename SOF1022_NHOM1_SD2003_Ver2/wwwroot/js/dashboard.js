// ==========================================================================
// LIBRARY MANAGEMENT SYSTEM - DASHBOARD JAVASCRIPT
// ==========================================================================

document.addEventListener("DOMContentLoaded", () => {
    DashboardApp.init();
});

const DashboardApp = {
    borrowChart: null,
    categoryChart: null,

    init() {
        this.initBorrowChart();
        this.initCategoryChart();
        this.initCounters();
        this.initProgressBars();
        this.bindEvents();
    },

    bindEvents() {
        window.addEventListener("resize", () => {
            if (this.borrowChart) this.borrowChart.resize();
            if (this.categoryChart) this.categoryChart.resize();
        });
    },

    // 1. Borrow Chart (6 Months Trend)
    initBorrowChart() {
        const canvas = document.getElementById("borrowChart");
        if (!canvas) return;

        let labels = [];
        let values = [];

        try {
            labels = JSON.parse(canvas.dataset.labels || "[]");
            values = JSON.parse(canvas.dataset.values || "[]");
        } catch (e) {
            console.error("Error parsing borrowChart data", e);
        }

        if (labels.length === 0) {
            labels = ["T6", "T7", "T8", "T9", "T10", "T11"];
            values = [0, 0, 0, 0, 0, 0];
        }

        const ctx = canvas.getContext("2d");

        // Create subtle gradient
        const gradient = ctx.createLinearGradient(0, 0, 0, 300);
        gradient.addColorStop(0, "rgba(37, 99, 235, 0.9)");
        gradient.addColorStop(1, "rgba(59, 130, 246, 0.45)");

        this.borrowChart = new Chart(canvas, {
            type: "bar",
            data: {
                labels: labels,
                datasets: [{
                    label: "Lượt mượn",
                    data: values,
                    backgroundColor: gradient,
                    hoverBackgroundColor: "#1d4ed8",
                    borderRadius: 8,
                    borderSkipped: false,
                    maxBarThickness: 42
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 1000,
                    easing: "easeOutQuart"
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: "#0f172a",
                        titleFont: { size: 13, weight: "600" },
                        bodyFont: { size: 12 },
                        padding: 10,
                        cornerRadius: 8,
                        displayColors: false,
                        callbacks: {
                            label: function (context) {
                                return `Số lượt mượn: ${context.parsed.y} lượt`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: "#64748b",
                            font: { size: 12, weight: "500" }
                        }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            precision: 0,
                            color: "#64748b",
                            font: { size: 12 }
                        },
                        grid: {
                            color: "#f1f5f9"
                        }
                    }
                }
            }
        });
    },

    // 2. Category Doughnut Chart
    initCategoryChart() {
        const canvas = document.getElementById("categoryChart");
        if (!canvas) return;

        let labels = [];
        let values = [];

        try {
            labels = JSON.parse(canvas.dataset.labels || "[]");
            values = JSON.parse(canvas.dataset.values || "[]");
        } catch (e) {
            console.error("Error parsing categoryChart data", e);
        }

        if (labels.length === 0) {
            labels = ["Chưa phân loại"];
            values = [1];
        }

        const colors = [
            "#2563eb",
            "#10b981",
            "#f59e0b",
            "#8b5cf6",
            "#ec4899",
            "#06b6d4",
            "#64748b"
        ];

        this.categoryChart = new Chart(canvas, {
            type: "doughnut",
            data: {
                labels: labels,
                datasets: [{
                    data: values,
                    backgroundColor: colors.slice(0, labels.length),
                    borderWidth: 2,
                    borderColor: "#ffffff",
                    hoverOffset: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: "68%",
                animation: {
                    duration: 1100,
                    easing: "easeOutQuart"
                },
                plugins: {
                    legend: {
                        position: "bottom",
                        labels: {
                            usePointStyle: true,
                            pointStyle: "circle",
                            padding: 14,
                            boxWidth: 8,
                            boxHeight: 8,
                            font: { size: 11.5, weight: "500" },
                            color: "#334155"
                        }
                    },
                    tooltip: {
                        backgroundColor: "#0f172a",
                        padding: 10,
                        cornerRadius: 8,
                        callbacks: {
                            label: function (context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const val = context.raw || 0;
                                const pct = total > 0 ? Math.round((val / total) * 100) : 0;
                                return ` ${context.label}: ${val} cuốn (${pct}%)`;
                            }
                        }
                    }
                }
            }
        });
    },

    // 3. Counter Animation
    initCounters() {
        const counters = document.querySelectorAll(".counter");
        counters.forEach(counter => {
            const raw = counter.dataset.target || counter.textContent || "0";
            const target = parseInt(raw.replace(/,/g, "").replace(/\./g, ""), 10);
            if (isNaN(target)) return;

            const duration = 1200;
            const start = performance.now();

            function update(timestamp) {
                const progress = Math.min((timestamp - start) / duration, 1);
                const current = Math.floor(target * (1 - Math.pow(1 - progress, 3)));
                counter.textContent = current.toLocaleString("vi-VN");

                if (progress < 1) {
                    requestAnimationFrame(update);
                }
            }

            requestAnimationFrame(update);
        });
    },

    // 4. Progress Bars Animation
    initProgressBars() {
        const bars = document.querySelectorAll(".progress-bar");
        bars.forEach(bar => {
            const originalWidth = bar.style.width;
            bar.style.width = "0%";
            setTimeout(() => {
                bar.style.width = originalWidth;
            }, 100);
        });
    }
};
