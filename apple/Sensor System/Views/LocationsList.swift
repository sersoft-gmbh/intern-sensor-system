import SwiftUI
import Charts

fileprivate extension ScaleType {
    static var counts: Self {
        if #available(iOS 16.4, tvOS 16.4, macOS 13.3, *) {
            return .symmetricLog
        } else {
            return .linear
        }
    }
}

struct LocationsList: View {
    @Binding
    var locations: Array<Location>
#if os(macOS)
    @Binding
    var selectedLocation: Location
#else
    @Binding
    var selectedLocation: Location?
#endif

#if os(tvOS)
    @FocusState
    private var isFocused: Bool
    @FocusState
    private var focusedLocation: Location?
    @Environment(\.colorScheme)
    private var colorScheme
#endif

#if os(iOS)
    @Environment(\.verticalSizeClass)
    private var verticalSizeClass
#endif

    private var showChart: Bool {
#if os(iOS)
        guard UIDevice.current.userInterfaceIdiom == .phone else { return true }
        return verticalSizeClass == .regular
#else
        return true
#endif
    }

    @State
    private var counts: SensorMeasurementCounts?

    @Environment(\.network)
    private var network

    private var list: some View {
        List(locations, selection: $selectedLocation) { location in
#if os(tvOS)
            label(for: location)
                .padding()
                .focusable()
                .focused($focusedLocation, equals: location)
                .foregroundColor(foregroundColor(for: location))
                .background(background(for: location))
                .cornerRadius(8)
#else
            NavigationLink(value: location) {
                label(for: location)
            }
#endif
        }
    }

    @ViewBuilder
    private var countCharts: some View {
        if let counts, showChart {
            Chart(Array(counts.perLocation).sorted(using: KeyPathComparator(\.value)), id: \.key) {
                BarMark(x: .value("Location", $0.key),
                        y: .value("Count", $0.value))
                .foregroundStyle(by: .value("Location", $0.key))
            }
            .chartForegroundStyleScale(domain: counts.perLocation.keys.sorted())
            .chartXAxis(.hidden)
            .chartYScale(type: .counts)
            .frame(maxHeight: 275)
#if !os(tvOS)
            .padding()
#endif
#if os(iOS)
            .background(Color(uiColor: .systemGroupedBackground))
#endif
            .transition(.move(edge: .bottom))
        }
    }

    private var content: some View {
#if os(tvOS)
        VStack {
            list
            countCharts
        }
#else
        list
            .safeAreaInset(edge: .bottom) {
                countCharts
            }
#endif
    }

    var body: some View {
        content
#if os(macOS)
            .frame(minWidth: 260)
#elseif os(tvOS)
            .frame(width: 400)
#endif
            .animation(.default, value: counts)
            .animation(.default, value: locations)
#if os(tvOS)
            .focused($isFocused)
            .defaultFocus($focusedLocation, selectedLocation)
            .onChange(of: isFocused) {
                guard $0 else { return }
                focusedLocation = selectedLocation
            }
            .onChange(of: focusedLocation) {
                guard $0 != nil else { return }
                selectedLocation = $0
            }
            .onChange(of: selectedLocation) {
                focusedLocation = $0
            }
#endif
            .refreshable {
                async let locations: Void = updateLocations()
                async let counts: Void = updateCounts()
                _ = await (locations, counts)
            }
            .periodicallyRefresh(frequency: .low) {
                await updateLocations()
            }
            .periodicallyRefresh(frequency: .high) {
                await updateCounts()
            }
    }

    private func label(for location: Location) -> some View {
        HStack {
            location.title
            Spacer()
            if let counts {
                Group {
                    switch location {
                    case .all: Text(counts.total, format: .number)
                    case .named(let location):
                        if let count = counts.perLocation[location] {
                            Text(count, format: .number)
                        }
                    }
                }
                .font(.callout)
                .transition(.move(edge: .trailing).combined(with: .opacity))
            }
        }
    }

#if os(tvOS)
    private func foregroundColor(for location: Location) -> Color? {
        guard location != focusedLocation else { return .black }
        guard location == selectedLocation else { return nil }
        switch colorScheme {
        case .light: return .white
        case .dark: return .black
        @unknown default: return .white
        }
    }

    private func background(for location: Location) -> some ShapeStyle {
        guard location != focusedLocation else { return AnyShapeStyle(.white) }
        guard location == selectedLocation else { return AnyShapeStyle(.clear) }
        switch colorScheme {
        case .light: return AnyShapeStyle(.black.opacity(0.5))
        case .dark: return AnyShapeStyle(.white.opacity(0.5))
        @unknown default: return AnyShapeStyle(.white.opacity(0.5))
        }
    }
#endif

    private func updateLocations() async {
        do {
            if locations.isEmpty {
                locations.append(.all)
            }
            locations[1...] = try await network.locations().sorted().lazy.map(Location.named)[...]
        } catch is CancellationError {
        } catch {
            print("Failed to fetch locations: \(error)")
            locations[1...].removeAll()
        }
    }

    private func updateCounts() async {
        do {
            counts = try await network.counts()
        } catch is CancellationError {
        } catch {
            print("Failed to fetch counts: \(error)")
            counts = nil
        }
    }
}
