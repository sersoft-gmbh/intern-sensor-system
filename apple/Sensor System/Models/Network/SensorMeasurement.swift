import Foundation

struct SensorMeasurement: Sendable, Hashable, Codable, Identifiable {
    let id: Int
    let date: Date
    let location: String

    private let temperatureCelsius: Double
//    let temperatureFahrenheit: Double
    let humidityPercent: Double

    private let pressureHectopascals: Double?

    private let heatIndexCelsius: Double
//    let heatIndexFahrenheit: Double

    var temperature: Measurement<UnitTemperature> {
        .init(value: temperatureCelsius, unit: .celsius)
    }

    var heatIndex: Measurement<UnitTemperature> {
        .init(value: heatIndexCelsius, unit: .celsius)
    }

    var pressure: Measurement<UnitPressure>? {
        pressureHectopascals.map {
            .init(value: $0, unit: .hectopascals)
        }
    }
}

#if DEBUG
extension SensorMeasurement {
    static var preview: Self {
        .init(id: 1,
              date: .now,
              location: "Preview Location",
              temperatureCelsius: 21.5,
              humidityPercent: 0.385,
              pressureHectopascals: 1010.3,
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
