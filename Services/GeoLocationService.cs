using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace UnifleqSolutions_IMS.Services
{
    public class GeoLocationService
    {
        private readonly HttpClient _http;

        public GeoLocationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> GetLocationAsync(string ipAddress)
        {
            try
            {
                // Use real IP, fallback for localhost
                if (string.IsNullOrEmpty(ipAddress) ||
                    ipAddress == "::1" ||
                    ipAddress == "127.0.0.1")
                    return "localhost";

                var response = await _http.GetStringAsync($"http://ip-api.com/json/{ipAddress}");
                var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.GetProperty("status").GetString() == "success")
                {
                    var city = root.GetProperty("city").GetString();
                    var region = root.GetProperty("regionName").GetString();
                    var country = root.GetProperty("countryCode").GetString();
                    return $"{city}, {region}, {country}";
                }

                return "Unknown Location";
            }
            catch
            {
                return "Unknown Location";
            }
        }
    }
}