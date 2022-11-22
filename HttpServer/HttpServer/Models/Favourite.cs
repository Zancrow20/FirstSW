namespace HttpServer.Models;

public record class Favourite
{
    public int User_Id { get; init; } // not null check(user_id > 0),
    public int Anime_Id { get; init; } // not null check(anime_id > 0),
    public int? Estimation { get; set; } // not null default null check(estimation between 1 and 5),
    public string? Status_Of_Watching { get; set; } // default null
    // check(status_of_watching in('Смотрю', 'Просмотрено','В планах', 'Брошено')),
    //TODO: нужно ли это поле public string? Comment { get; set; } //default null
}