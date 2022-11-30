using System.Net;

namespace HttpServer.Models;

public class Response
{
    public byte[] Buffer { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string? Content { get; set; }
    public Cookie? Cookie { get; set; }
}