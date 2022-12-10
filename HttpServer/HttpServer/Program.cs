using System;
using System.Buffers;
using System.Net;
using System.IO;
using HttpServer;

namespace HttpServer
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            using (var server = new HttpServer())
            {
                var handler = new RequestHandler(server) {KeepRunning = true};
                await handler.HandleRequests("start");
                while (handler.KeepRunning)
                    await handler.HandleRequests(Console.ReadLine()?.ToLower());
            }
            Console.Read();
            Console.Clear();
        }
    }
}