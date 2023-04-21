import SwiftUI

fileprivate enum NetworkEnvironmentKey: EnvironmentKey {
    typealias Value = Network

    static let defaultValue = Network()
}

extension EnvironmentValues {
    var network: Network {
        self[NetworkEnvironmentKey.self]
    }
}
