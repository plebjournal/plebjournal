const loadPortfolioSummary = async (horizon) => {
  const horizonQuery = horizon || '12-months';
  const { traces } = await fetch(`/api/portfolio-summary?horizon=${horizonQuery}`).then(res => res.json());
  const chart = document.getElementById('portfolio-chart');
  const layout = {
    font: {
      family: 'monospace'
    },
    yaxis: {
      title: "Fiat Value",
      side: "left",
      type: "log",
    },
    yaxis2: {
      title: "BTC",
      side: "right",
      overlaying: 'y'
    },
    xaxis: {
      type: 'date',
    },
    hovermode: 'x unified',
  };
  
  chart && Plotly.newPlot(chart, traces, layout)
};

htmx.on('tx-created', () => loadPortfolioSummary());
htmx.on('tx-deleted', () => loadPortfolioSummary());

htmx.on('show-chart', (evnt) => {
  loadPortfolioSummary(evnt.detail.value);
});
