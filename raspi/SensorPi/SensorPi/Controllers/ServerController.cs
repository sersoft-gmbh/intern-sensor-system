using System.Net.Http.Headers;
using System.Net.Http.Json;
using SensorPi.Models;

namespace SensorPi.Controllers;

public sealed class ServerController : IDisposable {
    private readonly HttpClient _httpClient;

    private ServerController(HttpClient httpClient, string accessToken)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public ServerController(Uri baseAddress, string accessToken) 
        : this(new HttpClient { BaseAddress = baseAddress }, accessToken) 
    {
    }

    ~ServerController() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing) _httpClient.Dispose();
    }

    public async Task SendMeasurement(Measurement measurement)
    {
        var response = await _httpClient.PutAsJsonAsync("/measurements", measurement);
        response.EnsureSuccessStatusCode();
    }
}
