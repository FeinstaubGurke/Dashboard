using Dashboard.Clients.Models;
using System.Net.Http.Headers;

namespace Dashboard.Clients
{
    public class TheThingsStackClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _applicationId;

        public TheThingsStackClient(
            IConfiguration configuration,
            HttpClient httpClient)
        {
            var apiKey = configuration.GetValue<string>("TheThingsNetwork:ApiKey");
            this._applicationId = configuration.GetValue<string>("TheThingsNetwork:ApplicationId") ?? "";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpClient.BaseAddress = new Uri("https://eu1.cloud.thethings.network");

            this._httpClient = httpClient;
        }

        public async Task<EndDevices[]> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            var response = await this._httpClient.GetAsync($"/api/v3/applications/{this._applicationId}/devices?field_mask=name,description,attributes");
            var deviceResponse = await response.Content.ReadFromJsonAsync<DeviceResponse>();

            if (deviceResponse == null)
            {
                return [];
            }

            return deviceResponse.end_devices;
        }
    }
}


