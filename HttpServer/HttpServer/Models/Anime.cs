namespace HttpServer.Models;

public record class Anime
{
    public int Anime_Id { get; init; } // not null identity(100,1),
    public string Anime_Name { get; init; } // not null check(anime_name <> ''),
    public string? Description { get; set; } // not null default 'Описание не указано',
    public int? Count_Of_Estimations { get; set; } // default 0 check(count_of_estimations >= 0),
    public decimal Average_Rating { get; set; } // default null
    // check(average_rating between 1 and 5),--3 - всего чисел, 2 - после запятой
}