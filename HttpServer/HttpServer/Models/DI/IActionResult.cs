using System.Net;

namespace HttpServer.Models.DI;

public interface IActionResult
{
    HttpStatusCode StatusCode { get; set; }
    byte[] Buffer { get; set; }
    string Location { get; set; }
    Cookie Cookie { get; set; }
    string ContentType { get; set; }
}