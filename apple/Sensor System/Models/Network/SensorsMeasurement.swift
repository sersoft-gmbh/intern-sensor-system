import Foundation

struct SensorsMeasurement: Sendable, Hashable, Codable, Identifiable {
    let id: Int
    let date: Date
    let location: String

    private let temperatureCelsius: Double
//    let temperatureFahrenheit: Double

    let humidityPercent: Double

    private let heatIndexCelsius: Double
//    let heatIndexFahrenheit: Double

    var temperature: Measurement<UnitTemperature> {
        .init(value: temperatureCelsius, unit: .celsius)
    }

    var heatIndex: Measurement<UnitTemperature> {
        .init(value: heatIndexCelsius, unit: .celsius)
    }
}

#if DEBUG
extension SensorsMeasurement {
    static var preview: Self {
        .init(id: 1,
              date: .now,
              location: "Preview Location",
              temperatureCelsius: 21.5,
              humidityPercent: 0.385,
              heatIndexCelsius: 20.8)
    }

    static var previewLocations: Array<String> {
        [
            "Preview Location",
            "Another Location",
        ]
    }
}
#endif
