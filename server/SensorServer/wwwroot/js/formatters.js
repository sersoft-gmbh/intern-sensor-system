const celsiusFormatter = new Intl.NumberFormat(undefined, {
    style: 'unit',
    unit: 'celsius',
    maximumSignificantDigits: 3,
});

const fahrenheitFormatter = new Intl.NumberFormat(undefined, {
    style: 'unit',
    unit: 'fahrenheit',
    maximumSignificantDigits: 3,
});

const percentageFormatter = new Intl.NumberFormat(undefined, {
    style: 'percent',
    maximumSignificantDigits: 3,
});

const hectopascalsFormatter = new Intl.NumberFormat(undefined, {
    // hectopascals are currently not yet supported by Intl.NumberFormat
    style: 'decimal',
    //style: 'unit',
    //unit: 'hectopascal',
    maximumSignificantDigits: 3,
});
