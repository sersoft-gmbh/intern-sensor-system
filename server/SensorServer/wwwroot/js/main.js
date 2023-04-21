(async () => {
    'use strict'

    const serverAddress = window.location;

    const currentLocationChartElement = document.getElementById('location-temperature-chart');
    const currentLocationChart = new Chart(currentLocationChartElement, {
        type: 'line',
        data: {
            datasets: []
        },
        options: {
            responsive: true,
            parsing: {
                xAxisKey: 'date',
                yAxisKey: 'temperatureCelsius'
            },
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'minute'
                    }
                },
                y: {
                    type: 'linear'
                }
            },
            plugins: {
                legend: {
                    display: false
                }
            }
        }
    });

    const locationsChartElement = document.getElementById('locations-chart');
    const locationsChart = new Chart(locationsChartElement, {
        type: 'line',
        data: {
            datasets: []
        },
        options: {
            responsive: true,
            parsing: {
                xAxisKey: 'date',
                yAxisKey: 'temperatureCelsius'
            },
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'minute'
                    }
                },
                y: {
                    type: 'linear'
                }
            },
            plugins: {
                legend: {
                    display: true
                },
                colors: {
                    forceOverride: true
                }
            }
        }
    });

    // Taken as is from https://stackoverflow.com/a/32922084
    function deepEqual(x, y) {
        const ok = Object.keys, tx = typeof x, ty = typeof y;
        return x && y && tx === 'object' && tx === ty ? (
            ok(x).length === ok(y).length &&
            ok(x).every(key => deepEqual(x[key], y[key]))
        ) : (x === y);
    }

    function buildUrl(path, query) {
        let url = new URL(serverAddress);
        url.pathname = path;
        if (query && query.length) {
            let searchParams = new URLSearchParams();
            query.forEach(queryItem => {
                searchParams.append(queryItem.name, queryItem.value);
            })
            url.search = searchParams.toString();
        }
        return url;
    }

    const celsiusFormatter = new Intl.NumberFormat(undefined, {
        style: 'unit',
        unit: 'celsius',
        maximumSignificantDigits: 3
    });

    const fahrenheitFormatter = new Intl.NumberFormat(undefined, {
        style: 'unit',
        unit: 'fahrenheit',
        maximumSignificantDigits: 3
    });

    const percentageFormatter = new Intl.NumberFormat(undefined, {
        style: 'percent',
        maximumSignificantDigits: 3
    });

    let currentLocation = '';

    /**
     * @typedef Measurement
     * @type {object}
     * @property {number} id
     * @property {string|Date} date
     * @property {string} location
     * @property {number} temperatureCelsius
     * @property {number} temperatureFahrenheit
     * @property {number} humidityPercent
     * @property {number} heatIndexFahrenheit
     * @property {number} heatIndexCelsius
     */

    /**
     * @typedef MeasurementStatistics
     * @type {object}
     * @property {number} averageTemperatureCelsius
     * @property {number} averageHumidityPercent
     * @property {Measurement | null} minTemperature
     * @property {Measurement | null} maxTemperature
     * @property {Measurement | null} minHumidity
     * @property {Measurement | null} maxHumidity
     * @property {Measurement | null} medianTemperature
     * @property {Measurement | null} medianHumidity
     */

    /**
     * @return Promise<string[]>
     */
    async function fetchLocations() {
        const response = await fetch(buildUrl("/locations"));
        const locations = await response.json();
        if (locations.length > 0) {
            if (currentLocation.length <= 0 || !locations.includes(currentLocation)) {
                currentLocation = locations[0];
            }
        } else {
            currentLocation = '';
        }
        return locations;
    }

    /**
     * @param {'ascending' | 'descending'} sortDirection
     * @param {number | null} count
     * @param {string | null} location
     * @return Promise<Measurement[]>
     */
    async function fetchMeasurements(sortDirection= 'ascending', count = null, location = null) {
        let query = [
            {
                name: 'sortDirection',
                value: sortDirection
            }
        ];
        if (count != null) {
            query.push({
                name: 'count',
                value: count
            });
        }
        if (location && location.length > 0) {
            query.push({
                name: 'location',
                value: location
            });
        }
        const response = await fetch(buildUrl("/measurements", query));
        return await response.json();
    }

    /**
     * @return Promise<Measurement>
     */
    async function fetchLatestMeasurement() {
        let query = [];
        if (currentLocation.length > 0) {
            query.push({
                name: 'location',
                value: currentLocation
            });
        }
        const response = await fetch(buildUrl("/measurements/latest", query));
        return await response.json();
    }

    /**
     * @param {string | null} location
     * @return Promise<MeasurementStatistics>
     */
    async function fetchMeasurementStatistics(location = null) {
        let query = [];
        if (location && location.length > 0) {
            query.push({
                name: 'location',
                value: location
            });
        }
        const response = await fetch(buildUrl("/measurements/statistics", query));
        return await response.json();
    }

    /**
     * @param {string[]} locations
     * @return Promise<Node>
     */
    async function getLocationsList(locations) {
        const locationsTemplate = document.getElementById("tmpl-location-list").content.cloneNode(true);
        const locationsList = locationsTemplate.getElementById("locations-list");
        locationsList.removeId();

        const template = document.getElementById("tmpl-location-entry").content;
        locations.forEach(location => {
            const currentLocationContent = template.cloneNode(true);
            const currentLocationElement = currentLocationContent.fillElementWithId(
                "location-name",
                'string',
                location);

            if (location === currentLocation) {
                currentLocationElement.classList.add("active");
            } else {
                currentLocationElement.addEventListener('click', async () => {
                    currentLocation = location;
                    await updateContents();
                })
            }
            locationsList.appendChild(currentLocationContent);
        });

        return locationsTemplate;
    }

    async function getLatestMeasurementElement() {
        const latestMeasurement = await fetchLatestMeasurement();

        const content = document.getElementById("tmpl-latest").content.cloneNode(true);

        content.fillElementWithId(
            'latest-date',
            'date',
            latestMeasurement.date);

        content.fillElementWithId(
            'latest-heatIndex-Celsius',
            'temperatureCelsius',
            latestMeasurement.heatIndexCelsius);

        const locationElm = content.fillElementWithId(
            'latest-location',
            'string',
            " at " + latestMeasurement.location);
        if (currentLocation.length >= 0) {
            locationElm.parentElement.removeChild(locationElm);
        }

        content.fillElementWithId(
            'latest-temperature-celsius',
            'temperatureCelsius',
            latestMeasurement.temperatureCelsius);

        content.fillElementWithId(
            'latest-humidity-percent',
            'percentage',
            latestMeasurement.humidityPercent);

        content.fillElementWithId(
            'latest-temperature-fahrenheit',
            'temperatureFahrenheit',
            latestMeasurement.temperatureFahrenheit);

        return content;
    }

    /**
     * @param {MeasurementStatistics} statistics
     */
    async function getStatisticsMeasurementElements(statistics) {
        /**
         * @param {Measurement | null} temperatureMeasurement
         * @param {Measurement | null} humidityMeasurement
         * @param {string} header
         * @return {Node}
         */
        function createElementForMeasurement(temperatureMeasurement, humidityMeasurement, header) {
            const content = document.getElementById("tmpl-statistics").content.cloneNode(true);

            content.fillElementWithId(
                'stats-temperature-date',
                'date',
                temperatureMeasurement?.date);

            content.fillElementWithId(
                'stats-humidity-date',
                'date',
                humidityMeasurement?.date);

            content.fillElementWithId(
                'stats-header',
                'string',
                header);

            content.fillElementWithId(
                'stats-temperature-celsius',
                'temperatureCelsius',
                temperatureMeasurement?.temperatureCelsius);

            content.fillElementWithId(
                'stats-humidity-percent',
                'percentage',
                humidityMeasurement?.humidityPercent);

            content.fillElementWithId(
                'stats-heatIndex-celsius',
                'temperatureCelsius',
                temperatureMeasurement?.heatIndexCelsius);

            return content;
        }

        function createAverageElement() {
            const content = document.getElementById("tmpl-average-statistics").content.cloneNode(true);
            content.fillElementWithId(
                'avg-stats-temperature',
                'temperatureCelsius',
                statistics.averageTemperatureCelsius);
            content.fillElementWithId(
                'avg-stats-humidity',
                'percentage',
                statistics.averageHumidityPercent);
            return content;
        }

        return {
            min: createElementForMeasurement(statistics.minTemperature, statistics.minHumidity, "min"),
            max: createElementForMeasurement(statistics.maxTemperature, statistics.maxHumidity, "max"),
            median: createElementForMeasurement(statistics.medianTemperature, statistics.medianHumidity, "median"),
            averages: createAverageElement(),
        };
    }

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

    async function updateContents() {
        const locations = await fetchLocations();
        locations.sort();

        const newLocationsListContent = await getLocationsList(locations);
        document.getElementById("locations-container").replaceContent(newLocationsListContent);

        const newLatestContent = await getLatestMeasurementElement();
        document.getElementById("latest-container").replaceContent(newLatestContent);

        const statistics = await fetchMeasurementStatistics(currentLocation);
        const newStatsContent = await getStatisticsMeasurementElements(statistics);
        document.getElementById("min-container").replaceContent(newStatsContent.min);
        document.getElementById("max-container").replaceContent(newStatsContent.max);
        document.getElementById("median-container").replaceContent(newStatsContent.median);
        document.getElementById("averages-container").replaceContent(newStatsContent.averages);

        const chartMeasurementsCount = 125;
        const chartMinMaxPaddingCelsius = 0.75;
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

    Node.prototype.replaceContent = function (newContent) {
        while (this.firstChild) {
            this.removeChild(this.lastChild);
        }
        this.appendChild(newContent);
    };
    Node.prototype.removeId = function () {
        this.attributes.removeNamedItem("id");
    };
    /**
     * @param {string} id
     * @param {'string'|'date'|'temperatureCelsius'|'percentage' |'temperatureFahrenheit'} valueType
     * @param {string|number|Date} value
     * @return {HTMLElement}
     */
    Node.prototype.fillElementWithId = function (id, valueType, value) {
        const element = this.getElementById(id);
        element.removeId();
        switch (valueType) {
            case 'string':
                element.innerText = value;
                break;
            case 'date':
                element.innerText = luxon.DateTime.fromISO(value).toLocaleString(luxon.DateTime.DATETIME_SHORT_WITH_SECONDS);
                break;
            case 'temperatureCelsius':
                element.innerText = celsiusFormatter.format(value);
                break;
            case 'temperatureFahrenheit':
                element.innerText = fahrenheitFormatter.format(value);
                break;
            case 'percentage':
                element.innerText = percentageFormatter.format(value);
                break;
            default:
                console.error('Unsupported value type', valueType);
                break;
        }
        return element;
    }

    await updateContents();
    setInterval(updateContents, 3000);
})();
