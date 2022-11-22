namespace HttpServer.Models;

public class Friend
{
    public int User_Id { get; set; } //not null check(user_id > 0),
    public int Friend_Id { get; set; } //int not null check(friend_id > 0),
    public int Common_Anime { get; set; } //int not null,
    public DateTime Date_Of_Friendship { get; set; }//date not null,
    public string Status_Of_Friendship { get; set; }
    //varchar(25) not null check(status_of_friendship in('Друзья навеки', 'ДЖОДЖО Братаны', 'Гули', 'Друзья по аниме', 'Знакомые')),

}