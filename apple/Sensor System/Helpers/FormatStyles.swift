import Foundation

extension FormatStyle where Self == Date.FormatStyle {
    static var fullDateTime: Self {
        .init(date: .abbreviated, time: .standard)
    }

    static var shortDateTime: Self {
        .dateTime
        .day(.twoDigits)
        .month(.twoDigits)
        .year(.twoDigits)
        .hour(.twoDigits(amPM: .abbreviated))
        .minute(.twoDigits)
        .second(.twoDigits)
    }
}

extension FormatStyle where Self == Measurement<UnitTemperature>.FormatStyle {
    static var temperature: Self {
        measurement(width: .abbreviated,
                    usage: .weather,
                    numberFormatStyle: .number.precision(.fractionLength(0...2)))
    }
}

extension FormatStyle where Self == Measurement<UnitPressure>.FormatStyle {
    static var pressure: Self {
        measurement(width: .abbreviated,
                    usage: .barometric,
                    numberFormatStyle: .number.precision(.fractionLength(0...2)))
    }
}
