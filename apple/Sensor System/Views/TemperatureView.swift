import SwiftUI

fileprivate extension Double {
    static var feelsLikeDefaultSize: Self {
#if os(tvOS)
        return 24
#elseif os(macOS)
        return 14
#else
        return 16
#endif
    }
}

struct TemperatureView: View {
    var measurement: SensorMeasurement
    var displayOptions: SensorMeasurementDisplayOptions
    var showFeelsLike: Bool

    @ScaledMetric(relativeTo: .caption)
    private var feelsLikeSize = Double.feelsLikeDefaultSize

    var body: some View {
        ValueBox(style: .single, "Temperature") {
            VStack {
                Text(measurement.temperature, format: .temperature)
                if showFeelsLike {
                    Text("(Feels like \(measurement.heatIndex, format: .temperature))")
                        .font(.system(size: feelsLikeSize))
                }
            }
        } footer: {
            MeasurementDetailsView(measurement: measurement,
                                   displayOptions: displayOptions)
        }
        .multilineTextAlignment(.center)
    }
}

#if DEBUG
struct TemperaturesView_Previews: PreviewProvider {
    static var previews: some View {
        TemperatureView(measurement: .preview,
                        displayOptions: [.showDate, .showLocation],
                        showFeelsLike: true)
    }
}
#endif
