namespace NagerHolidays.Models;

public class Holiday: BaseEntity
{
    public DateOnly Date {get; set;}
    public string Name {get; set;} = null;
    public string LocalName {get; set;} = null;
    
    //fk+navigation  back to country, with duplicated code so q filter w/o join
    public int CountryId {get; set;}
    public Country? Country {get; set;}
    public string CountryCode {get; set;} = null;
    
    public int Year {get; set;}
    public bool Fixed {get; set;}
    public bool Global {get; set;}
    public int? LaunchYear {get; set;}
    public string? Counties {get; set;} //comma joined + null when global
    public string Types {get; set;} = null; //joined comma separated
    
}