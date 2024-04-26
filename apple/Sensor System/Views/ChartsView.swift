import SwiftUI
import Charts

fileprivate extension FormatStyle where Self == PlottableMeasurement<UnitTemperature>.FormatStyle {
    static var temperature: Self { .init(formatStyle: .temperature) }
}

fileprivate extension FormatStyle where Self == PlottableMeasurement<UnitPressure>.FormatStyle {
    static var pressure: Self { .init(formatStyle: .pressure) }
}

struct ChartsView: View {
    private struct MeasurementChart<Value: Plottable, Format: FormatStyle>: View
    where Format.FormatInput == Value, Format.FormatOutput == String
    {
        var measurements: Array<SensorMeasurement>

        var title: Text
        var valuePath: KeyPath<SensorMeasurement, Value>
        var format: Format

        var body: some View {
            title.font(.title3)
            Chart(measurements) { measurement in
                LineMark(x: .value("Date & Time", measurement.date),
                         y: .value("Temperature", measurement[keyPath: valuePath]),
                         series: .value("Location", measurement.location))
                .foregroundStyle(by: .value("Location", measurement.location))
                PointMark(x: .value("Date & Time", measurement.date),
                          y: .value("Temperature", measurement[keyPath: valuePath]))
                .foregroundStyle(by: .value("Location", measurement.location))
            }
            .chartYAxis {
                AxisMarks(format: format)
            }
            .frame(minHeight: 300)
        }

        init(measurements: Array<SensorMeasurement>,
             title: Text,
             valuePath: KeyPath<SensorMeasurement, Value>,
             format: Format,
             yRange: (minValue: Value, maxValue: Value)? = nil) {
            self.measurements = measurements
            self.title = title
            self.valuePath = valuePath
            self.format = format
        }

        init(measurements: Array<SensorMeasurement>,
             title: Text,
             valuePath: KeyPath<SensorMeasurement, Optional<Value>>,
             format: Format) {
            self.init(measurements: measurements.filter { $0[keyPath: valuePath] != nil },
                      title: title,
                      valuePath: valuePath.appending(path: \.self!),
                      format: format)
        }
    }

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

    private var temperatureChart: some View {
        MeasurementChart(measurements: measurements,
                         title: Text("Temperature"),
                         valuePath: \.temperature.plottable,
                         format: .temperature)
        .chartYScale(domain: PlottableMeasurement<UnitTemperature>(value: -30, unit: .celsius)...PlottableMeasurement(value: 60, unit: .celsius))
    }

    private var humidityChart: some View {
        MeasurementChart(measurements: measurements,
                         title: Text("Humidity"),
                         valuePath: \.humidityPercent,
                         format: .percent)
        .chartYScale(domain: 0...1)
    }

    private var pressureChart: some View {
        MeasurementChart(measurements: measurements,
                         title: Text("Pressure"),
                         valuePath: \.pressure?.plottable,
                         format: .pressure)
        .chartYScale(domain: PlottableMeasurement<UnitPressure>(value: 400, unit: .hectopascals)...PlottableMeasurement(value: 1200, unit: .hectopascals))
    }

    var body: some View {
        ScrollView {
#if !os(tvOS)
            LazyVStack(spacing: vStackSpacing) {
                temperatureChart
                humidityChart
                pressureChart
            }
            .chartForegroundStyleScale(domain: locations)
            .padding()
#if os(macOS)
            .frame(minWidth: 1000, minHeight: 350)
#endif
#else
            HStack {
                VStack {
                    temperatureChart
                }
            }
            HStack {
                VStack {
                    humidityChart
                }
                VStack {
                    pressureChart
                }
            }
            .chartForegroundStyleScale(domain: locations)
#endif
        }
        .animation(.default, value: measurements)
        .animation(.default, value: selectedLocationName)
        .periodicallyRefresh(frequency: .high, callInitially: false) {
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
#Preview {
    ChartsView(locations: SensorMeasurement.previewLocations,
               selectedLocationName: nil)
}
#endif
