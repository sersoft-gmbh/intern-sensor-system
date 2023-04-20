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
    const percentageFormatter = new Intl.NumberFormat(undefined, { 
        style: 'percent',
        maximumSignificantDigits: 3
    });
    
    let currentLocation = '';

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
            const currentLocationElement = currentLocationContent.getElementById("location-name");
            currentLocationElement.removeId();
            currentLocationElement.innerText = location;
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
        
        const dateElem = content.getElementById("latest-date");
        dateElem.removeId();
        dateElem.innerText = luxon.DateTime.fromISO(latestMeasurement.date).toLocaleString(luxon.DateTime.DATETIME_SHORT_WITH_SECONDS);
        
        const locationElm = content.getElementById("latest-location");
        if (currentLocation.length >= 0) {
            locationElm.parentElement.removeChild(locationElm);
        } else {
            locationElm.removeId();
            locationElm.innerText = " at " + latestMeasurement.location;
        }

        const celsiusElem = content.getElementById("latest-temperature-celsius");
        celsiusElem.removeId();
        celsiusElem.innerText = celsiusFormatter.format(latestMeasurement.temperatureCelsius);

        const humidityElm = content.getElementById("latest-humidity-percent");
        humidityElm.removeId();
        humidityElm.innerText = percentageFormatter.format(latestMeasurement.humidityPercent);

        return content;
    }

    async function getMinMaxMeasurementElements() {
        function createElementForMeasurement(temperatureMeasurement, humidityMeasurement, header) {
            const content = document.getElementById("tmpl-min-max").content.cloneNode(true);

            const dateElemTemp = content.getElementById('min-max-temperature-date');
            dateElemTemp.removeId();
            dateElemTemp.innerText = luxon.DateTime.fromISO(temperatureMeasurement.date).toLocaleString(luxon.DateTime.DATETIME_SHORT_WITH_SECONDS);

            const dateElemHum = content.getElementById('min-max-humidity-date');
            dateElemHum.removeId();
            dateElemHum.innerText = luxon.DateTime.fromISO(humidityMeasurement.date).toLocaleString(luxon.DateTime.DATETIME_SHORT_WITH_SECONDS);

            const headerElement = content.getElementById("min-max-header");
            headerElement.removeId();
            headerElement.innerText = header;
            
            const celsiusElem = content.getElementById('min-max-temperature-celsius');
            celsiusElem.removeId();
            celsiusElem.innerText = celsiusFormatter.format(temperatureMeasurement.temperatureCelsius);

            const humidityElm = content.getElementById('min-max-humidity-percent');
            humidityElm.removeId();
            humidityElm.innerText = percentageFormatter.format(humidityMeasurement.humidityPercent);

            return content;
        }

        const statistics = await fetchMeasurementStatistics();

        return {
            min: createElementForMeasurement(statistics.minTemperature, statistics.minHumidity, "min"),
            max: createElementForMeasurement(statistics.maxTemperature, statistics.maxHumidity, "max"),
        };
    }
    
    async function updateLocations() {
        const newLocationsListContent = await getLocationsList();
        document.getElementById("locations-container").replaceContent(newLocationsListContent);
    }

    async function updateContents() {
        const newLatestContent = await getLatestMeasurementElement();
        document.getElementById("latest-container").replaceContent(newLatestContent);
        const newMinMaxContent = await getMinMaxMeasurementElements();
        document.getElementById("min-container").replaceContent(newMinMaxContent.min);
        document.getElementById("max-container").replaceContent(newMinMaxContent.max);
    }

    Element.prototype.replaceContent = function(newContent) {
        while (this.firstChild) {
            this.removeChild(this.lastChild);
        }
        this.appendChild(newContent);
    };
    Node.prototype.removeId = function() {
        this.attributes.removeNamedItem("id");
    };

    await updateLocations();
    await updateContents();
    setInterval(updateLocations, 5000);
    setInterval(updateContents, 3000);
})();
