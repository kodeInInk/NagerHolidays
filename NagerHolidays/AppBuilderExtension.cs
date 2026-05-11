using Microsoft.EntityFrameworkCore;
using NagerHolidays.Data;
using NagerHolidays.Services;

namespace NagerHolidays;

public static class AppBuilderExtension
{
    public static async Task MigrateAndPopulateDatabaseAsync(this WebApplication app, bool populateDatabase = true)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        var context = services.GetRequiredService<HolidayDbContext>();
        var nager = services.GetRequiredService<INagerDateApiClient>();
        var config = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<DataPopulator>>();
        
        //migrate db to get schema in place
        var definedMigrations = context.Database.GetMigrations().ToList();
        if (definedMigrations.Count > 0)
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }
        // populate the db with data from the Nager API
        if (populateDatabase)
        {
            await DataPopulator.PopulateAsync(context, nager, config, logger);
        }
    }
}