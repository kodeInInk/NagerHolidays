using NagerHolidays.Models.DTOs;

namespace NagerHolidays.Services;

public interface INagerDateApiClient
{
    Task<List<NagerCountryDto>> GetAvailableCountriesAsync(CancellationToken ct = default);
    Task<List<NagerHolidayDto>> GetPublicHolidaysAsync(int year, string countryCode, CancellationToken ct = default);
}