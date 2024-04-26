import SwiftUI

struct PressureView: View {
    var measurement: SensorMeasurement
    var displayOptions: SensorMeasurementDisplayOptions

    var body: some View {
        if let pressure = measurement.pressure {
            ValueBox(style: .single, "Pressure") {
                Text(pressure, format: .pressure)
            } footer: {
                MeasurementDetailsView(measurement: measurement,
                                       displayOptions: displayOptions)
            }
            .multilineTextAlignment(.center)
        }
    }
}

#if DEBUG
#Preview {
    PressureView(measurement: .preview,
                 displayOptions: [.showDate, .showLocation])
}
#endif
