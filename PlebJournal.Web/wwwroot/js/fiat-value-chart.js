const loadFiatValueChart = async (horizon) => {
  const horizonQuery = horizon || '12-months';
  const { traces } = await fetch(`/api/fiat-value-chart-config?horizon=${horizonQuery}`).then(res => res.json());
  const chart = document.getElementById('fiat-value-chart');
  const layout = {
    font: {
      family: 'monospace'
    },
    showLegend: true,
    yaxis: {
      type: 'log',
      side: 'right',
    },
    hovermode: 'x unified',
  };

  Plotly.newPlot(chart, traces, layout)
};

htmx.on("show-chart-fiat-value", (evnt) => loadFiatValueChart(evnt.detail.value));