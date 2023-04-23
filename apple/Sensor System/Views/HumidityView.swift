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
struct HumidityView_Previews: PreviewProvider {
    static var previews: some View {
        HumidityView(measurement: .preview,
                     displayOptions: [.showDate, .showLocation])
    }
}
#endif
