import SwiftUI
import Charts

private struct _TemperatureCelsius: Plottable {
    struct _FormatStyle: FormatStyle {
        let formatStyle: Measurement<UnitTemperature>.FormatStyle

        init(formatStyle: Measurement<UnitTemperature>.FormatStyle) {
            self.formatStyle = formatStyle
        }

        init(from decoder: Decoder) throws {
            formatStyle = try .init(from: decoder)
        }

        func encode(to encoder: Encoder) throws {
            try formatStyle.encode(to: encoder)
        }

        func locale(_ locale: Locale) -> Self {
            .init(formatStyle: formatStyle.locale(locale))
        }

        func format(_ value: _TemperatureCelsius) -> String {
            formatStyle.format(value.measurement)
        }
    }

    private let value: Double

    var measurement: Measurement<UnitTemperature> {
        .init(value: value, unit: .celsius)
    }

    var primitivePlottable: Double { value }

    init(measurement: Measurement<UnitTemperature>) {
        self.value = measurement.converted(to: .celsius).value
    }

    init?(primitivePlottable: Double) {
        value = primitivePlottable
    }
}

fileprivate extension Measurement<UnitTemperature> {
    var celsius: _TemperatureCelsius { .init(measurement: self) }
}

fileprivate extension FormatStyle where Self == _TemperatureCelsius._FormatStyle {
    static var temperatureCelsius: Self { .init(formatStyle: .temperature) }
}

struct ChartsView: View {
    var locations: Array<String>
    var selectedLocationName: String?
    
    @State
    private var measurements = Array<SensorMeasurement>()

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
            VStack {
                Text("Temperature")
                    .font(.title3)
                Chart(measurements) { measurement in
                    LineMark(x: .value("Date & Time", measurement.date),
                             y: .value("Temperature", measurement.temperature.celsius),
                             series: .value("Location", measurement.location))
                    .foregroundStyle(by: .value("Location", measurement.location))
                    PointMark(x: .value("Date & Time", measurement.date),
                              y: .value("Temperature", measurement.temperature.celsius))
                    .foregroundStyle(by: .value("Location", measurement.location))
                }
                .chartYAxis {
                    AxisMarks(format: _TemperatureCelsius._FormatStyle.temperatureCelsius)
                }
            }
            VStack {
                Text("Humidity")
                    .font(.title3)
                Chart(measurements) { measurement in
                    LineMark(x: .value("Date & Time", measurement.date),
                             y: .value("Humidity", measurement.humidityPercent),
                             series: .value("Location", measurement.location))
                    .foregroundStyle(by: .value("Location", measurement.location))
                    PointMark(x: .value("Date & Time", measurement.date),
                              y: .value("Humidity", measurement.humidityPercent))
                    .foregroundStyle(by: .value("Location", measurement.location))
                }
                .chartYAxis {
                    AxisMarks(format: Decimal.FormatStyle.Percent.percent)
                }
            }
        }
        .chartForegroundStyleScale(domain: locations)
        .padding()
#if os(macOS)
        .frame(minWidth: 1000, minHeight: 350)
#endif
        .animation(.default, value: measurements)
        .animation(.default, value: selectedLocationName)
        .periodicallyRefresh {
            await loadMeasurements()
        }
        .task(id: selectedLocationName) {
            await loadMeasurements()
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
            measurements = try await network.measurements(count: count, location: selectedLocationName)
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
        ChartsView(locations: SensorMeasurement.previewLocations,
                   selectedLocationName: nil)
    }
}
#endif
