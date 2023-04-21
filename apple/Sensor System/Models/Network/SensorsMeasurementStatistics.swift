import Foundation

struct SensorsMeasurementStatistics: Sendable, Hashable, Codable {
    private let averageTemperatureCelsius: Double?
//    let averageTemperatureFahrenheit: Double?
    var averageTemperature: Measurement<UnitTemperature>? {
        averageTemperatureCelsius.map { .init(value: $0, unit: .celsius) }
    }
    
    let averageHumidityPercent: Double?

    let minTemperature: SensorsMeasurement?
    let maxTemperature: SensorsMeasurement?

    let minHumidity: SensorsMeasurement?
    let maxHumidity: SensorsMeasurement?

    let medianTemperature: SensorsMeasurement?
    let medianHumidity: SensorsMeasurement?
}

#if DEBUG
extension SensorsMeasurementStatistics {
    static var preview: Self {
        .init(averageTemperatureCelsius: 21.8,
              averageHumidityPercent: 0.353,
              minTemperature: .preview,
              maxTemperature: .preview,
              minHumidity: .preview,
              maxHumidity: .preview,
              medianTemperature: .preview,
              medianHumidity: .preview)
    }
}
#endif
