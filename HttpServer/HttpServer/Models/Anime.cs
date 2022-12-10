using System.ComponentModel.DataAnnotations;

namespace HttpServer.Models;

public record class Anime
{
    [Required]
    public int Id { get; init; } // not null identity(100,1),
    [Required]
    public string Anime_Name { get; set; } // not null check(anime_name <> ''),
    [Required]
    public int? Count_Of_Estimations { get; set; } // default 0 check(count_of_estimations >= 0),
    [Required(AllowEmptyStrings = true)]
    public decimal Average_Rating { get; set; } // default null
    // check(average_rating between 1 and 5),--3 - всего чисел, 2 - после запятой
    public bool Is_Ongoing { get; set; }
    
    public string Image_Path { get; set; }
}