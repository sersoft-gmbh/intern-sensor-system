import OSLog

extension Logger {
    static func makeAppLogger(category: String) -> Logger {
        .init(subsystem: "de.sersoft.internship.Sensor-System",
              category: category)
    }
}
