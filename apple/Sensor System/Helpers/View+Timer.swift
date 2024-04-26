import Combine
import SwiftUI

private struct TimedView<C: Clock>: ViewModifier {
    fileprivate let clock: C
    fileprivate let duration: C.Duration
    fileprivate let tolerance: C.Duration?
    fileprivate let callInitially: Bool
    fileprivate let action: @Sendable () async -> ()

    @State
    private var isInitialRefresh = true

    func body(content: Content) -> some View {
        content
            .task {
                while !Task.isCancelled {
                    if isInitialRefresh {
                        isInitialRefresh = false
                        guard callInitially else { continue }
                    }
                    await action()
                    guard !Task.isCancelled else { return }
                    try? await Task.sleep(for: duration, tolerance: tolerance, clock: clock)
                }
            }
    }
}

extension View {
    func every<C: Clock>(_ duration: C.Duration,
                         tolerance: C.Duration? = nil,
                         clock: C = .continuous,
                         callInitially: Bool = true,
                         perform work: @escaping @Sendable () async -> ()) -> some View {
        modifier(TimedView(clock: clock, duration: duration, tolerance: tolerance, callInitially: callInitially, action: work))
    }
}

enum PeriodicRefreshFrequency: Sendable, Hashable {
    case low, high

    fileprivate var timerValues: (interval: Duration, tolerance: Duration?) {
        switch self {
        case .high: return (.seconds(4), .seconds(1))
        case .low: return (.seconds(8), .seconds(2))
        }
    }
}

extension View {
    func periodicallyRefresh(frequency: PeriodicRefreshFrequency = .high,
                             callInitially: Bool = true,
                             byExecuting work: @escaping @Sendable () async -> ()) -> some View {
        let timerValues = frequency.timerValues
        return every(timerValues.interval,
                     tolerance: timerValues.tolerance,
                     callInitially: callInitially,
                     perform: work)
    }
}
