import SwiftUI

struct LocationValuesView: View {
    var locations: Array<String>
    var selectedLocationName: String?

    @State
    private var latestMeasurement: SensorMeasurement?
    @State
    private var statistics: SensorMeasurementStatistics?

    @Environment(\.network)
    private var network

    @ViewBuilder
    private var latestAndAverageViews: some View {
        if let latestMeasurement {
            LatestMeasurementView(measurement: latestMeasurement,
                                  showLocation: selectedLocationName == nil)
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
#elseif os(tvOS)
                HStack(alignment: .top, spacing: 13) {
                    statisticsViews(for: statistics)
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
#if !os(tvOS)
                .padding()
#endif
            Spacer()
#endif
        }
#if os(macOS)
        .frame(minWidth: 1050, minHeight: 530)
#endif
        .animation(.default, value: selectedLocationName)
        .animation(.default, value: latestMeasurement)
        .animation(.default, value: statistics)
        .periodicallyRefresh(frequency: .high, callInitially: false) {
            await updateContents()
        }
        .task(id: selectedLocationName) {
            await updateContents()
        }
    }

    private func statisticsViews(for statistics: SensorMeasurementStatistics) -> some View {
        ForEach(StatisticsView.StatisticsKind.allCases, id: \.self) {
            StatisticsView(statistics: statistics,
                           kind: $0,
                           showLocation: selectedLocationName == nil)
        }
    }

    private func updateContents() async {
        async let latest: Void = updateLatestMeasurement()
        async let stats: Void = updateStatistics()
        _ = await (latest, stats)
    }

    private func updateLatestMeasurement() async {
        do {
            let latest = try await network.latestMeasurement(forLocation: selectedLocationName)
            if latestMeasurement == nil {
                withAnimation(nil) {
                    latestMeasurement = latest
                }
            } else {
                latestMeasurement = latest
            }
        } catch is CancellationError {
        } catch {
            print("Failed to fetch latest measurement: \(error)")
            latestMeasurement = nil
        }
    }

    private func updateStatistics() async {
        do {
            let stats = try await network.statistics(forLocation: selectedLocationName)
            if statistics == nil {
                withAnimation(nil) {
                    statistics = stats
                }
            } else {
                statistics = stats
            }
        } catch is CancellationError {
        } catch {
            print("Failed to fetch measurement statistics: \(error)")
            statistics = nil
        }
    }
}

#if DEBUG
struct LocationValuesView_Previews: PreviewProvider {
    static var previews: some View {
        LocationValuesView(locations: SensorMeasurement.previewLocations,
                           selectedLocationName: nil)
    }
}
#endif
