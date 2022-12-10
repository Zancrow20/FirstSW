namespace HttpServer.Models;

public class User_ID
{
    public int Id { get; init; } // not null check(user_id > 0),
    
    [Password]
    public string SaltedPassword { get; init; } // not null check(password <> ''),
    
    public string Salt { get; set; }
    
    [Nickname]
    public string User_Nickname { get; init; } // not null unique check(user_nickname <> ''),
    
    public string? Status { get; set; } // default null,
    
    [MyPhone]
    public string? Phone_Number { get; set; } // not null check(phone_number <> ''),
    
    [MyEmail]
    public string? Email { get; set; } // not null check(email <> ''),
    
    [Genre]
    public string? Favourite_Genre { get; set; } // not null default 'Сенён',
}