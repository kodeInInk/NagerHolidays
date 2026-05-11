namespace NagerHolidays.Models.DTOs;

//GET /api/v3/AvailableCountries
public class NagerCountryDto
{
    public string CountryCode { get; set; } = null!;
    public string Name { get; set; } = null!;
}