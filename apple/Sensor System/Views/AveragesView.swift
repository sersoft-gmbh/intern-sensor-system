import SwiftUI

struct AveragesView: View {
    var statistics: SensorsMeasurementStatistics

    var body: some View {
        if statistics.averageTemperature != nil || statistics.averageHumidityPercent != nil {
            ValuesGroupView("Average") {
                HStack(alignment: .top) {
                    if let temperature = statistics.averageTemperature {
                        ValueView("Temperature") {
                            Text(temperature, format: .temperature)
                        }
                    }
                    if let humidity = statistics.averageHumidityPercent {
                        ValueView("Humidity") {
                            Text(humidity,
                                 format: .percent.precision(.fractionLength(0...2)))
                        }
                    }
                }
            }
        }
    }
}

#if DEBUG
struct AveragesView_Previews: PreviewProvider {
    static var previews: some View {
        AveragesView(statistics: .preview)
    }
}
#endif
