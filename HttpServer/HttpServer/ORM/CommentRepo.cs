using HttpServer.Models;

namespace HttpServer.ORM;

public class CommentRepo : IRepository<Comment>
{
    private static readonly ORM DB = new
        (@"Data Source=DESKTOP-Q9MBLGB\SQLEXPRESS;Initial Catalog=AnimeWebsite;Integrated Security=True");

    public async Task<Comment?> GetById(Comment comment)
        => await DB.Select<Comment>(comment.Id);

    public async Task<int> Create(Comment entity)
        => await DB.InsertWithId(entity);

    public async Task<int> Update(Comment entity)
        => await DB.Update(entity);

    public async Task<int> Delete(Comment entity)
        => await DB.Delete(entity);

    public async Task<IEnumerable<Comment>> GetEntities()
        => await DB.Select<Comment>();

    public async Task<Comment> GetEntityByProperties(Comment comment)
    {
        throw new NotImplementedException();
    }
}