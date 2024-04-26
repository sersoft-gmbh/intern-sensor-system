import SwiftUI

struct LatestMeasurementView: View {
    var measurement: SensorMeasurement
    var showLocation: Bool

    var body: some View {
        ValueBox(style: .group, "Latest") {
            TemperatureView(measurement: measurement,
                            displayOptions: [],
                            showFeelsLike: true)
            HumidityView(measurement: measurement,
                         displayOptions: [])
            PressureView(measurement: measurement,
                         displayOptions: [])
        } footer: {
            Text(measurement.date, format: .fullDateTime)
            if showLocation {
                Text(measurement.location)
                    .transition(.scale.combined(with: .opacity))
            }
        }
        .multilineTextAlignment(.center)
        .animation(.default, value: showLocation)
    }
}

#if DEBUG
#Preview {
    LatestMeasurementView(measurement: .preview, showLocation: true)
}
#endif
