(async () => {
    'use strict'
    
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
     * @return Promise<MeasurementStatistics>
     */
    async function fetchMeasurementStatistics() {
        let query = [];
        if (currentLocation.length > 0) {
            query.push({
                name: 'location',
                value: currentLocation
            });
        }
        const response = await fetch(buildUrl("/measurements/statistics", query));
        return await response.json();
    }
    
    async function getLocationsList() {
        const locations = await fetchLocations();
        locations.sort();

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
                    await updateLocations();
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
 
    async function getStatisticsMeasurementElements() {
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
        
        function createAverageElement(header, average) {
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

        const statistics = await fetchMeasurementStatistics();

        return {
            min: createElementForMeasurement(statistics.minTemperature, statistics.minHumidity, "min"),
            max: createElementForMeasurement(statistics.maxTemperature, statistics.maxHumidity,  "max"),
            median: createElementForMeasurement(statistics.medianTemperature, statistics.medianHumidity, "median"),
            averages: createAverageElement(statistics.averageTemperatureCelsius),
        };
    }
    
    async function updateLocations() {
        const newLocationsListContent = await getLocationsList();
        document.getElementById("locations-container").replaceContent(newLocationsListContent);
    }

    async function updateContents() {
        const newLatestContent = await getLatestMeasurementElement();
        document.getElementById("latest-container").replaceContent(newLatestContent);
        const newStatsContent = await getStatisticsMeasurementElements();
        document.getElementById("min-container").replaceContent(newStatsContent.min);
        document.getElementById("max-container").replaceContent(newStatsContent.max);
        document.getElementById("median-container").replaceContent(newStatsContent.median);
        document.getElementById("averages-container").replaceContent(newStatsContent.averages);
    }

    Node.prototype.replaceContent = function(newContent) {
        while (this.firstChild) {
            this.removeChild(this.lastChild);
        }
        this.appendChild(newContent);
    };
    Node.prototype.removeId = function() {
        this.attributes.removeNamedItem("id");
    };
    /**
     * @param {string} id
     * @param {'string'|'date'|'temperatureCelsius'|'percentage' |'temperatureFahrenheit'} valueType
     * @param {string|number|Date} value
     * @return {HTMLElement}
     */
    Node.prototype.fillElementWithId = function(id, valueType, value) {
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

    await updateLocations();
    await updateContents();
    setInterval(updateLocations, 5000);
    setInterval(updateContents, 3000);
})();
