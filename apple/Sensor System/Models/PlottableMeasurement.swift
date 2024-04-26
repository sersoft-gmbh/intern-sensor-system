import Foundation
import Charts

struct PlottableMeasurement<Unit: Dimension>: Sendable, Plottable, Hashable, Comparable  {
    struct FormatStyle: Foundation.FormatStyle {
        private let formatStyle: Measurement<Unit>.FormatStyle

        init(formatStyle: Measurement<Unit>.FormatStyle) {
            self.formatStyle = formatStyle
        }

        init(from decoder: any Decoder) throws {
            formatStyle = try .init(from: decoder)
        }

        func encode(to encoder: any Encoder) throws {
            try formatStyle.encode(to: encoder)
        }

        func locale(_ locale: Locale) -> Self {
            .init(formatStyle: formatStyle.locale(locale))
        }

        func format(_ value: PlottableMeasurement<Unit>) -> String {
            formatStyle.format(value.measurement)
        }
    }

    private let baseValue: Double

    var measurement: Measurement<Unit> {
        .init(value: baseValue, unit: .baseUnit())
    }

    var primitivePlottable: Double { baseValue }

    init(measurement: Measurement<Unit>) {
        baseValue = measurement.converted(to: .baseUnit()).value
    }

    init(value: Double, unit: Unit) {
        self.init(measurement: .init(value: value, unit: unit))
    }

    init?(primitivePlottable: Double) {
        baseValue = primitivePlottable
    }

    public static func <(lhs: Self, rhs: Self) -> Bool {
        lhs.baseValue < rhs.baseValue
    }
}

extension Measurement where UnitType: Dimension {
    var plottable: PlottableMeasurement<UnitType> { .init(measurement: self) }
}
