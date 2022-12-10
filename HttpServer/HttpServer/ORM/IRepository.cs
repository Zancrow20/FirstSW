using HttpServer.Models;

namespace HttpServer.ORM;

public interface IRepository<TEntity>
{
    Task<TEntity?> GetById(TEntity entity);
    Task<int> Create(TEntity entity);
    Task<int> Update(TEntity entity);
    Task<int> Delete(TEntity entity);
    Task<IEnumerable<TEntity>> GetEntities();
    Task<TEntity> GetEntityByProperties(TEntity entity);
}