using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NagerHolidays.Data;
using NagerHolidays.Models;
using NagerHolidays.Repositories;
using NagerHolidays.Services;

namespace NagerHolidays;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        //services
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            //when getting countries, there was a circular dependency issue=> ignore back-references
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });
        
        var allowedOrigins = builder.Configuration["AllowedOrigins"] ?? "*";
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
            });
        });

        builder.Services.AddOpenApi();  //https://aka.ms/aspnet/openapi
        //MSSQL via EF Core
        builder.Services.AddDbContext<HolidayDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                o => o.CommandTimeout(180));
        });
        builder.Services.AddHttpClient<INagerDateApiClient, NagerDateApiClient>(client =>
        {
            var baseUrl = builder.Configuration["NagerDateApi:BaseUrl"] ?? "https://date.nager.at/api/v3/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        //swagger
        builder.Services.AddSwaggerGen(x =>
        {
            x.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Holiday Assessment API",
                Version = "v1",
                Description = "Assessment API that retrieves data from date.nager.at, persists it to MSSQL via EF Core, and exposes the 3 required queries."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            // Include XML comments
            x.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        });
        
        //repositories
        builder.Services.AddTransient(typeof(HolidayDbContext));
        builder.Services.AddTransient<IBaseRepository<Country>, CountryRepository>();
        builder.Services.AddTransient<IBaseRepository<Holiday>, HolidayRepository>();
        builder.Services.AddTransient<IHolidayService, HolidayService>();

        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        //db migration and seeding
        await app.MigrateAndPopulateDatabaseAsync();
        //https request pipeline
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors();
        app.UseAuthorization();
        
        app.MapControllers();
        
        app.Run();
    }
}