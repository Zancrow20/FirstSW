using HttpServer.Models;

namespace HttpServer.ORM;

public class AnimeInfoRepo : IRepository<AnimeInfo>
{
    private static readonly ORM DB = new
        (@"Data Source=DESKTOP-Q9MBLGB\SQLEXPRESS;Initial Catalog=AnimeWebsite;Integrated Security=True");

    public async Task<AnimeInfo?> GetById(AnimeInfo info)
        => await DB.Select<AnimeInfo>(info.Id);

    public async Task<int> Create(AnimeInfo entity)
        => await DB.Insert(entity);

    public async Task<int> Update(AnimeInfo entity)
        => await DB.Update(entity);

    public async Task<int> Delete(AnimeInfo entity)
        => await DB.Delete(entity);

    public async Task<IEnumerable<AnimeInfo>> GetEntities()
    {
        throw new NotImplementedException();
    }

    public async Task<AnimeInfo> GetEntityByProperties(AnimeInfo animeInfo)
    {
        throw new NotImplementedException();
    }
}