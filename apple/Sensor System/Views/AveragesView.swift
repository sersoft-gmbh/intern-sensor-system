import SwiftUI

struct AveragesView: View {
    var statistics: SensorMeasurementStatistics

    var body: some View {
        if statistics.averageTemperature != nil || statistics.averageHumidityPercent != nil || statistics.averagePressure != nil {
            ValueBox(style: .group, "Average") {
                if let temperature = statistics.averageTemperature {
                    ValueBox(style: .single, "Temperature") {
                        Text(temperature, format: .temperature)
                    }
                }
                if let humidity = statistics.averageHumidityPercent {
                    ValueBox(style: .single, "Humidity") {
                        Text(humidity,
                             format: .percent.precision(.fractionLength(0...2)))
                    }
                }
                if let pressure = statistics.averagePressure {
                    ValueBox(style: .single, "Pressure") {
                        Text(pressure, format: .pressure)
                    }
                }
            }
        }
    }
}

#if DEBUG
#Preview {
    AveragesView(statistics: .preview)
}
#endif
