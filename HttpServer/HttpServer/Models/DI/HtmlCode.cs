using System.Net;

namespace HttpServer.Models.DI;

public class HtmlCode : IActionResult
{
    public HttpStatusCode StatusCode { get; set; }
    public byte[] Buffer { get; set; }
    public string Template { get; set; }
    public Cookie Cookie { get; set; }
    public string ContentType { get; set; }
}