import SwiftUI

struct AveragesView: View {
    var statistics: SensorsMeasurementStatistics

    var body: some View {
        ValuesGroupView("Average") {
            HStack(alignment: .top) {
                ValueView("Temperature") {
                    Text(statistics.averageTemperature, format: .temperature)
                }
                ValueView("Humidity") {
                    Text(statistics.averageHumidityPercent,
                         format: .percent.precision(.fractionLength(0...2)))
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
