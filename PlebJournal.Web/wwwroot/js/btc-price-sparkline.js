async function loadSparkChart() {
    const { traces } = await fetch('/api/btc-price-chart').then(res => res.json());
    const chart = document.getElementById('btc-price-card-sparkline');
    const layout = {
        margin: { t: 0, b: 0, l: 0, r: 0 },
        showLegend: false,
        yaxis: {
            showgrid: false,
            zeroline: false
        },
        xaxis: {
            showgrid: false,
            zeroline: false
        },
        height: 40
    };
    const config = {
      displayModeBar: false  
    };

    chart && Plotly.newPlot(chart, traces, layout, config);
}

loadSparkChart();