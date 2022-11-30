using HttpServer.Models;

namespace HttpServer.ORM;

public class CommentRepo : IRepository<Comment>
{
    private static readonly ORM DB = new
        (@"Data Source=DESKTOP-Q9MBLGB\SQLEXPRESS;Initial Catalog=AnimeWebsite;Integrated Security=True");

    public Task<Comment?> GetById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<int> Create(Comment entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> Update(Comment entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> Delete(Comment entity)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Comment>> GetEntities()
    {
        throw new NotImplementedException();
    }

    public Task<Comment> GetEntityByProperties(params object[] properties)
    {
        throw new NotImplementedException();
    }
}