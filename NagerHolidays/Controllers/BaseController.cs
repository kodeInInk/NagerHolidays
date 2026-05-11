using Microsoft.AspNetCore.Mvc;
using NagerHolidays.Models;
using NagerHolidays.Repositories;

namespace NagerHolidays.Controllers;

[Produces("application/json")]
public abstract class BaseController<TEntity, TRepository, TController> : ControllerBase
    where TEntity: BaseEntity
    where TRepository: IBaseRepository<TEntity>
{
    protected ILogger<TController> _logger {get;}
    protected TRepository _repository {get;}

    protected BaseController(TRepository repository, ILogger<TController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    ///Retrieves an entity from the database by its integer ID.
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Returns the requested entity.</response>
    /// <response code="400">Invalid id.</response>
    /// <response code="404">No entity found for given id.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Get(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new ApiError(400, "Invalid id"));
        }

        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound(new ApiError(404, "Entity not found"));
            }

            return Ok(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal server error getting {Entity} with id {Id}", typeof(TEntity).Name, id);
            return StatusCode(500, new ApiError(500, ex.Message));
        }
    }

    /// <summary>
    /// Retrieves all entities.
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Returns the list of entities.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet]
    public virtual async Task<IActionResult> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        return Ok(entities);
    }
}