import SwiftUI

struct LatestMeasurementView: View {
    var measurement: SensorsMeasurement

    var body: some View {
        ValuesGroupView("Latest", date: measurement.date) {
            TemperaturesView(measurement: measurement,
                             showDate: false,
                             showFeelsLike: true)
            HumidityView(measurement: measurement,
                         showDate: false)
        }
        .multilineTextAlignment(.center)
    }
}

#if DEBUG
struct LatestMeasurementView_Previews: PreviewProvider {
    static var previews: some View {
        LatestMeasurementView(measurement: .preview)
    }
}
#endif
