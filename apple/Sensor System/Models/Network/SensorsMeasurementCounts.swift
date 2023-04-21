struct SensorMeasurementCounts: Sendable, Hashable, Codable {
    let total: Int
    let perLocation: Dictionary<String, Int>
}
