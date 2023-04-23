import Foundation

struct SensorMeasurementStatistics: Sendable, Hashable, Codable {
    private let averageTemperatureCelsius: Double?
//    let averageTemperatureFahrenheit: Double?
    var averageTemperature: Measurement<UnitTemperature>? {
        averageTemperatureCelsius.map { .init(value: $0, unit: .celsius) }
    }
    
    let averageHumidityPercent: Double?

    let minTemperature: SensorMeasurement?
    let maxTemperature: SensorMeasurement?

    let minHumidity: SensorMeasurement?
    let maxHumidity: SensorMeasurement?

    let medianTemperature: SensorMeasurement?
    let medianHumidity: SensorMeasurement?
}

#if DEBUG
extension SensorMeasurementStatistics {
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
