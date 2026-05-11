using Microsoft.EntityFrameworkCore;
using NagerHolidays.Data;
using NagerHolidays.Models;

namespace NagerHolidays.Repositories;

public class CountryRepository: IBaseRepository<Country>
{
    private readonly HolidayDbContext _context;

    public CountryRepository(HolidayDbContext context)
    {
        _context = context;
    }

    public async Task<Country?> GetByIdAsync(int id)
    {
        return await _context.Countries
            .Include(c => c.Holidays)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Country>> GetAllAsync()
    {
        return await _context.Set<Country>()
            .Include(c => c.Holidays)
            .OrderBy(c => c.CountryCode)
            .ToArrayAsync();
    }

    public async Task<Country> AddAsync(Country country)
    {
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();
        return country;
    }
    
    //UD not implemented cause: they are momentarily not necessary, at least in current veersion of controllers 
    public Task<Country> UpdateAsync(int id, Country country)
    {
        throw new NotImplementedException();
    }

    public Task<Country> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}