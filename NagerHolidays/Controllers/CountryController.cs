using NagerHolidays.Models;
using NagerHolidays.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace NagerHolidays.Controllers;

[Route("country")]
[ApiController]
public class CountryController(
    ILogger<CountryController> logger,
    IBaseRepository<Country> countryRepository
) : BaseController<Country, IBaseRepository<Country>, CountryController>(countryRepository, logger)
{

}
