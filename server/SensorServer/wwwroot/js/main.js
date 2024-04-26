'use strict';

/**
 * @param {string[]} locations
 * @param {string | null} currentLocation
 * @param {function} updateLocation
 * @return Promise<Node>
 */
async function getLocationsListElement(locations, currentLocation, updateLocation) {
    const locationsTemplate = document.getElementById("tmpl-location-list").content.cloneNode(true);
    const locationsList = locationsTemplate.getElementById("locations-list");
    locationsList.removeId();

    const template = document.getElementById("tmpl-location-entry").content;
    locations.forEach(location => {
        const currentLocationContent = template.cloneNode(true);
        const currentLocationElement = currentLocationContent.fillElementWithId(
            "location-name",
            'string',
            location,
            false);

        if (location === currentLocation) {
            currentLocationElement.classList.add("active");
        } else {
            currentLocationElement.addEventListener('click', async () => {
                updateLocation(location);
                await updateContents();
            })
        }
        locationsList.appendChild(currentLocationContent);
    });

    return locationsTemplate;
}

async function getLatestMeasurementElement(currentLocation) {
    const latestMeasurement = await fetchLatestMeasurement(currentLocation);

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
        " at " + latestMeasurement.location,
        false);
    if (currentLocation && currentLocation.length >= 0) {
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

    content.fillElementWithId(
        'latest-pressure-hectopascals',
        'hectopascals',
        latestMeasurement.pressureHectopascals);

    return content;
}

/**
 * @param {string | null} currentLocation
 */
async function getStatisticsMeasurementElements(currentLocation) {
    /**
     * @param {Measurement | null} temperatureMeasurement
     * @param {Measurement | null} humidityMeasurement
     * @param {Measurement | null} pressureMeasurement
     * @param {string} header
     * @return {Node}
     */
    function createElementForMeasurement(temperatureMeasurement, humidityMeasurement, pressureMeasurement, header) {
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
            'stats-pressure-date',
            'date',
            pressureMeasurement?.date);

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

        content.fillElementWithId(
            'stats-pressure-hectopascals',
            'hectopascals',
            pressureMeasurement?.pressureHectopascals);

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
        content.fillElementWithId(
            'avg-stats-pressure',
            'hectopascals',
            statistics.averagePressureHectopascals);
        return content;
    }

    const statistics = await fetchMeasurementStatistics(currentLocation);
    return {
        min: createElementForMeasurement(statistics.minTemperature, statistics.minHumidity, statistics.minPressure, "min"),
        max: createElementForMeasurement(statistics.maxTemperature, statistics.maxHumidity, statistics.maxPressure, "max"),
        median: createElementForMeasurement(statistics.medianTemperature, statistics.medianHumidity, statistics.medianPressure, "median"),
        averages: createAverageElement(),
    };
}

let currentLocation = '';
async function updateContents() {
    const locations = await fetchLocations();
    locations.sort();
    if (locations.length > 0) {
        if (currentLocation.length <= 0 || !locations.includes(currentLocation)) {
            currentLocation = locations[0];
        }
    } else {
        currentLocation = '';
    }

    const newLocationsListContent = await getLocationsListElement(locations, currentLocation, location => {
        currentLocation = location;
    });
    document.getElementById("locations-container").replaceContent(newLocationsListContent);

    const newLatestContent = await getLatestMeasurementElement(currentLocation);
    document.getElementById("latest-container").replaceContent(newLatestContent);

    const newStatsContent = await getStatisticsMeasurementElements(currentLocation);
    document.getElementById("min-container").replaceContent(newStatsContent.min);
    document.getElementById("max-container").replaceContent(newStatsContent.max);
    document.getElementById("median-container").replaceContent(newStatsContent.median);
    document.getElementById("averages-container").replaceContent(newStatsContent.averages);

    await updateCharts(currentLocation);
}

(async () => {
    await updateContents();
    setInterval(updateContents, 3000);
})();
