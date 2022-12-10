using System.Net;

namespace HttpServer.Models.DI;

public class MyAuthorization : IActionResult
{
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.Unauthorized;
    public byte[] Buffer { get; set; }
    public string Location { get; set; }
    public Cookie Cookie { get; set; }
    public string ContentType { get; set; }
}