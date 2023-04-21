import SwiftUI

struct StatisticsView: View {
    enum StatisticsKind: Sendable, Hashable, CaseIterable {
        case min, median, max
    }

    var statistics: SensorsMeasurementStatistics
    var kind: StatisticsKind

    private var measurements: (temperature: SensorsMeasurement?, humidity: SensorsMeasurement?) {
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

    var body: some View {
        let (temperature, humidity) = measurements
        if temperature != nil || humidity != nil {
            ValuesGroupView(titleKey) {
                if let temperature {
                    TemperaturesView(measurement: temperature,
                                     showDate: true,
                                     showFeelsLike: false)
                    .transition(.opacity)
                }
                if let humidity {
                    HumidityView(measurement: humidity,
                                 showDate: true)
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
        StatisticsView(statistics: .preview, kind: .min)
        StatisticsView(statistics: .preview, kind: .median)
        StatisticsView(statistics: .preview, kind: .max)
    }
}
#endif
