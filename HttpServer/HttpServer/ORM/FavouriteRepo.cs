using HttpServer.Models;

namespace HttpServer.ORM;

public class FavouriteRepo : IRepository<Favourite>
{
    private static readonly ORM DB = new
        (@"Data Source=DESKTOP-Q9MBLGB\SQLEXPRESS;Initial Catalog=AnimeWebsite;Integrated Security=True");

    public Task<Favourite?> GetById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<int> Create(Favourite entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> Update(Favourite entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> Delete(Favourite entity)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Favourite>> GetEntities()
    {
        throw new NotImplementedException();
    }

    public Task<Favourite> GetEntityByProperties(params object[] properties)
    {
        throw new NotImplementedException();
    }
}