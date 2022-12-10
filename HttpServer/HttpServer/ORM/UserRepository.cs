using HttpServer.Models;

namespace HttpServer.ORM;


public class UserRepository : IRepository<User_ID>
{
    private static readonly ORM DB = new 
        (@"Data Source=DESKTOP-Q9MBLGB\SQLEXPRESS;Initial Catalog=AnimeWebsite;Integrated Security=True");
    
    public async Task<User_ID?> GetById(User_ID user)
        => await DB.Select<User_ID>(user.Id);

    public async Task<int> Create(User_ID user)
        => await DB.Insert(user);
    

    public async Task<int> Update(User_ID user)
        => await DB.Update(user);

    public async Task<int> Delete(User_ID user)
        => await DB.Delete(user);

    public async Task<IEnumerable<User_ID>> GetEntities()
        => await DB.Select<User_ID>();
    
    public async Task<User_ID?> GetEntityByProperties(User_ID user)
    {
        var email = user.Email;
        user = (await DB.Select<User_ID>(email, "email")).FirstOrDefault();
        return user ?? null;
    }
}