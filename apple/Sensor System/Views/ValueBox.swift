import SwiftUI

fileprivate extension Double {
    static var groupFooterSizeDefault: Self {
#if os(tvOS)
        return 18
#else
        return 15
#endif
    }

    static var singleContentSizeDefault: Self {
#if os(tvOS)
        return 42
#else
        return 20
#endif
    }

    static var singleFooterSizeDefault: Self {
#if os(tvOS)
        return 15
#elseif os(macOS)
        return 12
#else
        return 13
#endif
    }
}

struct ValueBox<BoxContent: View, Header: View, Footer: View>: View {
    enum Style: Sendable, Hashable {
        case group(spacing: CGFloat?), single

        static var group: Self { .group(spacing: nil) }
    }

    var style: Style

    private var header: Header?
    private var content: BoxContent
    private var footer: Footer?

    @ScaledMetric(relativeTo: .body)
    private var singleContentSize = Double.singleContentSizeDefault

    @ScaledMetric
    private var footerSize: Double

#if os(iOS)
    private let bgMaterial: Material
#endif
    private let bgOpacity: Double

    private var background: some ShapeStyle {
#if os(iOS)
        bgMaterial.opacity(bgOpacity)
#else
        .background.opacity(bgOpacity)
#endif
    }

    var body: some View {
        Group {
            switch style {
            case .single:
                VStack(spacing: 8) {
                    header
                        .font(.subheadline)
                        .bold()
                    content
                        .font(.system(size: singleContentSize))
                        .monospacedDigit()
                    footer
                        .font(.system(size: footerSize))
                }
            case .group(let spacing):
                VStack {
                    header
                        .font(.headline)
                    HStack(alignment: .top, spacing: spacing) {
                        content
                    }
                    footer
                        .font(.system(size: footerSize))
                }
            }
        }
        .padding()
        .background(background)
        .cornerRadius(5)
    }

    private init(style: Style,
                 header: Header?,
                 content: BoxContent,
                 footer: Footer?) {
        self.style = style
        self.header = header
        self.content = content
        self.footer = footer
        switch style {
        case .single:
#if os(iOS)
            bgOpacity = 1
            bgMaterial = .regularMaterial
#else
            bgOpacity = 0.25
#endif
            _footerSize = .init(wrappedValue: .singleFooterSizeDefault, relativeTo: .footnote)
        case .group(_):
            bgOpacity = 0.5
#if os(iOS)
            bgMaterial = .ultraThinMaterial
#endif
            _footerSize = .init(wrappedValue: .groupFooterSizeDefault, relativeTo: .footnote)
        }
    }

    init(style: Style = .single,
         @ViewBuilder content: () -> BoxContent,
         @ViewBuilder header: () -> Header,
         @ViewBuilder footer: () -> Footer) {
        self.init(style: style,
                  header: header(),
                  content: content(),
                  footer: footer())
    }

    init(style: Style = .single,
         @ViewBuilder content: () -> BoxContent)
    where Header == Never, Footer == Never
    {
        self.init(style: style,
                  header: nil,
                  content: content(),
                  footer: nil)
    }

    init(style: Style = .single,
         @ViewBuilder content: () ->  BoxContent,
         @ViewBuilder footer: () -> Footer)
    where Header == Never
    {
        self.init(style: style,
                  header: nil,
                  content: content(),
                  footer: footer())
    }

    init(style: Style = .single,
         @ViewBuilder content: () -> BoxContent,
         @ViewBuilder header: () -> Header)
    where Footer == Never
    {
        self.init(style: style,
                  header: header(),
                  content: content(),
                  footer: nil)
    }
}

extension ValueBox where Header == Text {
    init(style: Style = .single,
         header: Text,
         @ViewBuilder content: () -> BoxContent,
         @ViewBuilder footer: () -> Footer) {
        self.init(style: style,
                  content: content,
                  header: { header },
                  footer: footer)
    }

    init(style: Style = .single,
         _ header: LocalizedStringKey,
         @ViewBuilder content: () -> BoxContent,
         @ViewBuilder footer: () -> Footer) {
        self.init(style: style,
                  header: Text(header),
                  content: content,
                  footer: footer)
    }

    @_disfavoredOverload
    init(style: Style = .single,
         _ header: some StringProtocol,
         @ViewBuilder content: () -> BoxContent,
         @ViewBuilder footer: () -> Footer) {
        self.init(style: style,
                  header: Text(header),
                  content: content,
                  footer: footer)
    }
}

extension ValueBox where Header == Text, Footer == Never {
    init(style: Style = .single,
         header: Text,
         @ViewBuilder content: () -> BoxContent) {
        self.init(style: style,
                  content: content,
                  header: { header })
    }

    init(style: Style = .single,
         _ header: LocalizedStringKey,
         @ViewBuilder content: () -> BoxContent) {
        self.init(style: style,
                  header: Text(header),
                  content: content)
    }

    @_disfavoredOverload
    init(style: Style = .single,
         _ header: some StringProtocol,
         @ViewBuilder content: () -> BoxContent) {
        self.init(style: style,
                  header: Text(header),
                  content: content)
    }
}

#if DEBUG
#Preview {
    ValueBox("Group Preview") {
        Text("Group Content")
    } footer: {
        Text("Footer Preview")
    }
}
#endif
