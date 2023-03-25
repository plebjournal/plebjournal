const loadWorkbenchChart = async () => {
  const { traces } = await fetch('/api/workbench-config').then(res => res.json());
  const chart = document.getElementById('workbench-chart-container');
  const layout = {
    showLegend: true,
    height: 800,
    yaxis: {
      type: 'log',
    },
    hovermode: 'x unified',
  };

  Plotly.newPlot(chart, traces, layout)
};
const reloadChart = async () => {
  const chart = document.getElementById('workbench-chart-container');
  const { traces } = await fetch('/api/workbench-config').then(res => res.json());
  const layout = {
    showLegend: true,
    height: 800,
    yaxis: {
      type: 'log',
      title: 'btc-usd'
    },
    hovermode: 'x unified',
  };
  Plotly.newPlot(chart, traces, layout)
};

loadWorkbenchChart();

htmx.on("formula-updated", () => reloadChart());