using System.Net;
using System.Text;
using HttpServer.Attributes;
using HttpServer.Models;
using HttpServer.Models.DI;
using HttpServer.ORM;
using Scriban;

namespace HttpServer.Controllers;

[HttpController("comment")]

public class Comment
{
    private CommentRepo _commentRepo = new CommentRepo();
    private UserRepository _userRepo = new UserRepository();
    [HttpPOST("ajax")]
    
    public async Task<IActionResult> CreateCommment(HttpListenerContext context, string input)
    {
        try
        {
            input = input.Replace("+", " ");
            const string comment = $"<div class=\"anime__review__item\">"
                                   + $"<div class=\"anime__review__item__pic\">"
                                   + $"<img src=\"img/anime/review-6.jpg\" alt=\"\">"
                                   + $"</div>"
                                   + $"<div class=\"anime__review__item__text\">"
                                   + "<h6>{{Nickname}} - <span>20 Hour ago</span></h6>"
                                   + "<p>{{Input}}</p>"
                                   + $"</div>"
                                   + $"</div>";
            var animeId = int.Parse(context.Request.UrlReferrer.Segments[3].Replace("/", ""));
            if (IsAuthorized(context, out var guid))
            {
                var id = (await SessionManager.GetInfo(guid))!.UserId;
                var user = await _userRepo.GetById(new User_ID() {Id = id});
                await _commentRepo.Create(new Models.Comment()
                {
                    Comment_Id = new Random().Next(1,10000000),
                    Comment_Value = input,
                    User_Id = user.Id,
                    Username = user.User_Nickname,
                    Id = animeId
                });
                
                var authTpl = Template.Parse(comment);
                var authRes = await authTpl.RenderAsync(new { Nickname = user.User_Nickname, Input = input } , member => member.Name);
                return new HtmlCode
                {
                    Buffer = Encoding.ASCII.GetBytes(authRes),
                    StatusCode = HttpStatusCode.OK,
                    ContentType = "text/html"
                };
            }
            
            await _commentRepo.Create(new Models.Comment()
            {
                Comment_Id = new Random().Next(1,10000000),
                Comment_Value = input,
                Username = "Anonim",
                Id = animeId
            });
            
            var tpl = Template.Parse(comment);
            var res = await tpl.RenderAsync(new { Nickname = "Anonim", Input = input } , member => member.Name);
            return new HtmlCode
            {
                Buffer = Encoding.ASCII.GetBytes(res),
                StatusCode = HttpStatusCode.OK,
                ContentType = "text/html"
            };
        }
        catch (Exception)
        {
            return CreateActionResult("404 not found", HttpStatusCode.NotFound,"text/html");
        }
    }
    
    private bool IsAuthorized(HttpListenerContext context,out Guid guid)
        => Guid.TryParse(context.Request.Cookies["SessionId"]?.Value.Split('=')[^1], out guid);
    
    private IActionResult CreateActionResult(string val, HttpStatusCode statusCode, string contentType)
    {
        return new HtmlCode
        {
            Buffer = Encoding.ASCII.GetBytes(val),
            StatusCode = statusCode,
            ContentType = contentType
        };
    }
}