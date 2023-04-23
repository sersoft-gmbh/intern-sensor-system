struct SensorMeasurementDisplayOptions: OptionSet, Sendable, Hashable {
    typealias RawValue = UInt

    let rawValue: RawValue

    init(rawValue: RawValue) {
        self.rawValue = rawValue
    }
}

extension SensorMeasurementDisplayOptions {
    static let showDate = SensorMeasurementDisplayOptions(rawValue: 1 << 0)
    static let showLocation = SensorMeasurementDisplayOptions(rawValue: 1 << 1)
}
