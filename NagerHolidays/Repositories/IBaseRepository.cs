namespace NagerHolidays.Repositories;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<TEntity> AddAsync(TEntity entity); //C
    Task<TEntity?> GetByIdAsync(int id); //R
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity> UpdateAsync(int id, TEntity entity); //U
    Task<TEntity> DeleteAsync(int id); //D
}