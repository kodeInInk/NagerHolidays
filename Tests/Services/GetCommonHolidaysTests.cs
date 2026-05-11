using Xunit;

namespace Tests.Services;

public class GetCommonHolidaysTests : HolidayServiceTestBase
{
    //RO & HU shared dates: Jan 1, Apr 21, May 1
    [Fact]
    public async Task ReturnsOnlyDatesSharedByBothCountries()
    {
        var result = (await Service.GetCommonHolidaysAsync(2025, "RO", "HU")).ToList();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, h => h.Date == new DateOnly(2025, 1, 1));
        Assert.Contains(result, h => h.Date == new DateOnly(2025, 4, 21));
        Assert.Contains(result, h => h.Date == new DateOnly(2025, 5, 1));
    }

    [Fact]
    public async Task IncludesLocalNamesForBothCountries()
    {
        var result = (await Service.GetCommonHolidaysAsync(2025, "RO", "HU")).ToList();
        var newYear = result.Single(h => h.Date == new DateOnly(2025, 1, 1));

        Assert.Contains("Anul Nou", newYear.LocalNames["RO"]);
        Assert.Contains("Újév", newYear.LocalNames["HU"]);
    }

    [Fact]
    public async Task ReturnedInAscendingDateOrder()
    {
        var result = (await Service.GetCommonHolidaysAsync(2025, "RO", "HU")).ToList();

        for (int i = 1; i < result.Count; i++)
            Assert.True(result[i - 1].Date <= result[i].Date);
    }

    [Fact]
    public async Task IsCaseInsensitive()
    {
        var lower = (await Service.GetCommonHolidaysAsync(2025, "ro", "hu")).ToList();
        var upper = (await Service.GetCommonHolidaysAsync(2025, "RO", "HU")).ToList();

        Assert.Equal(lower.Count, upper.Count);
    }

    [Fact]
    public async Task ReturnsEmpty_When_YearHasNoData()
    {
        // nothing seeded for 2020
        var result = await Service.GetCommonHolidaysAsync(2020, "RO", "HU");

        Assert.Empty(result);
    }
}
