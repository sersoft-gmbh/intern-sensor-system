import SwiftUI

struct ValueView<ValueView: View>: View {
    var title: Text
    var date: Date?
    @ViewBuilder
    var value: ValueView

    @ScaledMetric(relativeTo: .body)
    private var valueSize = Double.valueSizeDefault
    @ScaledMetric(relativeTo: .footnote)
    private var dateSize = Double.dateSizeDefault

    var body: some View {
        VStack(spacing: 8) {
            title
                .font(.subheadline)
                .bold()
            value
                .font(.system(size: valueSize))
                .monospacedDigit()
            if let date {
                Text(date, format: .shortDateTime)
                    .font(.system(size: dateSize))
            }
        }
        .padding()
#if os(iOS)
        .background(.regularMaterial)
#else
        .background(.background.opacity(0.5))
#endif
        .cornerRadius(5)
    }
}

fileprivate extension Double {
    static var valueSizeDefault: Self {
#if os(tvOS)
        return 42
#else
        return 20
#endif
    }

    static var dateSizeDefault: Self {
#if os(tvOS)
        return 15
#elseif os(macOS)
        return 12
#else
        return 13
#endif
    }
}

extension ValueView {
    init(_ title: LocalizedStringKey, date: Date? = nil, @ViewBuilder value: () -> ValueView) {
        self.init(title: Text(title), date: date, value: value)
    }

    @_disfavoredOverload
    init(_ title: some StringProtocol, date: Date? = nil, @ViewBuilder value: () -> ValueView) {
        self.init(title: Text(title), date: date, value: value)
    }
}

#if DEBUG
struct ValueView_Previews: PreviewProvider {
    static var previews: some View {
        ValueView("Preview", date: .now) {
            Text("Value")
        }
    }
}
#endif
