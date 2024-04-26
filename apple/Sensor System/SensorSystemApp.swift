import SwiftUI

extension Network {
    static let backendBaseURL = URL(string: "CHANGE ME")!
}

@main
struct SensorSystemApp: App {
#if os(tvOS) || os(iOS)
    @Environment(\.scenePhase)
    private var scenePhase
#endif

    var body: some Scene {
        WindowGroup {
            ContentView()
#if os(tvOS) || os(iOS)
                .onChange(of: scenePhase) {
                    UIApplication.shared.isIdleTimerDisabled = $1 == .active
                }
#endif
        }
    }
}
