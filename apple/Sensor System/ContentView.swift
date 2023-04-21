import SwiftUI

struct ContentView: View {
    private enum Tab: Hashable {
        case values, charts
    }

    @State
    private var selectedTab: Tab = .values

    @State
    private var locations = Array<String>()

    @Environment(\.network)
    private var network
    
    var body: some View {
        TabView(selection: $selectedTab) {
            LocationValuesView(locations: locations)
                .tabItem {
                    Label("Values", systemImage: "thermometer.medium")
                }
                .tag(Tab.values)
            ChartsView(locations: locations)
                .tabItem {
                    Label("Charts", systemImage: "chart.xyaxis.line")
                }
                .tag(Tab.charts)
        }
#if os(macOS)
        .padding(.top)
#endif
        .animation(.default, value: locations)
        .animation(.default, value: selectedTab)
        .task {
            guard locations.isEmpty else { return }
            await updateLocations()
        }
        .onReceive(Timer.publish(every: 5, on: .main, in: .default).autoconnect()) { _ in
            Task {
                await updateLocations()
            }
        }
    }

    private func updateLocations() async {
        do {
            locations = try await network.locations()
        } catch is CancellationError {
            return
        } catch {
            print("Failed to fetch locations: \(error)")
            locations.removeAll()
        }
    }
}

#if DEBUG
struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
    }
}
#endif
