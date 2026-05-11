namespace NagerHolidays.Models.DTOs;

public class HolidaySummaryDto
{
    public DateOnly Date {get; set;}
    public string Name {get; set;} = null;
    public string LocalName {get; set;} = null;
}