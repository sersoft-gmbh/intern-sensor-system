import SwiftUI

struct StatisticsView: View {
    enum StatisticsKind: Sendable, Hashable, CaseIterable {
        case min, median, max
    }

    var statistics: SensorMeasurementStatistics
    var kind: StatisticsKind
    var showLocation: Bool

    private var measurements: (temperature: SensorMeasurement?, humidity: SensorMeasurement?) {
        switch kind {
        case .min: return (statistics.minTemperature, statistics.minHumidity)
        case .median: return (statistics.medianTemperature, statistics.medianHumidity)
        case .max: return (statistics.maxTemperature, statistics.maxHumidity)
        }
    }

    private var titleKey: LocalizedStringKey {
        switch kind {
        case .min: return "Min"
        case .median: return "Median"
        case .max: return "Max"
        }
    }

    private var displayOptions: SensorMeasurementDisplayOptions {
        var options = SensorMeasurementDisplayOptions.showDate
        if showLocation {
            options.insert(.showLocation)
        }
        return options
    }

    var body: some View {
        let (temperature, humidity) = measurements
        if temperature != nil || humidity != nil {
            ValueBox(style: .group(spacing: 8), titleKey) {
                if let temperature {
                    TemperatureView(measurement: temperature,
                                    displayOptions: displayOptions,
                                    showFeelsLike: false)
                    .transition(.opacity)
                }
                if let humidity {
                    HumidityView(measurement: humidity,
                                 displayOptions: displayOptions)
                    .transition(.opacity)
                }
            }
            .multilineTextAlignment(.center)
            .transition(.opacity)
        }
    }
}

#if DEBUG
struct StatisticsView_Previews: PreviewProvider {
    static var previews: some View {
        StatisticsView(statistics: .preview, kind: .min, showLocation: true)
        StatisticsView(statistics: .preview, kind: .median, showLocation: true)
        StatisticsView(statistics: .preview, kind: .max, showLocation: true)
    }
}
#endif
