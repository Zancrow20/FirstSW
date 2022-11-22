namespace HttpServer.Models;

public record class AnimeInfo
{
    public int Anime_Id { get; init; } //not null identity(100,1),
    public string? Author { get; set; } //
    public string? Studio { get; set; } //
    public string? Genre { get; set; } //not null default 'Сенён'
    public DateTime? Date_Of_Release { get; set; } //not null check(date_of_release > '01-01-1907'),
    public bool Is_Ongoing { get; set; } //Или всё же bite?
    public string? Type { get; set; } //check(type in('S', 'F')),--F - film, S - series
    public int Count_Of_Episodes { get; set; } //default 0 check(count_of_episodes between 0 and 100000),
}