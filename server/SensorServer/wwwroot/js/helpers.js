'use strict'

// Taken as is from https://stackoverflow.com/a/32922084
function deepEqual(x, y) {
    const ok = Object.keys, tx = typeof x, ty = typeof y;
    return x && y && tx === 'object' && tx === ty ? (
        ok(x).length === ok(y).length &&
        ok(x).every(key => deepEqual(x[key], y[key]))
    ) : (x === y);
}

(() => {
    /**
     * @param newContent {Element}
     */
    Node.prototype.replaceContent = function (newContent) {
        while (this.firstChild) {
            this.removeChild(this.lastChild);
        }
        this.appendChild(newContent);
    };
    Element.prototype.removeId = function () {
        this.attributes.removeNamedItem("id");
    };

    /**
     * @param {string} id
     * @param {'string'|'date'|'temperatureCelsius'|'percentage'|'temperatureFahrenheit'|'hectopascals'} valueType
     * @param {string|number|Date|null|undefined} value
     * @param {boolean} removeIfNoValue
     * @return {HTMLElement}
     */
    Element.prototype.fillElementWithId = function (
        id, 
        valueType, 
        value, 
        removeIfNoValue = true) {
        const element = this.querySelector('#' + id);
        if (!value && removeIfNoValue) {
            element.remove();
            return element;
        }
        
        element.removeId();
        switch (valueType) {
            case 'string':
                element.innerText = value;
                break;
            case 'date':
                element.innerText = value ? luxon.DateTime.fromISO(value).toLocaleString(luxon.DateTime.DATETIME_SHORT_WITH_SECONDS) : 'N/A';
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
            case 'hectopascals':
                element.innerText = value ? hectopascalsFormatter.format(value) + ' hP' : 'N/A';
                break;
            default:
                console.error('Unsupported value type', valueType);
                break;
        }
        return element;
    }
})();
