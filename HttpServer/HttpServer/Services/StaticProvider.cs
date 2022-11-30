using System.Net;
using System.Text;

namespace HttpServer.Services;

public class StaticProvider
{
   private static ServerSettings _settings;
   private static readonly Dictionary<string, string> _extensions = new()
   {
      {"html", "text/html"},
      {"css", "text/css"},
      {"php", "text/php"},
      {"png", "image/png"},
      {"gif", "image/gif"},
      {"jpeg", "image/jpeg"},
      {"svg", "image/svg+xml"},
      {"jpg", "image/jpg"},
      {"mp4", "video/mp4"},
      {"webm", "video/webm"},
      {"map", "text/css"},
   };
   
   public static async Task<byte[]> GetPage(HttpListenerContext context, ServerSettings settings)
    {
       var request = context.Request;
       var response = context.Response;
       _settings = settings;
       return await StaticResponseProviderMethod(request, response);
    }
    
    private static async Task<byte[]> StaticResponseProviderMethod(HttpListenerRequest request,HttpListenerResponse response)
       {
          //var rawUrl = request.RawUrl;
          var buffer= Array.Empty<byte>();
          if (Directory.Exists(_settings?.Path))
          {
             buffer =  await GetFile(request.RawUrl?.Replace("%20", " "), response, request);
             if (buffer == null)
             {
                response.Headers.Set(HttpResponseHeader.ContentType, "text/plain");
                response.StatusCode = (int) HttpStatusCode.NotFound;
                var error = "404 - not found";
                buffer = Encoding.UTF8.GetBytes(error);
             }
          }
          else
          {
             var error = $"Directory '{_settings?.Path}' not found";
             buffer = Encoding.UTF8.GetBytes(error);
          }
    
          response.ContentLength64 = buffer.Length;
          var output = response.OutputStream;
          await output.WriteAsync(buffer);
    
          output.Close();
          return buffer;
       }
    
       private static void AddHeaders(HttpListenerResponse response, string extension) =>
          response.Headers.Add(HttpResponseHeader.ContentType, _extensions[extension]);
    
       private static async Task<byte[]> GetFile(string? rawUrl, HttpListenerResponse response, HttpListenerRequest request)
       {
          byte[]? buffer = null;
          string path;
          //TODO проверить это место на правильный юрл
          /*if (response.StatusCode == 301)
             path = _settings?.Path + request.RawUrl;*/
          if (rawUrl == "/")
             path = _settings?.Path + "main" + rawUrl;
          else
             path = _settings.Path + rawUrl;
          var extension = Path.GetExtension(rawUrl)?.Trim('.').TrimStart('+');
          if (Directory.Exists(path))
          {
             path += "/index.html";
             if (File.Exists(path))
             {
                extension = "html";
                buffer = await File.ReadAllBytesAsync(path);
                AddHeaders(response, extension);
             }
          }
          else if (File.Exists(path))
          {
             buffer = await File.ReadAllBytesAsync(path);
             if (extension != null) 
                AddHeaders(response, extension);
          }
    
          return buffer;
       }
}