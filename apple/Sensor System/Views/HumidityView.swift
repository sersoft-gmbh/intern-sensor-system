import SwiftUI

struct HumidityView: View {
    var measurement: SensorsMeasurement
    var showDate: Bool

    var body: some View {
        ValueView("Humidity", date: showDate ? measurement.date : nil) {
            Text(measurement.humidityPercent,
                 format: .percent.precision(.fractionLength(0...2)))
        }
    }
}

#if DEBUG
struct HumidityView_Previews: PreviewProvider {
    static var previews: some View {
        HumidityView(measurement: .preview,
                     showDate: true)
    }
}
#endif
