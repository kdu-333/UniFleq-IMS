using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace UnifleqSolutions_IMS.Services
{
    public class CaptchaService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public CaptchaService(IConfiguration config, HttpClient http)
        {
            _config = config;
            _http = http;
        }

        public async Task<bool> VerifyAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;

            var secretKey = _config["ReCaptcha:SecretKey"];
            var response = await _http.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}",
                null);

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("success").GetBoolean();
        }
    }
}