(async () => {
    'use strict'
    
    const serverAddress = ".";

    const celsiusFormatter = new Intl.NumberFormat(undefined, { 
        style: 'unit',
        unit: 'celsius',
        maximumSignificantDigits: 3
    });
    const percentageFormatter = new Intl.NumberFormat(undefined, { 
        style: 'percent',
        maximumSignificantDigits: 3
    });

    async function fetchLatestMeasurement() {
        const response = await fetch(serverAddress + "/measurements/latest");
        return await response.json();
    }

    async function fetchMinMaxMeasurement(minOrMax, valueType) {
        const response = await fetch(serverAddress + "/measurements/" + minOrMax + "-" + valueType);
        return await response.json();
    }

    async function getLatestMeasurementElement() {
        const latestMeasurement = await fetchLatestMeasurement();

        const content = document.getElementById("tmpl-latest").content.cloneNode(true);
        
        const dateElem = content.getElementById("latest-date");
        dateElem.id = null;
        dateElem.innerText = luxon.DateTime.fromISO(latestMeasurement.date).toLocaleString(luxon.DateTime.DATETIME_SHORT_WITH_SECONDS);
        
        const locationElm = content.getElementById("latest-location");
        locationElm.id = null;
        locationElm.innerText = latestMeasurement.location;

        const celsiusElem = content.getElementById("latest-temperature-celsius");
        celsiusElem.id = null;
        celsiusElem.innerText = celsiusFormatter.format(latestMeasurement.temperatureCelsius);

        const humidityElm = content.getElementById("latest-humidity-percent");
        humidityElm.id = null;
        humidityElm.innerText = percentageFormatter.format(latestMeasurement.humidityPercent);

        return content;
    }

    async function getMinMaxMeasurementElement(minOrMax) {
        const temperatureMeasurement = await fetchMinMaxMeasurement(minOrMax, "temperature");
        const humidityMeasurement = await fetchMinMaxMeasurement(minOrMax, "humidity");
       
        const content = document.getElementById("tmpl-min-max").content.cloneNode(true);

        const dateElemTemp = content.getElementById("min-max-temperature-date");
        dateElemTemp.id = null;
        dateElemTemp.innerText = luxon.DateTime.fromISO(temperatureMeasurement.date).toLocaleString(luxon.DateTime.DATETIME_SHORT_WITH_SECONDS);

        const dateElemHum = content.getElementById("min-max-humidity-date");
        dateElemHum.id = null;
        dateElemHum.innerText = luxon.DateTime.fromISO(humidityMeasurement.date).toLocaleString(luxon.DateTime.DATETIME_SHORT_WITH_SECONDS);

        const headerElement = content.getElementById("min-or-max");
        headerElement.id = null;
        headerElement.innerText = minOrMax;
        
        const celsiusElem = content.getElementById("min-max-temperature-celsius");
        celsiusElem.id = null;
        celsiusElem.innerText = celsiusFormatter.format(temperatureMeasurement.temperatureCelsius);

        const humidityElm = content.getElementById("min-max-humidity-percent");
        humidityElm.id = null;
        humidityElm.innerText = percentageFormatter.format(humidityMeasurement.humidityPercent);
        
        return content;        
    }
    
    async function updateContents() {
        function replaceContent(parent, newContent) {
            while (parent.firstChild) {
                parent.removeChild(parent.lastChild);
            }
            parent.appendChild(newContent);
        }
        
        const newLatestContent = await getLatestMeasurementElement();
        replaceContent(document.getElementById("latest-container"), newLatestContent);
        const newMinContent = await getMinMaxMeasurementElement("min");
        replaceContent(document.getElementById("min-container"), newMinContent);
        const newMaxContent = await getMinMaxMeasurementElement("max");
        replaceContent(document.getElementById("max-container"), newMaxContent);
    }

    await updateContents();
    setInterval(updateContents, 1000);
})();
