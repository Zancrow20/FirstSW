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
    
    [HttpGET("all")]
    public async Task<IActionResult> GetAllAnime(HttpListenerContext context)
    {
        try
        {
            var animeSeries = await _animeRepo.GetEntities();
            const string path = "./site/src/anime/all/index.html";
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
                Buffer = Encoding.ASCII.GetBytes("404 not found"),
                StatusCode = HttpStatusCode.NotFound,
                ContentType = "text/html"
            };
        }
    }
    
    [HttpGET("anime-page")]
    public async Task<IActionResult> GetAnimePage(HttpListenerContext context, int id)
    {
        try
        {
            var anime = await _animeRepo.GetById(id);
            //TODO Как правильно получить id по клику на картинку? через html или js+ajax???
            const string path = "./site/src/anime/anime-page/index.html";
            var data = await File.ReadAllTextAsync(path);
        
            var tpl = Template.Parse(data);
            var res = await tpl.RenderAsync(new { Anime = anime}, member => member.Name);
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
                Buffer = Encoding.ASCII.GetBytes("404 not found"),
                StatusCode = HttpStatusCode.NotFound,
                ContentType = "text/html"
            };
        }
        
    }
    
    [HttpGET("random")]
    public async Task<IActionResult> GetRandomAnimePage(HttpListenerContext context)
    {
        try
        {
            var series = await _animeRepo.GetEntities();
            var minId = series.Min(anime => anime.Anime_Id);
            var maxId = series.Max(anime => anime.Anime_Id);
            var rnd = new Random();
            var id = rnd.Next(minId, maxId);
            var anime = series.FirstOrDefault(anime => anime.Anime_Id == id);
        
            //TODO Как правильно указать путь для аниме? Или просто для всех одинаковый?(Спроси у Тимерхана)
            const string path = "./site/src/anime/random/index.html";
            var data = await File.ReadAllTextAsync(path);
        
            var tpl = Template.Parse(data);
            var res = await tpl.RenderAsync(new { Anime = anime}, member => member.Name);
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
                Buffer = Encoding.ASCII.GetBytes("404 not found"),
                StatusCode = HttpStatusCode.NotFound,
                ContentType = "text/html"
            };
        }
        
    }
    
    [HttpGET("best")]
    public async Task<IActionResult> GetBest(HttpListenerContext context)
    {
        try
        {
            var best = (await _animeRepo.GetEntities()).OrderBy(anime => anime.Average_Rating)
                .Take(10);
            const string path = "./site/src/anime/best/index.html";
            var data = await File.ReadAllTextAsync(path);
        
            var tpl = Template.Parse(data);
            var res = await tpl.RenderAsync(new { Best = best}, member => member.Name);
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
                Buffer = Encoding.ASCII.GetBytes("404 not found"),
                StatusCode = HttpStatusCode.NotFound,
                ContentType = "text/html"
            };
        }
        
    }

    [HttpGET("ongoing")]
    public async Task<IActionResult> GetOngoing(HttpListenerContext context)
    {
        try
        {
            var ongoing = (await _animeRepo.GetEntities()).Where(anime => anime.Is_Ongoing);
            const string path = "./site/src/anime/ongoing/index.html";
            var data = await File.ReadAllTextAsync(path);
        
            var tpl = Template.Parse(data);
            var res = await tpl.RenderAsync(new { Ongoing = ongoing}, member => member.Name);
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
                Buffer = Encoding.ASCII.GetBytes("404 not found"),
                StatusCode = HttpStatusCode.NotFound,
                ContentType = "text/html"
            };
        }
        
    }

    [HttpGET("completed")]
    public async Task<IActionResult> GetCompleted(HttpListenerContext context)
    {
        try
        {
            var completed = (await _animeRepo.GetEntities()).Where(anime => !anime.Is_Ongoing);
            const string path = "./site/src/anime/completed/index.html";
            var data = await File.ReadAllTextAsync(path);
        
            var tpl = Template.Parse(data);
            var res = await tpl.RenderAsync(new { Completed = completed}, member => member.Name);
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
                Buffer = Encoding.ASCII.GetBytes("404 not found"),
                StatusCode = HttpStatusCode.NotFound,
                ContentType = "text/html"
            };
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
                var _records = await _animeRepo.GetLike(query);
                const string searchData = "<table>" +
                                            "{{- for anime_record in records}}" +
                                                "<tr>" +
                                                    "<td class=\"search_result-name\">" +
                                                        "<a href=\"/anime/anime-page/{{anime_record.Anime_Id}}/\" class=\"search_result-ref\">" +
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
        //TODO: сделать для всех также
        return new HtmlCode
        {
            Buffer = Encoding.ASCII.GetBytes(val),
            StatusCode = statusCode,
            ContentType = contentType
        };
    }
}