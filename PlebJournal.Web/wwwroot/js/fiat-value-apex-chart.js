const formatDollar = (amount) => {
    const f = new Intl.NumberFormat(undefined, { currency: "CAD", style: "currency" })
    return f.format(amount);
}

const loadApexChart = async () => {
    const { traces, dates } = await fetch(`/api/fiat-value-apex-config`).then(res => res.json());
    const options = {
        chart: {
            type: 'area',
            height: 350,
            sparkLine: {
                enabled: true,
            },
        },
        stroke: {
            curve: 'smooth',
        },
        dataLabels: {
            enabled: false
        },
        xaxis: {
            type: 'datetime',
        },
        
        yaxis: {
            labels: {
                formatter: formatDollar
            },
        },

        legend: {
            horizontalAlign: 'left'
        },
        series: [
            traces,
        ],
    };
    
    var chart = new ApexCharts(document.querySelector("#fiat-value-apex-chart"), options);
    chart.render();

    document
        .querySelector('#chart-1-year')
        .addEventListener('click', function(e) {
            chart.zoomX(
                new Date(dates.oneYear).getTime(),
                new Date().getTime()
            );
        })
    
    document
        .querySelector('#chart-1-month')
        .addEventListener('click', function(e) {
            chart.zoomX(
                new Date(dates.oneMonth).getTime(),
                new Date().getTime()
            );
        });
    
    document
        .querySelector('#chart-6-months')
        .addEventListener('click', function(e) {
            chart.zoomX(
                new Date(dates.sixMonths).getTime(),
                new Date().getTime()
            );
        })

    document
        .querySelector('#chart-all-time')
        .addEventListener('click', function(e) {
            const startDate = traces.data[0] || { x: new Date() }; 
            chart.zoomX(
                new Date(startDate.x).getTime(),
                new Date().getTime()
            );
        })    
}

loadApexChart();