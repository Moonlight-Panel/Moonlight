window.initConsolePlugins = function () {
    window.XtermBlazor.registerAddon("xterm-addon-fit", new window.FitAddon.FitAddon());

    addEventListener('resize', (event) => {
        //XtermBlazor.invokeAddonFunction()
    });
};

window.initGraphs = function () {
    
};

window.initCpu = function () {
    var element = document.getElementById('cpuchart');

    var height = parseInt(KTUtil.css(element, 'height'));
    var labelColor = KTUtil.getCssVariableValue('--kt-gray-500');
    var borderColor = KTUtil.getCssVariableValue('--kt-gray-200');
    var baseColor = KTUtil.getCssVariableValue('--kt-info');
    var lightColor = KTUtil.getCssVariableValue('--kt-info-light');

    if (!element) {
        return;
    }

    var options = {
        series: [{
            name: 'CPU Auslastung',
            data: [0, 0, 0, 0, 0, 0, 0]
        }],
        chart: {
            fontFamily: 'inherit',
            type: 'area',
            height: height,
            toolbar: {
                show: false
            }
        },
        plotOptions: {},
        legend: {
            show: false
        },
        dataLabels: {
            enabled: false
        },
        fill: {
            type: 'solid',
            opacity: 1
        },
        stroke: {
            curve: 'smooth',
            show: true,
            width: 3,
            colors: [baseColor]
        },
        xaxis: {
            categories: [],
            axisBorder: {
                show: false,
            },
            axisTicks: {
                show: false
            },
            labels: {
                style: {
                    colors: labelColor,
                    fontSize: '12px'
                }
            },
            crosshairs: {
                position: 'front',
                stroke: {
                    color: baseColor,
                    width: 1,
                    dashArray: 3
                }
            },
            tooltip: {
                enabled: false,
                formatter: undefined,
                offsetY: 0,
                style: {
                    fontSize: '12px'
                }
            }
        },
        yaxis: {
            labels: {
                style: {
                    colors: labelColor,
                    fontSize: '12px'
                }
            }
        },
        states: {
            normal: {
                filter: {
                    type: 'none',
                    value: 0
                }
            },
            hover: {
                filter: {
                    type: 'none',
                    value: 0
                }
            },
            active: {
                allowMultipleDataPointsSelection: false,
                filter: {
                    type: 'none',
                    value: 0
                }
            }
        },
        colors: [lightColor],
        grid: {
            borderColor: borderColor,
            strokeDashArray: 4,
            yaxis: {
                lines: {
                    show: true
                }
            }
        },
        markers: {
            strokeColor: baseColor,
            strokeWidth: 3
        }
    };

    var chart = new ApexCharts(element, options);
    chart.render();
    
    window.console.cpuchart = chart;
};

window.updateCpu = function (value) {
    window.console.cpuchart.appendSeries({
        name: 'CPU Auslastung',
        data: value
    });
};