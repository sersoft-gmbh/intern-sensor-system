import SwiftUI

struct LocationsList: View {
    var locations: Array<String>

    @State
    private var counts: SensorMeasurementCounts?

    @Environment(\.network)
    private var network

    var body: some View {
        List {
            HStack {
                Text("All")
                Spacer()
                if let counts {
                    Text(counts.total, format: .number)
                }
            }
            ForEach(locations, id: \.self) { location in
                HStack {
                    Text(location)
                    Spacer()
                    if let count = counts?.perLocation[location] {
                        Text(count, format: .number)
                    }
                }
            }
        }
        .animation(.default, value: counts)
        .task {
            await updateCounts()
        }
        .onReceive(Timer.publish(every: 5, on: .main, in: .default).autoconnect()) { _ in
            Task {
                await updateCounts()
            }
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
