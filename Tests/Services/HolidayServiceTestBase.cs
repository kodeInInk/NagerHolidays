using Microsoft.EntityFrameworkCore;
using NagerHolidays.Data;
using NagerHolidays.Models;
using NagerHolidays.Services;

namespace Tests.Services;


///creates a fresh in-memory database per test class instance and seeds simple data
public abstract class HolidayServiceTestBase : IDisposable
{
    protected readonly HolidayDbContext Context;
    protected readonly HolidayService Service;

    //fixed reference date so tests never break as real life calendar time advances
    protected static readonly DateTime AsOfDateTime = new(2025, 6, 15);
    protected static readonly DateOnly AsOfDate = new(2025, 6, 15);

    protected HolidayServiceTestBase()
    {
        var options = new DbContextOptionsBuilder<HolidayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // fresh DB per class instance
            .Options;

        Context = new HolidayDbContext(options);
        Service = new HolidayService(Context);

        Seed();
    }

    public void Dispose() => Context.Dispose();

    // Mock data
    
    // NL: 5 holidays in 2025 — mix of past/future and weekday/weekend
    //   Jan  1 Wed  New Year's Day
    //   Apr 21 Mon  Easter Monday
    //   Apr 27 Sun  King's Day => weekend
    //   May  5 Mon  Liberation Day
    //   Dec 25 Thu  Christmas => future (after AsOfDate)
    //
    // RO: 5 holidays in 2025
    //   Jan  1 Wed  New Year's Day
    //   Jan  2 Thu  New Year's Day 2
    //   Apr 20 Sun  Easter => weekend
    //   Apr 21 Mon  Easter Monday
    //   May  1 Thu  Labour Day
    //
    // HU: 4 holidays in 2025
    //   Jan  1 Wed  New Year's Day => shared with RO
    //   Mar 15 Sat  National Day =>weekend
    //   Apr 21 Mon  Easter Monday =>shared with RO
    //   May  1 Thu  Labour Day  => shared with RO
    //
    // RO & HU (by date): Jan 1, Apr 21, May 1
    // NL & HU (by date): Jan 1, Apr 21   (NL doesn;t have labour day)
    private void Seed()
    {
        var nl = Add(new Country { CountryCode = "NL", Name = "Netherlands" });
        var ro = Add(new Country { CountryCode = "RO", Name = "Romania" });
        var hu = Add(new Country { CountryCode = "HU", Name = "Hungary" });

        // NL
        Holiday(nl, 2025, 1,  1,  "New Year's Day",  "Nieuwjaarsdag");
        Holiday(nl, 2025, 4,  21, "Easter Monday","Tweede Paasdag");
        Holiday(nl, 2025, 4,  27, "King's Day", "Koningsdag");
        Holiday(nl, 2025, 5,  5,  "Liberation Day","Bevrijdingsdag");
        Holiday(nl, 2025, 12, 25, "Christmas Day", "Eerste Kerstdag");

        // RO
        Holiday(ro, 2025, 1,  1,  "New Year's Day","Anul Nou");
        Holiday(ro, 2025, 1,  2,  "New Year's Day 2", "A doua zi de Anul Nou");
        Holiday(ro, 2025, 4,  20, "Easter","Paștele");
        Holiday(ro, 2025, 4,  21, "Easter Monday", "Paștele");
        Holiday(ro, 2025, 5,  1,  "Labour Day", "Ziua Muncii");

        // HU
        Holiday(hu, 2025, 1,  1,  "New Year's Day","Újév");
        Holiday(hu, 2025, 3,  15, "National Day", "Nemzeti ünnep");
        Holiday(hu, 2025, 4,  21, "Easter Monday","Húsvéthétfő");
        Holiday(hu, 2025, 5,  1,  "Labour Day","A munka ünnepe");

        Context.SaveChanges();
    }

    private Country Add(Country country)
    {
        Context.Countries.Add(country);
        return country;
    }

    /// <summary>Adds a holiday directly to the context without saving.</summary>
    protected void Holiday(Country country, int year, int month, int day,
        string name, string localName, bool global = true, string? counties = null)
    {
        Context.Holidays.Add(new Holiday
        {
            Date = new DateOnly(year, month, day),
            Name = name,
            LocalName= localName,
            CountryCode = country.CountryCode,
            CountryId= country.Id,
            Year  = year,
            Fixed = true,
            Global= global,
            Counties = counties,
            Types = "Public",
        });
    }
}
