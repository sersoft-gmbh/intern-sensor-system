'use strict';

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

const serverAddress = window.location;

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

/**
 * @return Promise<string[]>
 */
async function fetchLocations() {
    const response = await fetch(buildUrl("/locations"));
    return await response.json();
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
 * @param {string | null} location
 * @return Promise<Measurement>
 */
async function fetchLatestMeasurement(location = null) {
    let query = [];
    if (location && location.length > 0) {
        query.push({
            name: 'location',
            value: location
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
