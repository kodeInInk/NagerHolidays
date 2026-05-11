using Xunit;

namespace Tests.Services;

public class GetLastCelebratedTests : HolidayServiceTestBase
{
    [Fact]
    public async Task ReturnsExactRequestedAmount()
    {
        var result = (await Service.GetLastCelebratedAsync("NL", 3, AsOfDateTime)).ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task OnlyReturnsPastHolidays()
    {
        var result = await Service.GetLastCelebratedAsync("NL", 10, AsOfDateTime);

        Assert.All(result, h => Assert.True(h.Date <= AsOfDate));
    }

    [Fact]
    public async Task ReturnsMostRecentFirst()
    {
        var result = (await Service.GetLastCelebratedAsync("NL", 10, AsOfDateTime)).ToList();

        for (int i = 1; i < result.Count; i++)
            Assert.True(result[i - 1].Date >= result[i].Date);
    }

    [Fact]
    public async Task IsCaseInsensitive()
    {
        var lower = (await Service.GetLastCelebratedAsync("nl", 10, AsOfDateTime)).ToList();
        var upper = (await Service.GetLastCelebratedAsync("NL", 10, AsOfDateTime)).ToList();

        Assert.Equal(lower.Count, upper.Count);
    }

    [Fact]
    public async Task ReturnsEmpty_ForUnknownCountry()
    {
        var result = await Service.GetLastCelebratedAsync("ZZ", 3, AsOfDateTime);

        Assert.Empty(result);
    }
}
