namespace HttpServer.ORM;

public class AnimeInfoRepo : IRepository<AnimeInfoRepo>
{
    private static readonly ORM DB = new
        (@"Data Source=DESKTOP-Q9MBLGB\SQLEXPRESS;Initial Catalog=AnimeWebsite;Integrated Security=True");

    public Task<AnimeInfoRepo?> GetById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<int> Create(AnimeInfoRepo entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> Update(AnimeInfoRepo entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> Delete(AnimeInfoRepo entity)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AnimeInfoRepo>> GetEntities()
    {
        throw new NotImplementedException();
    }

    public Task<AnimeInfoRepo> GetEntityByProperties(params object[] properties)
    {
        throw new NotImplementedException();
    }
}