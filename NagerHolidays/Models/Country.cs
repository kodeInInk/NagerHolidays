namespace NagerHolidays.Models;

public class Country: BaseEntity
{
    //ISO 3166-1 alpha-2 code(= "NL"/"RO"/...)
    public string CountryCode { get; set; } = null;
    public string Name { get; set; } = null;
    public List<Holiday> Holidays { get; set; } = [];

}