import Combine
import SwiftUI

private struct TimedView: ViewModifier {
    final class TimedObject: ObservableObject {
        @Published
        private(set) var lastRefresh: Date?

        init(timeInterval: TimeInterval, tolerance: TimeInterval?) {
            Timer.publish(every: timeInterval,
                          tolerance: tolerance,
                          on: .main,
                          in: .default)
            .autoconnect()
            .map { $0 }
            .assign(to: &$lastRefresh)
        }
    }

    @StateObject
    fileprivate var refreshTimer: TimedObject

    fileprivate let action: () async -> ()

    func body(content: Content) -> some View {
        content
            .task(id: refreshTimer.lastRefresh) {
                await action()
            }
    }
}

fileprivate extension Duration {
    var timeInterval: TimeInterval {
        var seconds = TimeInterval(components.seconds)
        seconds += TimeInterval(components.attoseconds) / 1e18
        return seconds
    }
}

extension View {
    func every(_ timeInterval: TimeInterval,
               tolerance: TimeInterval? = nil,
               perform work: @escaping () async -> ()) -> some View {
        modifier(TimedView(refreshTimer: .init(timeInterval: timeInterval,
                                               tolerance: tolerance),
                           action: work))
    }

    func every(_ duration: Duration,
               tolerance: Duration? = nil,
               perform work: @escaping () async -> ()) -> some View {
        every(duration.timeInterval, tolerance: tolerance?.timeInterval, perform: work)
    }
}

enum PeriodicRefreshFrequency: Sendable, Hashable {
    case low, high

    fileprivate var timerValues: (interval: TimeInterval, tolerance: TimeInterval?) {
        switch self {
        case .high: return (4, 1)
        case .low: return (8, 2)
        }
    }
}

extension View {
    func periodicallyRefresh(frequency: PeriodicRefreshFrequency = .high,
                             byExecuting work: @escaping () async -> ()) -> some View {
        let timerValues = frequency.timerValues
        return every(timerValues.interval, tolerance: timerValues.tolerance, perform: work)
    }
}
