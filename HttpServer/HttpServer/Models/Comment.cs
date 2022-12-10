namespace HttpServer.Models;

public record class Comment
{
    public int User_Id { get; set; }
    public int Id { get; set; }
    public int Comment_Id { get; set; }
    public string Comment_Value { get; set; }
    public string Username { get; set; }
}