import SwiftUI

struct TemperaturesView: View {
    var measurement: SensorsMeasurement
    var showDate: Bool
    var showFeelsLike: Bool

    @ScaledMetric(relativeTo: .caption)
    private var feelsLikeSize = Double.feelsLikeDefaultSize

    var body: some View {
        ValueView("Temperature", date: showDate ? measurement.date : nil) {
            VStack {
                Text(measurement.temperature, format: .temperature)
                if showFeelsLike {
                    Text("(Feels like \(measurement.heatIndex, format: .temperature))")
                        .font(.system(size: feelsLikeSize))
                }
            }
        }
        .multilineTextAlignment(.center)
    }
}
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

#if DEBUG
struct TemperaturesView_Previews: PreviewProvider {
    static var previews: some View {
        TemperaturesView(measurement: .preview,
                         showDate: true,
                         showFeelsLike: true)
    }
}
#endif
