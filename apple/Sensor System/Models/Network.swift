import Foundation
import OSLog

struct Network {
    private struct Request: Sendable, Hashable {
        private let path: Array<String>
        private let query: Array<URLQueryItem>?

        init(path: Array<String>, query: Array<URLQueryItem>? = nil) {
            self.path = path
            self.query = query.flatMap { $0.isEmpty ? nil : $0 }
        }

        init(path: some Sequence<String>, query: some Sequence<URLQueryItem>) {
            self.init(path: Array(path), query: Optional(Array(query)))
        }

        init(path: String..., query: URLQueryItem...) {
            self.init(path: path, query: Optional(query))
        }

        init(path: String..., query: Array<URLQueryItem>?) {
            self.init(path: path, query: query)
        }

        var urlRequest: URLRequest {
            var url = backendBaseURL
            for component in path {
                url.append(component: component)
            }
            if let query {
                url.append(queryItems: query)
            }
            return URLRequest(url: url)
        }
    }

    enum ResponseError: Error, CustomStringConvertible {
        case invalidResponse(URLResponse)
        case invalidStatusCode(Int)

        var description: String {
            switch self {
            case .invalidResponse(let response): return "Response is not an HTTP response: \(response)"
            case .invalidStatusCode(let statusCode): return "Invalid status code: \(statusCode)"
            }
        }
    }

    private let session = URLSession.shared
    private let jsonDecoder: JSONDecoder = {
        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601
        return decoder
    }()
    private let logger = Logger.makeAppLogger(category: "network")

    private func fetch<T: Decodable>(_ request: Request) async throws -> T {
        do {
#if DEBUG
            let (requestID, requestStart) = (UUID(), ContinuousClock.now)
            logger.debug("Sending request (\(requestID, privacy: .public)): \(String(describing: request))")
#endif
            let (data, response) = try await session.data(for: request.urlRequest)
#if DEBUG
            logger.debug("Received response (\(requestID, privacy: .public)) after \(requestStart.duration(to: .now).formatted(Duration.UnitsFormatStyle(allowedUnits: [.hours, .minutes, .seconds, .milliseconds, .microseconds, .nanoseconds], width: .abbreviated)), privacy: .public): \(response)")
            logger.debug("Received response data (\(requestID, privacy: .public)): \(String(decoding: data, as: UTF8.self))")
#endif
            guard let httpResponse = response as? HTTPURLResponse
            else { throw ResponseError.invalidResponse(response) }
            guard (200..<300).contains(httpResponse.statusCode)
            else { throw ResponseError.invalidStatusCode(httpResponse.statusCode) }
            return try jsonDecoder.decode(T.self, from: data)
        } catch let error as URLError where error.code == .cancelled {
            throw CancellationError()
        }
    }

    private func fetchReplacingNotFoundWithNil<T: Decodable>(_ request: Request) async throws -> T? {
        do {
            return try await fetch(request)
        } catch ResponseError.invalidStatusCode(404) {
            return nil
        }
    }

    func locations() async throws -> Array<String> {
        try await fetch(.init(path: "locations"))
    }

    func measurements(ascending: Bool = false, count: Int? = nil, location: String? = nil) async throws -> Array<SensorMeasurement> {
        var query: Array<URLQueryItem> = [
            URLQueryItem(name: "sortDirection", value: ascending ? "ascending" : "descending")
        ]
        if let count {
            query.append(.init(name: "count", value: String(count)))
        }
        if let location {
            query.append(.init(name: "location", value: location))
        }
        return try await fetch(.init(path: "measurements", query: query))
    }

    func latestMeasurement(forLocation location: String? = nil) async throws -> SensorMeasurement? {
        try await fetchReplacingNotFoundWithNil(.init(path: "measurements", "latest",
                                                      query: location.map { [.init(name: "location", value: $0)] }))
    }

    func counts() async throws -> SensorMeasurementCounts {
        try await fetch(.init(path: "measurements", "counts"))
    }

    func statistics(forLocation location: String? = nil) async throws -> SensorMeasurementStatistics {
        try await fetch(.init(path: "measurements", "statistics",
                              query: location.map { [.init(name: "location", value: $0)] }))
    }
}
