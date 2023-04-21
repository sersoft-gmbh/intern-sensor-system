'use strict';

/**
 * @param {string[]} locations
 * @param {string | null} currentLocation
 * @return Promise<Node>
 */
async function getLocationsListElement(locations, currentLocation) {
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
        " at " + latestMeasurement.location);
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

    return content;
}

/**
 * @param {string | null} currentLocation
 */
async function getStatisticsMeasurementElements(currentLocation) {
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

    const statistics = await fetchMeasurementStatistics(currentLocation);
    return {
        min: createElementForMeasurement(statistics.minTemperature, statistics.minHumidity, "min"),
        max: createElementForMeasurement(statistics.maxTemperature, statistics.maxHumidity, "max"),
        median: createElementForMeasurement(statistics.medianTemperature, statistics.medianHumidity, "median"),
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

    const newLocationsListContent = await getLocationsListElement(locations, currentLocation);
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
