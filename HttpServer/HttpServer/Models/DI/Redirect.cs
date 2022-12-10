using System.Net;

namespace HttpServer.Models.DI;

public class Redirect : IActionResult
{
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.Redirect;
    public byte[] Buffer { get; set; }
    public string Location { get; set; }
    public Cookie Cookie { get; set; }
    public string ContentType { get; set; }
}