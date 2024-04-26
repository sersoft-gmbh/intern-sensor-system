import SwiftUI

struct HumidityView: View {
    var measurement: SensorMeasurement
    var displayOptions: SensorMeasurementDisplayOptions

    var body: some View {
        ValueBox(style: .single, "Humidity") {
            Text(measurement.humidityPercent,
                 format: .percent.precision(.fractionLength(0...2)))
        } footer: {
            MeasurementDetailsView(measurement: measurement,
                                   displayOptions: displayOptions)
        }
        .multilineTextAlignment(.center)
    }
}

#if DEBUG
#Preview {
    HumidityView(measurement: .preview,
                 displayOptions: [.showDate, .showLocation])
}
#endif
