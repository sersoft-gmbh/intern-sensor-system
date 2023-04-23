import SwiftUI

enum Location: Sendable, Hashable, Identifiable {
    case all
    case named(String)

    var id: some Hashable { self }

    var title: Text {
        switch self {
        case .all: return Text("All")
        case .named(let name): return Text(name)
        }
    }

    var locationName: String? {
        switch self {
        case .all: return nil
        case .named(let location): return location
        }
    }
}
