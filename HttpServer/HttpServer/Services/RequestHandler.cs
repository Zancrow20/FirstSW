using System.Diagnostics;
using System.Net;

namespace HttpServer;

public class RequestHandler
{
    public bool KeepRunning { get; set; }

    private readonly HttpServer Server;

    public RequestHandler(HttpServer server)
    {
        Server = server;
    }
    public async Task HandleRequests(string? request)
    {
        switch (request)
        {
            case "start":
                await Server.Start();
                break;
            case "stop":
                Server.Stop();
                break;
            case "restart":
                Server.Stop();
                await Server.Start();
                break;
            case "exit":
                KeepRunning = false;
                break;
        }
    }
}
