import SwiftUI

struct LocationValuesView: View {
    var locations: Array<String>

    @State
    private var selectedLocation: String?

    @State
    private var latestMeasurement: SensorsMeasurement?
    @State
    private var statistics: SensorsMeasurementStatistics?

    @Environment(\.network)
    private var network

    private var locationsPicker: some View {
        Picker("Location", selection: $selectedLocation) {
            ForEach(locations, id: \.self) {
                Text($0).tag(Optional($0))
            }
        }
    }

    @ViewBuilder
    private var latestAndAverageViews: some View {
        if let latestMeasurement {
            LatestMeasurementView(measurement: latestMeasurement)
                .transition(.move(edge: .leading).combined(with: .opacity))
        }
        if let statistics {
            AveragesView(statistics: statistics)
                .transition(.move(edge: .trailing).combined(with: .opacity))
        }
    }

    @ViewBuilder
    private var content: some View {
#if os(iOS)
        if UIDevice.current.userInterfaceIdiom == .pad {
            HStack(alignment: .top, spacing: 130) {
                latestAndAverageViews
            }
        } else {
            ViewThatFits(in: .horizontal) {
                HStack(alignment: .top) {
                    latestAndAverageViews
                }
                VStack {
                    latestAndAverageViews
                }
            }
        }
#else
        HStack(alignment: .top, spacing: 130) {
            latestAndAverageViews
        }
#endif
        Spacer()
        if let statistics {
            Group {
#if os(iOS)
                if UIDevice.current.userInterfaceIdiom == .pad {
                    HStack(alignment: .top) {
                        statisticsViews(for: statistics)
                    }
                } else {
                    VStack {
                        statisticsViews(for: statistics)
                    }
                }
#else
                HStack(alignment: .top) {
                    statisticsViews(for: statistics)
                }
#endif
            }
            .transition(.move(edge: .bottom).combined(with: .opacity))
        }
    }

    var body: some View {
        VStack {
            if locations.count > 1 {
                locationsPicker
#if os(iOS)
                    .pickerStyle(.segmented)
#endif
                    .padding()
                    .transition(.move(edge: .top).combined(with: .opacity))
            }
#if os(iOS)
            if UIDevice.current.userInterfaceIdiom == .pad {
                Spacer()
                content
                    .padding()
                Spacer()
            } else {
                ScrollView {
                    content
                        .padding()
                }
            }
#else
            Spacer()
            content
                .padding()
            Spacer()
#endif
        }
#if os(macOS)
        .frame(minWidth: 1000, minHeight: 530)
#endif
        .animation(.default, value: selectedLocation)
        .animation(.default, value: latestMeasurement)
        .animation(.default, value: statistics)
        .onChange(of: locations) {
            if selectedLocation.map($0.contains) != true {
                selectedLocation = $0.first
            }
        }
        .task(id: selectedLocation) {
            await updateContents()
        }
        .onReceive(Timer.publish(every: 5, on: .main, in: .default).autoconnect()) { _ in
            Task {
                await updateContents()
            }
        }
    }

    private func statisticsViews(for statistics: SensorsMeasurementStatistics) -> some View {
        ForEach(StatisticsView.StatisticsKind.allCases, id: \.self) {
            StatisticsView(statistics: statistics, kind: $0)
        }
    }

    private func updateContents() async {
        async let latest: Void = updateLatestMeasurement()
        async let stats: Void = updateStatistics()
        _ = await (latest, stats)
    }

    private func updateLatestMeasurement() async {
        do {
            latestMeasurement = try await network.latestMeasurement(forLocation: selectedLocation)
        } catch is CancellationError {
        } catch {
            print("Failed to fetch latest measurement: \(error)")
            latestMeasurement = nil
        }
    }

    private func updateStatistics() async {
        do {
            statistics = try await network.statistics(forLocation: selectedLocation)
        } catch is CancellationError {
        } catch {
            print("Failed to fetch latest measurement: \(error)")
            statistics = nil
        }
    }
}

#if DEBUG
struct LocationValuesView_Previews: PreviewProvider {
    static var previews: some View {
        LocationValuesView(locations: SensorsMeasurement.previewLocations)
    }
}
#endif
