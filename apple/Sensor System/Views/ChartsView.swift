import SwiftUI
import Charts

struct ChartsView: View {
    var locations: Array<String>
    
    @State
    private var measurements = Array<SensorsMeasurement>()

    @State
    private var selectedLocation: String?

    @Environment(\.network)
    private var network

    private var vStackSpacing: CGFloat? {
        #if os(tvOS)
        return 50
        #else
        return nil
        #endif
    }

    var body: some View {
        VStack(spacing: vStackSpacing) {
            Picker("Location", selection: $selectedLocation) {
                Text("All").tag(Optional<String>.none)
                ForEach(locations, id: \.self) {
                    Text($0).tag(Optional($0))
                }
            }
#if os(iOS)
            .pickerStyle(.segmented)
#endif
            Chart(measurements) { measurement in
                LineMark(x: .value("Date & Time", measurement.date),
                         y: .value("Temperature", measurement.temperature.value),
                         series: .value("Location", measurement.location))
                .foregroundStyle(by: .value("Location", measurement.location))
                PointMark(x: .value("Date & Time", measurement.date),
                          y: .value("Temperature", measurement.temperature.value))
                .foregroundStyle(by: .value("Location", measurement.location))
            }
            .chartForegroundStyleScale(domain: locations)
        }
        .padding()
#if os(macOS)
        .frame(minWidth: 1000, minHeight: 350)
#endif
        .animation(.default, value: measurements)
        .animation(.default, value: selectedLocation)
        .task {
            guard measurements.isEmpty else { return }
            await loadMeasurements()
        }
        .task(id: selectedLocation) {
            await loadMeasurements()
        }
        .onChange(of: locations) {
            if selectedLocation.map($0.contains) != true {
                selectedLocation = nil
            }
        }
        .onReceive(Timer.publish(every: 5, on: .main, in: .default).autoconnect()) { _ in
            Task {
                await loadMeasurements()
            }
        }
    }

    private func loadMeasurements() async {
        let count: Int
        #if os(iOS)
        count = await UIDevice.current.userInterfaceIdiom == .phone ? 50 : 100
        #else
        count = 125
        #endif
        do {
            measurements = try await network.measurements(count: count, location: selectedLocation)
                .sorted(using: [KeyPathComparator(\.location), KeyPathComparator(\.date)])
        } catch is CancellationError {
        } catch {
            print("Failed to fetch measurements: \(error)")
            measurements.removeAll()
        }
    }
}

#if DEBUG
struct ChartsView_Previews: PreviewProvider {
    static var previews: some View {
        ChartsView(locations: SensorsMeasurement.previewLocations)
    }
}
#endif
