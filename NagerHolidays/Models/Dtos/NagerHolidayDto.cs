namespace NagerHolidays.Models.DTOs;

//GET /api/v3/PublicHolidays/{year}/{countryCode}
public class NagerHolidayDto
{ 
    //ISO date (yyyy-MM-dd)- mapped via System.Text.Json
    public DateOnly Date {get; set;}
    
    public string LocalName {get; set;} = null;
    public string Name {get; set;} = null!;

    public string CountryCode {get; set;} = null;

    public bool Fixed {get; set;}
    public bool Global {get; set;}
    public List<string>? Counties {get; set;}
    public int? LaunchYear {get; set;}
    public List<string>? Types {get; set;}
}