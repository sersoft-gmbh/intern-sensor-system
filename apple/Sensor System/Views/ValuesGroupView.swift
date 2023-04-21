import SwiftUI

struct ValuesGroupView<GroupContent: View>: View {
    var title: Text
    var date: Date?
    @ViewBuilder
    var content: GroupContent

    @ScaledMetric(relativeTo: .footnote)
    private var dateSize = Double.dateSizeDefault

    var body: some View {
        VStack {
            title
                .font(.headline)
            HStack(alignment: .top) {
                content
            }
            if let date {
                Text(date, format: .fullDateTime)
                    .font(.system(size: dateSize))
            }
        }
        .padding()
#if os(iOS)
        .background(.ultraThinMaterial.opacity(0.5))
#else
        .background(.background.opacity(0.5))
#endif
        .cornerRadius(5)
    }
}

fileprivate extension Double {
    static var dateSizeDefault: Self {
#if os(tvOS)
        return 18
#else
        return 15
#endif
    }
}

extension ValuesGroupView {
    init(_ title: LocalizedStringKey,
         date: Date? = nil,
         @ViewBuilder content: () -> GroupContent) {
        self.init(title: Text(title), date: date, content: content)
    }

    @_disfavoredOverload
    init(_ title: some StringProtocol,
         date: Date? = nil,
         @ViewBuilder content: () -> GroupContent) {
        self.init(title: Text(title), date: date, content: content)
    }
}

#if DEBUG
struct ValuesGroupView_Previews: PreviewProvider {
    static var previews: some View {
        ValuesGroupView(title: Text("Group Preview"), date: .now) {
            Text("Group Content")
        }
    }
}
#endif
