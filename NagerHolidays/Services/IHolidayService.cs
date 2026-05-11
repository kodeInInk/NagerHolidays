using NagerHolidays.Models.DTOs;

namespace NagerHolidays.Services;

public interface IHolidayService
{
    /// <summary>
    /// Given a country, this endpoint returns the last celebrated <paramref name="amount"/>(default = 3)
    /// holidays(date and name) from the database.
    /// </summary>
    Task<IEnumerable<HolidaySummaryDto>> GetLastCelebratedAsync(
        string countryCode, int amount = 3, DateTime? asOf = null);

    /// <summary>
    /// Given a year and country codes, for each country this endpoint returns the number of public holidays not
    /// falling on weekends(Saturday/Sunday), sorted in descending order, from the database.
    /// </summary>
    Task<IEnumerable<CountryHolidayCountDto>> GetNonWeekendCountsAsync(
        int year, IEnumerable<string> countryCodes);

    /// <summary>
    /// Given a year and 2 country codes(A and B), this endpoint returns the deduplicated list of dates celebrated in both
    /// countries (date + local names) from the database.
    /// </summary>
    Task<IEnumerable<CommonHolidayDto>> GetCommonHolidaysAsync(
        int year, string countryCodeA, string countryCodeB);
}