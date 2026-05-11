using Microsoft.EntityFrameworkCore;
using NagerHolidays.Data;
using NagerHolidays.Models;
using NagerHolidays.Models.DTOs;

namespace NagerHolidays.Repositories;

public class HolidayRepository: IBaseRepository<Holiday>
{
    private readonly HolidayDbContext _context;

    public HolidayRepository(HolidayDbContext context)
    {
        _context = context;
    }

        
    public async Task<Holiday> AddAsync(Holiday holiday)
    {
        _context.Holidays.Add(holiday);
        await _context.SaveChangesAsync();
        return holiday;
    }

    public async Task<Holiday?> GetByIdAsync(int id)
    {
        return await _context.Holidays.FindAsync(id);
    }

    public async Task<IEnumerable<Holiday>> GetAllAsync()
    {
        return await _context.Set<Holiday>()
            .OrderBy(h => h.Date)
            .ToArrayAsync();
    }

    public async Task<Holiday> UpdateAsync(int id, Holiday newHoliday)
    {
        var exHoliday = await GetByIdAsync(id);
        if (exHoliday == null) return null;
        
        exHoliday.Date = newHoliday.Date;
        exHoliday.Name = newHoliday.Name;
        exHoliday.LocalName = newHoliday.LocalName;
        exHoliday.CountryCode = newHoliday.CountryCode;
        exHoliday.CountryId = newHoliday.CountryId;
        exHoliday.Year = newHoliday.Year;
        exHoliday.Fixed = newHoliday.Fixed;
        exHoliday.Global = newHoliday.Global;
        exHoliday.LaunchYear = newHoliday.LaunchYear;
        exHoliday.Counties = newHoliday.Counties;
        exHoliday.Types = newHoliday.Types;
        
        await _context.SaveChangesAsync();
        return exHoliday;
    }

    public async Task<Holiday> DeleteAsync(int id)
    {
        var exHoliday = await GetByIdAsync(id);
        if(exHoliday == null) return null;
        
        _context.Holidays.Remove(exHoliday);
        await _context.SaveChangesAsync();
        return exHoliday;
    }
}