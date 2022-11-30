using System.Net;

namespace HttpServer.Models.DI;

public class ServerException : IActionResult
{
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;
    public byte[] Buffer { get; set; }
    public string Template { get; set; }
    public Cookie Cookie { get; set; }
    public string ContentType { get; set; }
}