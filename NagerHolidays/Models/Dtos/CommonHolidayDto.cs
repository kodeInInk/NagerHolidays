namespace NagerHolidays.Models.DTOs;

public class CommonHolidayDto
{
    public DateOnly Date {get;set;}
    public Dictionary<string, List<string>> LocalNames {get;set;} = new();
}