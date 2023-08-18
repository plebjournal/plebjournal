const loadDcaCalculator = async () => {
  const { traces } = await fetch('/api/dca-calculator').then(res => res.json());
  const chart = document.getElementById('dca-calculator-container');
  const layout = {
    font: {
      family: 'monospace'
    },
    height: 600,
    showLegend: true,
    yaxis: {
      title: "Fiat Value",
      side: "left",
      type: "log",
    },
    yaxis2: {
      title: "BTC Stack",
      side: "right",
      overlaying: 'y'
    },
    yaxis3: {
      title: "Cost Basis",
      side: "right",
      overlaying: 'y'
    }, 
    xaxis: {
      type: 'date',
    },
    hovermode: 'x unified',
  };

  Plotly.newPlot(chart, traces, layout)
};

loadDcaCalculator();

htmx.on('dca-calculated', () => loadDcaCalculator());