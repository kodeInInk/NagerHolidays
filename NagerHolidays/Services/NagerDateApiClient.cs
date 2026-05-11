using System.Net;
using System.Text.Json;
using NagerHolidays.Models.DTOs;

namespace NagerHolidays.Services;

public class NagerDateApiClient : INagerDateApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<NagerDateApiClient> _logger;

    //case-insensitivity, preventing deserialisation failures due to casing mismatches
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NagerDateApiClient(HttpClient http, ILogger<NagerDateApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<NagerCountryDto>> GetAvailableCountriesAsync(CancellationToken ct = default)
    {
        //get https://date.nager.at/api/v3/AvailableCountries
        var result = await _http.GetFromJsonAsync<List<NagerCountryDto>>("AvailableCountries", JsonOptions, ct);
        return result ?? new List<NagerCountryDto>();
    }

    public async Task<List<NagerHolidayDto>> GetPublicHolidaysAsync(int year, string countryCode, CancellationToken ct = default)
    {
        //get https://date.nager.at/api/v3/PublicHolidays/{year}/{countryCode}
        var url = $"PublicHolidays/{year}/{countryCode}";
        try
        {
            using var response = await _http.GetAsync(url, ct);
            // some country codes might return 204 (NoContent) or 404 (NotFound) when data is unavailable
            if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<NagerHolidayDto>();
            }
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<List<NagerHolidayDto>>(JsonOptions, ct);
            
            return data ?? new List<NagerHolidayDto>();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Could not fetch country for year {Country}/{Year}", countryCode, year);
            return new List<NagerHolidayDto>();
        }
    }
}