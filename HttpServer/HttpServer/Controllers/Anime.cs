using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using HttpServer.Attributes;
using Scriban;
using HttpServer.Models;
using HttpServer.Models.DI;
using HttpServer.ORM;

namespace HttpServer.Controllers;

[HttpController("anime")]
public class Anime
{
    //TODO В принципе тут осталось сверстать страницы и 
    private AnimeRepo _animeRepo = new ();
    private AnimeInfoRepo _animeInfoRepo = new();
    private CommentRepo _commentRepo = new();
    
    [HttpGET("all")]
    public async Task<IActionResult> GetAllAnime(HttpListenerContext context)
    {
        try
        {
            var animeSeries = await _animeRepo.GetEntities();
            const string path = "../../../Views/AllAnime.html";
            var data = await File.ReadAllTextAsync(path);

            var tpl = Template.Parse(data);
            var res = await tpl.RenderAsync(new { AnimeSeries = animeSeries}, member => member.Name);
            return new HtmlCode
            {
                Buffer = Encoding.ASCII.GetBytes(res),
                StatusCode = HttpStatusCode.OK,
                ContentType = "text/html"
            };
        }
        catch (Exception)
        {
            return new HtmlCode
            {
                Buffer = Encoding.ASCII.GetBytes("All anime Error"),
                StatusCode = HttpStatusCode.NotFound,
                ContentType = "text/html"
            };
        }
    }
    
    [HttpGET("anime-page")]
    public async Task<IActionResult> GetAnimePage(HttpListenerContext context)
    {
        
        //TODO разобраться с видеоплеером, почему не воспроизводит
        try
        {
            if(!int.TryParse(context.Request.RawUrl?.Split('/', StringSplitOptions.RemoveEmptyEntries)[^1], out var id))
                throw new Exception("Anime page exception");
            var anime = await _animeRepo.GetById(new Models.Anime{Id = id});
            var animeInfo = await _animeInfoRepo.GetById(new AnimeInfo(){Id = anime.Id});
            var comments = (await _commentRepo.GetEntities()).Where(comment => comment.Id == anime.Id);
            
            
            const string path = "../../../Views/Anime-Page.html";
            var data = await File.ReadAllTextAsync(path);
        
            var tpl = Template.Parse(data);
            var res = await tpl.RenderAsync(new { Anime = anime, Info = animeInfo, Comments = comments}, member => member.Name);
            return new HtmlCode
            {
                Buffer = Encoding.ASCII.GetBytes(res),
                StatusCode = HttpStatusCode.OK,
                ContentType = "text/html"
            };
        }
        catch (Exception)
        {
            return CreateActionResult("Anime Page Error", HttpStatusCode.NotFound, "text/html");
        }
        
    }
    
    [HttpGET("random")]
    public async Task<IActionResult> GetRandomAnimePage(HttpListenerContext context)
    {
        try
        {
            var series = await _animeRepo.GetEntities();
            var minId = series.Min(anime => anime.Id);
            var maxId = series.Max(anime => anime.Id);
            var rnd = new Random();
            var id = rnd.Next(minId, maxId);
            var anime = series.FirstOrDefault(anime => anime.Id == id);
            var animeInfo = await _animeInfoRepo.GetById(new AnimeInfo {Id = anime.Id});
            var comments = (await _commentRepo.GetEntities()).Where(comment => comment.Id == anime.Id);

            return await CreateAnimePageHtml(anime, animeInfo, comments);
        }
        catch (Exception)
        {
            return CreateActionResult("Random Anime Page Error", HttpStatusCode.NotFound, "text/html");
        }
    }
    
    [HttpGET("best")]
    public async Task<IActionResult> GetBest(HttpListenerContext context)
    {
        try
        {
            var best = (await _animeRepo.GetEntities()).OrderBy(anime => anime.Average_Rating)
                .Take(10);
            return await CreateAnimeHtml(best, "Best");
        }
        catch (Exception)
        {
            return CreateActionResult("Best Error", HttpStatusCode.NotFound, "text/html");
        }
    }

    [HttpGET("ongoing")]
    public async Task<IActionResult> GetOngoing(HttpListenerContext context)
    {
        try
        {
            var ongoing = (await _animeRepo.GetEntities()).Where(anime => anime.Is_Ongoing);
            return await CreateAnimeHtml(ongoing, "Ongoing");
        }
        catch (Exception)
        {
            return CreateActionResult("Ongoing Error", HttpStatusCode.NotFound, "text/html");
        }
    }

    [HttpGET("completed")]
    public async Task<IActionResult> GetCompleted(HttpListenerContext context)
    {
        try
        {
            var completed = (await _animeRepo.GetEntities()).Where(anime => !anime.Is_Ongoing);
            return await CreateAnimeHtml(completed, "Completed");
        }
        catch (Exception)
        {
            return CreateActionResult("Completed Error", HttpStatusCode.NotFound, "text/html");
        }
    }
    
    [HttpPOST("ajax")]
    
    public async Task<IActionResult> Search(HttpListenerContext context, string input)
    {
        try
        {
            var res = "";
            if (input != string.Empty)
            {
                var query = $"select * from Anime where anime_name like '{input}%'";
                var _records = await _animeRepo.GetByQuery(query);
                const string searchData = "<table>" +
                                            "{{- for anime_record in records}}" +
                                                "<tr>" +
                                                    "<td class=\"search_result-name\">" +
                                                        "<a href=\"/anime/anime-page/{{anime_record.Id}}/\" class=\"search_result-ref\">" +
                                                            "{{anime_record.Anime_Name}}" +
                                                        "</a>" +
                                                    "</td>" +
                                                "</tr>" +
                                            "{{- end }}" +
                                            "</table>";
                var tpl = Template.Parse(searchData);
                res = await tpl.RenderAsync(new { records = _records } , member => member.Name);
            }
            
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

    private IActionResult CreateActionResult(string val, HttpStatusCode statusCode, string contentType)
    {
        return new HtmlCode
        {
            Buffer = Encoding.ASCII.GetBytes(val),
            StatusCode = statusCode,
            ContentType = contentType
        };
    }

    private async Task<IActionResult> CreateAnimeHtml(IEnumerable<Models.Anime> series, string status)
    {
        const string path = "../../../Views/pagesTemp.html";
        var data = await File.ReadAllTextAsync(path);
        
        var tpl = Template.Parse(data);
        var res = tpl.RenderAsync(new {Status = status, Series = series}, member => member.Name);
        return new HtmlCode
        {
            Buffer = Encoding.ASCII.GetBytes(await res),
            StatusCode = HttpStatusCode.OK,
            ContentType = "text/html"
        };
    }
    
    private async Task<IActionResult> CreateAnimePageHtml(Models.Anime anime, AnimeInfo? info, IEnumerable<Models.Comment> comments)
    {
        const string path = "../../../Views/Anime-Page.html";
        var data = await File.ReadAllTextAsync(path);
        
        var tpl = Template.Parse(data);
        var res = tpl.RenderAsync(new { Anime = anime, Info = info, Comments = comments}, member => member.Name);
        return new HtmlCode
        {
            Buffer = Encoding.ASCII.GetBytes(await res),
            StatusCode = HttpStatusCode.OK,
            ContentType = "text/html"
        };
    }
}