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
