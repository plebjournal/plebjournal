const loadPlotlyChart = async () => {
  const chart = document.getElementById('wma-chart-container');
  const data = await fetch('/api/200-wma').then(d => d.json());
  const priceTrace = {
    name: 'BTC USD',
    x: data.price.map(xy => xy.x),
    y: data.price.map(xy => xy.y),
    mode: 'lines',
  };
  const wmaTrace = {
    name: '200 WMA',
    x: data.wma.map(xy => xy.x),
    y: data.wma.map(xy => xy.y),
    mode: 'lines',
  };
  const purchasesTrace = {
    name: 'Purchases',
    x: data.purchases.map(xy => xy.x),
    y: data.purchases.map(xy => xy.y),
    mode: 'markers',
    type: 'scatter',
  };
  
  const layout = {
    title: 'BTC USD - 200 WMA',
    height: 600,
    yaxis: {
      type: 'log' 
    },
    hovermode: 'x unified',
  };
  Plotly.newPlot(chart, [priceTrace, wmaTrace, purchasesTrace], layout);
};

loadPlotlyChart();