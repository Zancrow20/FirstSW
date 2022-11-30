namespace HttpServer.Models;

public class Session
{
    public int UserId { get; }
    public Guid Guid { get; set; }
    public string Email { get; }
    public DateTime CreateDateTime { get; }

    public Session(Guid guid,int userId, string email)
    {
        Guid = guid;
        UserId = userId;
        Email = email;
        CreateDateTime = DateTime.Now;
    }
}