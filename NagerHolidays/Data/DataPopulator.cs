using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using NagerHolidays.Models;
using NagerHolidays.Models.DTOs;
using NagerHolidays.Services;

namespace NagerHolidays.Data;


// the parallel-fetch shape (bounded concurrency) follows
// Stephen Toub's "Parallel.ForEachAsync in .NET 6" post:
// https://devblogs.microsoft.com/dotnet/parallel-foreach-async-in-net-6/
// thread-safe accumulator (ConcurrentBag) per the thread-safe collections guide:
// https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/when-to-use-a-thread-safe-collection
public class DataPopulator
{
    private const int FromYear = 1976;
    private const int ToYear = 2076;
    private const int Parallelism = 8;

    public static async Task PopulateAsync(
        HolidayDbContext context,
        INagerDateApiClient nager,
        IConfiguration config,
        ILogger<DataPopulator> logger)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var resetFlag = config["PopulateData:ResetDb"] == "true";

        // if db already has data and reset flag == false/not set, skip population
        if (await context.Holidays.AnyAsync() && !resetFlag)
        {
            logger.LogInformation("Database not empty, skipping population");
            return;
        }
        // otherwise if reset requested, truncate tables first
        if (resetFlag)
        {
            await ResetDatabaseAsync(context, logger);
        }
        
        
        //step 1: grab countries from NagerAPI
        var nagerCountries = await nager.GetAvailableCountriesAsync();
        if (nagerCountries.Count() == 0)
        {
            logger.LogInformation("NagerAPI returned 0 countries. Skipping population");
            return;
        }
        var countries = await UpsertCountriesAsync(context, nagerCountries, logger);
        
        //step 2: figure out year range+parallelism from config
        var fromYear = config.GetValue<int?>("PopulateData:FromYear") ?? FromYear;
        var toYear= config.GetValue<int?>("PopulateData:ToYear") ?? ToYear;
        if(fromYear > toYear) (fromYear, toYear) = (fromYear, toYear);
        
        var parralelism = config.GetValue<int?>("PopulateData:Parallelism") ?? Parallelism;
        if(parralelism < 1) parralelism = 1;
        logger.LogInformation("Seeding holidays for {Count} countries, years {From}-{To}, parallelism={P}",
            countries.Count, fromYear, toYear, parralelism);
        
        //step 3: build the year code job list
        var jobs =
            from y in Enumerable.Range(fromYear, toYear - fromYear + 1)
            from c in countries.Values
            select (year: y, code: c.CountryCode);
        var fetched = new ConcurrentBag<Holiday>();
        
        // Parallel.ForEachAsync gives bounded async concurrency
        // we be making call the HTTP client (thread-safe via IHttpClientFactory) and write to the bag
        await Parallel.ForEachAsync(
            jobs,
            new ParallelOptions { MaxDegreeOfParallelism = parralelism },
            async (job, ct) =>
            {
                var dtos = await nager.GetPublicHolidaysAsync(job.year, job.code, ct);
                if (dtos.Count == 0) return;

                var country = countries[job.code];
                foreach (var dto in dtos)
                {
                    fetched.Add(Map2Entity(dto, country));
                }
            });

        if (fetched.IsEmpty)
        {
            logger.LogWarning("No holidays fetched for any country/year. Nothing to insert");
            return;
        }
        
        await context.Holidays.AddRangeAsync(fetched);
        await context.SaveChangesAsync();

        sw.Stop();
        logger.LogInformation("Population finished with {Count} holidays in {ElapsedMilliseconds}ms", fetched.Count, sw.ElapsedMilliseconds);
    }
    
    
    
    
    private static async Task ResetDatabaseAsync(HolidayDbContext db, ILogger logger)
    {
        logger.LogInformation("Truncating tables before seeding...");
        //holidays first (FK -> countries), then countries
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Holidays;");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Countries;");
        //keep ids starting from 1
        await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Holidays', RESEED, 0);");
        await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Countries', RESEED, 0);");
    }
    
    private static async Task<Dictionary<string, Country>> UpsertCountriesAsync(
        HolidayDbContext context,
        IEnumerable<NagerCountryDto> nagerCountries,
        ILogger logger)
    {
        // load whatever is already in the db so we can detect existing rows by code
        var existing = await context.Countries.ToDictionaryAsync(c => c.CountryCode);

        var added = 0;
        foreach (var nc in nagerCountries)
        {
            if (existing.TryGetValue(nc.CountryCode, out var current))
            {
                if (current.Name != nc.Name) current.Name = nc.Name;
                continue;
            }

            var c = new Country { CountryCode = nc.CountryCode, Name = nc.Name };
            context.Countries.Add(c);
            existing[nc.CountryCode] = c;
            added++;
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Added countries: {Added} new, {Total} total", added, existing.Count);
        return existing;
    }
    
    private static Holiday Map2Entity(NagerHolidayDto dto, Country country)
    {
        // join collections with comma so we don't need a separate table for them
        string? counties = null;
        if (dto.Counties != null && dto.Counties.Count > 0)
        {
            counties = string.Join(",", dto.Counties);
        }

        var types = "Public";
        if (dto.Types != null && dto.Types.Count > 0)
        {
            types = string.Join(",", dto.Types);
        }
        return new Holiday
        {
            Date = dto.Date,
            Name = dto.Name,
            LocalName = dto.LocalName,
            CountryCode = dto.CountryCode,
            CountryId = country.Id,
            Year = dto.Date.Year,
            Fixed = dto.Fixed,
            Global = dto.Global,
            LaunchYear = dto.LaunchYear,
            Counties = counties,
            Types = types
        };
    }
}