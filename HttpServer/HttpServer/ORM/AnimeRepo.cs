using HttpServer.Models;

namespace HttpServer.ORM;

public class AnimeRepo : IRepository<Anime>
{
    private static readonly ORM DB = new 
        (@"Data Source=DESKTOP-Q9MBLGB\SQLEXPRESS;Initial Catalog=AnimeWebsite;Integrated Security=True");

    public async Task<Anime?> GetById(Anime anime)
        => await DB.Select<Anime>(anime.Id);

    public async Task<int> Create(Anime entity)
        => await DB.Insert(entity);

    public async Task<int> Update(Anime entity)
        => await DB.Update(entity);

    public async Task<int> Delete(Anime entity)
        => await DB.Delete(entity);

    public async Task<IEnumerable<Anime>> GetEntities()
        => await DB.Select<Anime>();

    public async Task<Anime> GetEntityByProperties(Anime anime)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Anime>> GetByQuery(string query)
        => await DB.ExecuteQuery<Anime>(query);
}