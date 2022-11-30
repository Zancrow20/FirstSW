namespace HttpServer.Models;

public record class User_ID
{
    public int User_Id { get; init; } // not null check(user_id > 0),
    public string SaltedPassword { get; init; } // not null check(password <> ''),
    public string Salt { get; set; }
    public string User_Nickname { get; init; } // not null unique check(user_nickname <> ''),
    public string? Status { get; set; } // default null,
    public string? Phone_Number { get; set; } // not null check(phone_number <> ''),
    public string? Email { get; set; } // not null check(email <> ''),
    public string? Favourite_Genre { get; set; } // not null default 'Сенён',
}