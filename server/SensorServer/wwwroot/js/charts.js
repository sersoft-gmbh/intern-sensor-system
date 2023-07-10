'use strict';

Chart.defaults.plugins.tooltip.callbacks.label = function (context) {
    let label = context.dataset.label || '';
    if (label) label += ': ';
    if (context.parsed.y !== null)
        label += celsiusFormatter.format(context.parsed.y);
    return label;
};

const currentLocationChartElement = document.getElementById('location-temperature-chart');
const currentLocationChart = new Chart(currentLocationChartElement, {
    type: 'line',
    data: {
        datasets: [],
    },
    options: {
        responsive: true,
        parsing: {
            xAxisKey: 'date',
            yAxisKey: 'temperatureCelsius',
        },
        scales: {
            x: {
                type: 'time',
                time: {
                    unit: 'minute',
                },
            },
            y: {
                type: 'linear',
                ticks: {
                    callback: function (value, index, ticks) {
                        return celsiusFormatter.format(value);
                    },
                },
            },
        },
        plugins: {
            legend: {
                display: false,
            },
        },
    },
});

const locationsChartElement = document.getElementById('locations-chart');
const locationsChart = new Chart(locationsChartElement, {
    type: 'line',
    data: {
        datasets: [],
    },
    options: {
        responsive: true,
        parsing: {
            xAxisKey: 'date',
            yAxisKey: 'temperatureCelsius',
        },
        scales: {
            x: {
                type: 'time',
                time: {
                    unit: 'minute',
                },
            },
            y: {
                type: 'linear',
                ticks: {
                    callback: function (value, index, ticks) {
                        return celsiusFormatter.format(value);
                    },
                },
            },
        },
        plugins: {
            legend: {
                display: true,
            },
            colors: {
                forceOverride: true,
            },
        },
    },
});

const chartMeasurementsCount = 125;
const chartMinMaxPaddingCelsius = 0.75;

function updateChartDataSet(currentData, newData) {
    while (currentData.length > 0 && !deepEqual(currentData[0], newData[0])) {
        currentData.shift();
    }
    let index = 0;
    while (index < Math.min(currentData.length, newData.length)) {
        if (!deepEqual(currentData[index], newData[index])) {
            currentData.splice(index, 0, newData[index]);
        }
        index++;
    }
    while (index < newData.length) {
        currentData.push(newData[index]);
        index++;
    }
}

/**
 * @param {string | null} currentLocation
 */
async function updateCurrentLocationChart(currentLocation) {
    const newMeasurementsForLocation = (await fetchMeasurements('descending', chartMeasurementsCount, currentLocation)).reverse();
    const newTemperaturesForLocation = newMeasurementsForLocation.map(m => m.temperatureCelsius);
    currentLocationChart.options.scales.y.min = Math.min(...newTemperaturesForLocation) - chartMinMaxPaddingCelsius;
    currentLocationChart.options.scales.y.max = Math.max(...newTemperaturesForLocation) + chartMinMaxPaddingCelsius;
    if (newMeasurementsForLocation.length <= 0) {
        currentLocationChart.data.datasets = [];
    } else if (currentLocationChart.data.datasets.length <= 0) {
        currentLocationChart.data.datasets = [{
            data: newMeasurementsForLocation,
        }];
    } else {
        updateChartDataSet(currentLocationChart.data.datasets[0].data, newMeasurementsForLocation);
    }
    if (newMeasurementsForLocation.length > 0) {
        currentLocationChartElement.classList.remove('d-none');
    } else {
        currentLocationChartElement.classList.add('d-none');
    }
    currentLocationChart.update();
}

async function updateAllLocationsChart() {
    const newMeasurements = (await fetchMeasurements('descending', chartMeasurementsCount)).reverse();
    let locationsInMeasurements = [];
    for (const measurement of newMeasurements) {
        if (locationsInMeasurements.indexOf(measurement.location) === -1)
            locationsInMeasurements.push(measurement.location)
    }
    locationsInMeasurements.sort();
    const newTemperatures = newMeasurements.map(m => m.temperatureCelsius);
    locationsChart.options.scales.y.min = Math.min(...newTemperatures) - chartMinMaxPaddingCelsius;
    locationsChart.options.scales.y.max = Math.max(...newTemperatures) + chartMinMaxPaddingCelsius;
    if (newMeasurements.length <= 0) {
        locationsChart.data.datasets = [];
    } else if (locationsChart.data.datasets.length <= 0) {
        for (const location of locationsInMeasurements) {
            locationsChart.data.datasets.push({
                data: newMeasurements.filter(m => m.location === location),
                label: location,
            });
        }
    } else {
        for (let i = 0; i < locationsInMeasurements.length; i++) {
            const location = locationsInMeasurements[i];
            const locationMeasurements = newMeasurements.filter(m => m.location === location);
            if (locationsChart.data.datasets.length <= i) {
                locationsChart.data.datasets.push({
                    data: locationMeasurements,
                    label: location,
                });
            } else if (locationsChart.data.datasets[i].label === location) {
                updateChartDataSet(locationsChart.data.datasets[i].data, locationMeasurements);
            } else {
                locationsChart.data.datasets.splice(i, 0, {
                    data: locationMeasurements,
                    label: location,
                });
            }
        }
        if (locationsChart.data.datasets.length > locationsInMeasurements.length) {
            const leftoverCount = locationsChart.data.datasets.length - locationsInMeasurements.length;
            locationsChart.data.datasets.splice(locationsInMeasurements.length, leftoverCount)
        }
    }
    if (newMeasurements.length > 0) {
        locationsChartElement.classList.remove('d-none');
    } else {
        locationsChartElement.classList.add('d-none');
    }
    locationsChart.update();
}

/**
 * @param {string | null} currentLocation
 */
async function updateCharts(currentLocation) {
    await updateCurrentLocationChart(currentLocation);
    await updateAllLocationsChart();
}
