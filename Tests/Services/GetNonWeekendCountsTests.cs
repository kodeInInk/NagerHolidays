using Xunit;

namespace Tests.Services;

public class GetNonWeekendCountsTests : HolidayServiceTestBase
{
    [Fact]
    public async Task ExcludesWeekendHolidays()
    {
        //NL 2025: King's Day (Apr 27) is Sunday=> exclude
        //remaining weekday holidays: Jan 1 (Wed), Apr 21 (Mon), May 5 (Mon), Dec 25 (Thu) = 4
        var result = (await Service.GetNonWeekendCountsAsync(2025, ["NL"])).ToList();

        Assert.Equal(4, result.Single(r => r.CountryCode == "NL").NonWeekendHolidayCount);
    }

    [Fact]
    public async Task ResultsSortedDescendingByCount()
    {
        var result = (await Service.GetNonWeekendCountsAsync(2025, ["NL", "RO", "HU"])).ToList();

        for (int i = 1; i < result.Count; i++)
            Assert.True(result[i - 1].NonWeekendHolidayCount >= result[i].NonWeekendHolidayCount);
    }

    [Fact]
    public async Task ReturnsZero_When_AllHolidaysFallOnWeekend()
    {
        //country whose only 2025 holiday is a Saturday
        var se = new NagerHolidays.Models.Country { CountryCode = "SE", Name = "Sweden" };
        Context.Countries.Add(se);
        Holiday(se, 2025, 3, 15, "Some Holiday", "Någon helgdag"); // Saturday
        await Context.SaveChangesAsync();

        var result = (await Service.GetNonWeekendCountsAsync(2025, ["SE"])).ToList();

        Assert.Equal(0, result.Single().NonWeekendHolidayCount);
    }

    [Fact]
    public async Task ReturnsZeroCount_ForUnknownCountryCode()
    {
        var result = (await Service.GetNonWeekendCountsAsync(2025, ["NL", "ZZ"])).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(0, result.Single(r => r.CountryCode == "ZZ").NonWeekendHolidayCount);
    }
}
