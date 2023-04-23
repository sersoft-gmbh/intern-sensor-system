import SwiftUI

struct MeasurementDetailsView: View {
    var measurement: SensorMeasurement
    var displayOptions: SensorMeasurementDisplayOptions

#if os(iOS)
    @State
    private var dateSize: CGSize?
#endif

    var body: some View {
        if displayOptions.contains(.showDate) {
#if os(iOS)
            Text(measurement.date, format: .shortDateTime)
                .background {
                    GeometryReader {
                        Color.clear
                            .onAppear { [size = $0.size] in
                                dateSize = size
                            }
                            .onChange(of: $0.size) {
                                dateSize = $0
                            }
                    }
                }
                .hidden()
#endif
            Text(measurement.date, format: .shortDateTime)
#if os(iOS)
                .frame(minHeight: dateSize.map { $0.height * 2 })
#endif
        }
        if displayOptions.contains(.showLocation) {
            Text(measurement.location)
        }
    }
}

#if DEBUG
struct MeasurementDetailsView_Previews: PreviewProvider {
    static var previews: some View {
        MeasurementDetailsView(measurement: .preview,
                               displayOptions: [.showDate, .showLocation])
    }
}
#endif
