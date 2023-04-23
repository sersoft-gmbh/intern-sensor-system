import SwiftUI

fileprivate extension Location {
#if os(macOS)
    static var `default`: Location { .all }
#else
    static var `default`: Location? {
        UIDevice.current.userInterfaceIdiom == .phone ? nil : .all
    }
#endif
}

struct ContentView: View {
    private enum DetailTab: Hashable {
        case values, charts
    }

    @State
    private var selectedTab = DetailTab.values

    @State
    private var locations = [Location.all]
    @State
    private var selectedLocation = Location.default

    @AppStorage("SelectedLocation")
    private var selectedLocationName: String?

#if os(tvOS)
    @Namespace
    private var listNamespace
    @Namespace
    private var detailsNamespace
    @FocusState
    private var listFocused: Bool
#endif

    @Environment(\.network)
    private var network

    private var content: some View {
#if os(tvOS)
        HStack(alignment: .top, spacing: 21) {
            LocationsList(locations: $locations,
                          selectedLocation: $selectedLocation)
            .focusSection()
            .focusScope(listNamespace)
            .focused($listFocused)
            
            if let selectedLocation {
                locationsDetails(for: selectedLocation)
                    .focusSection()
                    .focusScope(detailsNamespace)
                    .onMoveCommand {
                        switch $0 {
                        case .up: listFocused = true
                        case .left where selectedTab == .values:
                            listFocused = true
                        default: break
                        }
                    }
            }
        }
#else
        NavigationSplitView {
            LocationsList(locations: $locations,
                          selectedLocation: $selectedLocation)
            .navigationTitle("Locations")
            .navigationDestination(for: Location.self) {
                locationsDetails(for: $0)
            }
            .navigationSplitViewColumnWidth(ideal: 280)
        } detail: {
#if os(macOS)
            locationsDetails(for: selectedLocation)
#else
            if let selectedLocation {
                locationsDetails(for: selectedLocation)
            }
#endif
        }
#endif
    }
    
    var body: some View {
        content
        .animation(.default, value: locations)
        .animation(.default, value: selectedTab)
        .onChange(of: locations) {
#if os(macOS)
            if !$0.contains(selectedLocation) {
                selectedLocation = .all
            }
#else
            if selectedLocation.map($0.contains) != true {
                selectedLocation = .default
            }
#endif
        }
        .onChange(of: selectedLocationName) {
            selectedLocation = $0.map(Location.named) ?? .default
        }
        .onChange(of: selectedLocation) {
            switch $0 {
#if !os(macOS)
            case nil: selectedLocationName = nil
#endif
            case .all: selectedLocationName = nil
            case .named(let name): selectedLocationName = name
            }
        }
        .onAppear {
            let _selectedLocationName: String?
#if os(macOS)
            _selectedLocationName = selectedLocation.locationName
#else
            _selectedLocationName = selectedLocation?.locationName
#endif
            if selectedLocationName != _selectedLocationName {
                selectedLocation = selectedLocationName.map(Location.named) ?? .default
            }
        }
    }

    private func locationsDetails(for location: Location) -> some View {
        TabView(selection: $selectedTab) {
            let locationNames = locations.compactMap(\.locationName)
            LocationValuesView(locations: locationNames,
                               selectedLocationName: location.locationName)
            .tabItem {
                Label("Values", systemImage: "thermometer.medium")
            }
            .tag(DetailTab.values)
            ChartsView(locations: locationNames,
                       selectedLocationName: location.locationName)
            .tabItem {
                Label("Charts", systemImage: "chart.xyaxis.line")
            }
            .tag(DetailTab.charts)
        }
#if os(macOS)
        .padding(.top)
#elseif os(iOS)
        .navigationBarTitleDisplayMode(.inline)
#endif
        .navigationTitle(location.title)
    }
}

#if DEBUG
struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
    }
}
#endif
