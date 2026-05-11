namespace NagerHolidays.Models.DTOs;

public class CountryHolidayCountDto
{
    public string CountryName {get; set;} = null;
    public string CountryCode {get; set;} = null;
    public int NonWeekendHolidayCount {get; set;}
}