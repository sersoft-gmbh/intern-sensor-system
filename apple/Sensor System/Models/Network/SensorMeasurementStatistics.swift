import Foundation

struct SensorMeasurementStatistics: Sendable, Hashable, Codable {
    private let averageTemperatureCelsius: Double?
//    let averageTemperatureFahrenheit: Double?
    var averageTemperature: Measurement<UnitTemperature>? {
        averageTemperatureCelsius.map { .init(value: $0, unit: .celsius) }
    }
    
    let averageHumidityPercent: Double?

    private let averagePressureHectopascals: Double?
    var averagePressure: Measurement<UnitPressure>? {
        averagePressureHectopascals.map { .init(value: $0, unit: .hectopascals) }
    }

    let minTemperature: SensorMeasurement?
    let maxTemperature: SensorMeasurement?

    let minHumidity: SensorMeasurement?
    let maxHumidity: SensorMeasurement?

    let minPressure: SensorMeasurement?
    let maxPressure: SensorMeasurement?

    let medianTemperature: SensorMeasurement?
    let medianHumidity: SensorMeasurement?
    let medianPressure: SensorMeasurement?
}

#if DEBUG
extension SensorMeasurementStatistics {
    static var preview: Self {
        .init(averageTemperatureCelsius: 21.8,
              averageHumidityPercent: 0.353,
              averagePressureHectopascals: 1008.75,
              minTemperature: .preview,
              maxTemperature: .preview,
              minHumidity: .preview,
              maxHumidity: .preview,
              minPressure: .preview,
              maxPressure: .preview,
              medianTemperature: .preview,
              medianHumidity: .preview,
              medianPressure: .preview)
    }
}
#endif
