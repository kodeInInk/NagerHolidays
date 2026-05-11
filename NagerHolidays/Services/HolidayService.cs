using Microsoft.EntityFrameworkCore;
using NagerHolidays.Data;
using NagerHolidays.Models.DTOs;

namespace NagerHolidays.Services;

public class HolidayService: IHolidayService
{
    private readonly HolidayDbContext _context;

    public HolidayService(HolidayDbContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<HolidaySummaryDto>> GetLastCelebratedAsync(string countryCode, int amount = 3, DateTime? asOf = null)
    {
        var today = asOf.HasValue ? DateOnly.FromDateTime(asOf.Value) : DateOnly.FromDateTime(DateTime.UtcNow);
        var code = countryCode.ToUpperInvariant(); //this is how nager stores them, as upper case

        var lastCelebratedHolidays = await _context.Holidays
            .AsNoTracking()
            .Where(h => h.CountryCode == code && h.Date <= today)
            .OrderByDescending(h => h.Date)
            .Take(amount)
            .Select(h => new HolidaySummaryDto
            {
                Date = h.Date,
                Name = h.Name,
                LocalName = h.LocalName,
            })
            .ToListAsync();
        return lastCelebratedHolidays;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CountryHolidayCountDto>> GetNonWeekendCountsAsync(int year, IEnumerable<string> countryCodes)
    {
        var codes = countryCodes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray();
        if (codes.Length == 0)
        {
            return new List<CountryHolidayCountDto>();
        }
        
        var allYear = await _context.Holidays
            .AsNoTracking()
            .Where(h => h.Year == year && codes.Contains(h.CountryCode))
            .Select(h => new { h.Date, h.CountryCode })
            .ToListAsync();
        var countHolidays = allYear
            .Where(x =>
                x.Date.DayOfWeek != DayOfWeek.Saturday &&
                x.Date.DayOfWeek != DayOfWeek.Sunday)
            .GroupBy(x => x.CountryCode)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var nameCountries = await _context.Countries
            .AsNoTracking()
            .Where(c => codes.Contains(c.CountryCode))
            .ToDictionaryAsync(c => c.CountryCode, c => c.Name);

        return codes.Select(code => new CountryHolidayCountDto
        {
            CountryCode = code,
            CountryName = nameCountries.ContainsKey(code) ? nameCountries[code] : code,
            NonWeekendHolidayCount = countHolidays.ContainsKey(code) ? countHolidays[code] : 0
        })
        .OrderByDescending(h => h.NonWeekendHolidayCount)
        .ThenBy(h => h.CountryName)
        .ToList();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CommonHolidayDto>> GetCommonHolidaysAsync(int year, string countryCodeA, string countryCodeB)
    {
        var codeA = countryCodeA.ToUpperInvariant();
        var codeB = countryCodeB.ToUpperInvariant();
        
        var holidaysAorB = await _context.Holidays
            .AsNoTracking()
            .Where(h => h.Year == year && 
                        (h.CountryCode == codeA || h.CountryCode == codeB))
            .Select(h => new { h.Date, h.CountryCode, h.LocalName })
            .ToListAsync();
        
        //group to help idenntify overlapping holidays
        var groupedHolidaysAorB = holidaysAorB
            .GroupBy(h => h.Date)
            .Where(g =>
                g.Any(x=> x.CountryCode == codeA)
                && g.Any(x => x.CountryCode == codeB));
        
        var commonHolidays = new List<CommonHolidayDto>();
        foreach (var group in groupedHolidaysAorB.OrderBy(g => g.Key))
        {
            var commonHoliday = new CommonHolidayDto{Date = group.Key};
            //keeping only distinct local names per country, making this a list, as a country may(alhough highly
            //unlikely) have more than one holiday named diff in a same day
            foreach (var holidaysByCountry in group.GroupBy(x => x.CountryCode))
            {
                commonHoliday.LocalNames[holidaysByCountry.Key] = holidaysByCountry
                    .Select(s => s.LocalName)
                    .Distinct()
                    .ToList();
            }

            commonHolidays.Add(commonHoliday);
        }
        return commonHolidays;
    }
}