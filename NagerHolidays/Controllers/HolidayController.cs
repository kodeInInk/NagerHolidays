using Microsoft.AspNetCore.Mvc;
using NagerHolidays.Models;
using NagerHolidays.Repositories;
using NagerHolidays.Services;

namespace NagerHolidays.Controllers;

[ApiController]
[Route("holiday")]
public class HolidayController(
    ILogger<HolidayController> logger,
    IBaseRepository<Holiday> holidayRepository,
    IHolidayService holidayService
) : BaseController<Holiday, IBaseRepository<Holiday>, HolidayController>(holidayRepository, logger)
{
    private readonly IHolidayService _holidayService = holidayService;

    /// <summary>
    /// Given a country, this endpoint returns the last celebrated <paramref name="amount"/>(default = 3)
    /// holidays(date and name) from the database.
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 code-"AT"/"RO"/...</param>
    /// <param name="amount">Optional- returnable number of holidays(default=3)</param>
    /// <returns>
    /// <response code="200">Returns the last celebrated holidays</response>
    /// <response code="400">Returns BadRequest if amount is too small or missing/invalid country code</response>
    /// <response code="404">No last holidays found</response>
    /// <response code="500">Unexpected server error.</response>
    /// </returns>
    [HttpGet("lastCelebrated/{countryCode}")]
    public async Task<IActionResult> GetLastCelebrated(string countryCode, [FromQuery] int amount = 3)
    {
        if (string.IsNullOrWhiteSpace(countryCode)) { return BadRequest(new ApiError(400, "Country code required")); }
        if (amount < 1){ return BadRequest(new ApiError(400, "Amount must be >=1")); }

        try
        {
            var holidays = await _holidayService.GetLastCelebratedAsync(countryCode, amount);
            if (holidays == null || !holidays.Any())
            {
                return NotFound(new ApiError(404, "No celebrated holiday found for the specified country."));
            }

            return Ok(holidays);
        } catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new ApiError(500, ex.Message));
        }
    }

    /// <summary>
    /// Given a year and country codes, for each country this endpoint returns the number of public holidays not
    /// falling on weekends(Saturday/Sunday), sorted in descending order, from the database.
    /// </summary>
    /// <param name="year">Calendar year to evaluate</param>
    /// <param name="countryCodes">Comma-separated ISO 3166-1 alpha-2 codes-"NL,RO,DE,...,AT"</param>
    /// <returns>
    /// <response code="200">Returns the per country, non weekend counts</response>
    /// <response code="400">Returns BadRequest if year/codes are missing or invalid.</response>
    /// <response code="500">Unexpected server error.</response>
    /// </returns>
    [HttpGet("nonWeekendCounts")]
    public async Task<IActionResult> GetNonWeekendCounts(
        [FromQuery] int year,
        [FromQuery] string countryCodes)
    {
        if (year < 1) { return BadRequest(new ApiError(400, "Year must be a positive integer."));}
        if (string.IsNullOrWhiteSpace(countryCodes)) { return BadRequest(new ApiError(400, "Minimum one countryCode necessary."));}
        
        var codes = countryCodes.Split(new[] { ',', ';' });
        if (codes.Length == 0) { return BadRequest(new ApiError(400, "At least one valid country code must be provided.")); }

        try
        {
            var holidayCounts = await _holidayService.GetNonWeekendCountsAsync(year, codes);
            return Ok(holidayCounts);
        } catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new ApiError(500, ex.Message));
        }
    }

    /// <summary>
    /// Given a year and 2 country codes(A and B), this endpoint returns the deduplicated list of dates celebrated in both
    /// countries (date + local names) from the database.
    /// </summary>
    /// <param name="year">Calendar year to evaluate</param>
    /// <param name="countryCodeA">First ISO 3166-1 alpha-2 code</param>
    /// <param name="countryCodeB">Second ISO 3166-1 alpha-2 code</param>
    /// <returns>
    /// <response code="200">Returns the common holidays.</response>
    /// <response code="400">Returns bad request when input is not postiive integer, countries are not inputted or countries are identical</response>
    /// <response code="404">No common countries found</response>
    /// <response code="500">Unexpected server error.</response>
    /// </returns>
    [HttpGet("common/{year}/{countryCodeA}/{countryCodeB}")]
    public async Task<IActionResult> GetCommonHolidays(
        int year,
        string countryCodeA,
        string countryCodeB)
    {
        if (year < 1) { return BadRequest(new ApiError(400, "Year must be a positive integer.")); }
        if (string.IsNullOrWhiteSpace(countryCodeA) || string.IsNullOrWhiteSpace(countryCodeB)) { return BadRequest(new ApiError(400, "Both country codes necessary")); }
        if (string.Equals(countryCodeA, countryCodeB, StringComparison.OrdinalIgnoreCase)) { return BadRequest(new ApiError(400, "Country codes must be different")); }

        try
        {
            var common = await _holidayService.GetCommonHolidaysAsync(year, countryCodeA, countryCodeB);
            if (common == null || !common.Any())
            {
                return NotFound(new ApiError(404, "No overlapping day was found"));
            }

            return Ok(common);
        } catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new ApiError(500, ex.Message));
        }
    }
}