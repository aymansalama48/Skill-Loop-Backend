using Skill_Loop.Application.Common.Abstractions.External.Client;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Skill_Loop.Infrastructure.External.Client;

/// <summary>
/// خدمة استعلام الموقع الجغرافي للـ IP بأسلوب آمن في بيئة الإنتاج
/// </summary>
public sealed class GeoLocationService : IGeoLocationService
{
    private readonly HttpClient _httpClient;

    public GeoLocationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetLocationAsync(string? ipAddress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || IsLocalIp(ipAddress))
        {
            return "شبكة محلية (Development / Local)";
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<IpApiResponse>(
                $"http://ip-api.com/json/{ipAddress}?fields=status,country,city",
                cancellationToken);

            if (response != null && string.Equals(response.Status, "success", StringComparison.OrdinalIgnoreCase))
            {
                var city = string.IsNullOrWhiteSpace(response.City) ? "مدينة غير معروفة" : response.City;
                var country = string.IsNullOrWhiteSpace(response.Country) ? "دولة غير معروفة" : response.Country;

                return $"{city}, {country}";
            }
        }
        catch (Exception)
        {
            // عدم إيقاف الخدمة في حال فشل API الجغرافيا الخارجي
        }

        return "موقع غير معروف";
    }

    private static bool IsLocalIp(string ip)
    {
        if (ip == "127.0.0.1" || ip == "::1" || ip.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return true;

        if (IPAddress.TryParse(ip, out var parsedIp))
        {
            var bytes = parsedIp.GetAddressBytes();
            if (bytes.Length == 4)
            {
                if (bytes[0] == 10) return true;
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
                if (bytes[0] == 192 && bytes[1] == 168) return true;
            }
        }

        return false;
    }

    private sealed record IpApiResponse(
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("country")] string Country,
        [property: JsonPropertyName("city")] string City);
}
