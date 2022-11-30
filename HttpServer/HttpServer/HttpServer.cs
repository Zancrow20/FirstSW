using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using HttpServer.Attributes;
using System.Text.RegularExpressions;
using HttpServer.Controllers;
using HttpServer.Models;
using HttpServer.Models.DI;
using HttpServer.Services;

namespace HttpServer;

public enum ServerStatus
{
   Abort,
   Close,
   Start,
   Stop,
}

public class HttpServer : IDisposable
{
   private readonly HttpListener _listener;
   private ServerStatus _serverStatus = ServerStatus.Close;

   private static readonly Dictionary<string, string> _extensions = new()
   {
      {"html", "text/html"},
      {"css", "text/css"},
      {"php", "text/php"},
      {"png", "image/png"},
      {"gif", "image/gif"},
      {"jpeg", "image/jpeg"},
      {"svg", "image/svg+xml"},
      {"jpg", "image/jpg"}
   };

   private ServerSettings? _settings;
   
   private Response ResponseInfo = new() {Buffer = Array.Empty<byte>(), StatusCode = default, Content = default, Cookie = default};

   public HttpServer()
   {
      _listener = new HttpListener();
   }

   public async Task Start()
   {
      if (_serverStatus == ServerStatus.Start)
      {
         Console.WriteLine("Сервер уже работает");
         return;
      }

      if (File.Exists(@"./settings.json"))
      {
         _settings = JsonSerializer.Deserialize<ServerSettings>(await File.ReadAllTextAsync("./settings.json"));
         _listener.Prefixes.Clear();  
      }
      else
         Console.WriteLine("settings.json not found");
      
      _listener.Prefixes.Add($"http://localhost:{_settings?.Port}/");
      Console.WriteLine("Запуск сервера...");
      _listener.Start();

      _serverStatus = ServerStatus.Start;
      Console.WriteLine("Сервер запущен");

      await ListenAsync();
   }

   public void Stop()
   {
      if (_serverStatus == ServerStatus.Stop) return;

      Console.WriteLine("Остановка сервера...");
      _listener.Stop();

      _serverStatus = ServerStatus.Stop;
      Console.WriteLine("Сервер остановлен");
   }

   private async Task ListenAsync()
   {
      while (_serverStatus == ServerStatus.Start)
      {
         try
         {
            var context = await _listener.GetContextAsync();
            var methodHandled = MethodHandler(context);
            if (!await methodHandled) await StaticProvider.GetPage(context, _settings);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
         }
         
         /*{
            /*response.Headers.Set(HttpResponseHeader.ContentType, ResponseInfo.Content);
            response.StatusCode = (int) ResponseInfo.StatusCode;
            if (ResponseInfo.Cookie != null)
               response.SetCookie(ResponseInfo.Cookie);
            if (ResponseInfo.Buffer == null)
            {
               await MethodHandler(context);
               continue;
            }
            response.ContentLength64 = ResponseInfo.Buffer.Length;
            await using var output = response.OutputStream;
            await output.WriteAsync(ResponseInfo.Buffer);
         }*/
            
      }
   }
   

   private async Task<bool> MethodHandler(HttpListenerContext httpContext)
   {
      var request = httpContext.Request;
      
      var response = httpContext.Response;
      var uri = string.Join("", request.Url.Segments!);
      
      if (request.Url?.Segments.Length < 1 || uri == "/")
      {
         ResponseInfo = new Response
         {
            StatusCode = HttpStatusCode.Redirect,
            Content = "text/html",
         };
         response.Headers.Set("Location","/anime/");
         return false;
      }

      
      var controllerName = uri.Split('/')[1];
      var httpMethod = $"Http{httpContext.Request.HttpMethod}";
      var inputParams = await GetQueryStringAsync(request);
      var assembly = Assembly.GetExecutingAssembly();

      var controller = assembly
         .GetTypes()
         .Where(t => Attribute.IsDefined(t, typeof(HttpController)))
         .FirstOrDefault(c => 
         {
            var attribute = c.CustomAttributes
               .FirstOrDefault(a => a.AttributeType.Name == "HttpController");
            return string.Equals(attribute
               ?.AttributeType
               .GetProperty("ControllerName")
               ?.GetValue(c.GetCustomAttribute<HttpController>())
               ?.ToString(), controllerName, StringComparison.CurrentCultureIgnoreCase);
         });

      var method = controller?.GetMethods()
         .FirstOrDefault(method =>
         {
            var attribute = method.CustomAttributes
               .FirstOrDefault(attr => attr.AttributeType.Name == $"{httpMethod}");
            if (attribute == null) return false;
            var methodUri = attribute.AttributeType
               .GetProperty("MethodUri")
               ?.GetValue(method.GetCustomAttribute(attribute.AttributeType))?
               .ToString();
            var httpMethodUri = request.Url?.AbsolutePath.Split('/',StringSplitOptions.RemoveEmptyEntries)[^1];
            
            if (methodUri == string.Empty)
               return httpMethodUri == methodUri;
            
            return Regex.IsMatch(httpMethodUri, methodUri);
         });
      
      if (method is null) return false;
      
      var objParams = new List<object?> {httpContext};
      switch (httpMethod)
      {
         case "HttpGET" when method.Name is not "GetAccountInfo" :
            objParams.AddRange(httpContext.Request.Url?
               .Segments
               .Where(segment => segment != "/")
               .Skip(3)
               .Select(s => s.Replace("/", ""))
               .ToList() ?? new List<string>());
            break;
         case "HttpPOST":
            objParams.AddRange(inputParams);
            break;
      }
      
      if (method.Name is "GetAccounts" or "GetAccountInfo")
      {
         var cookieValue = request.Cookies["SessionId"] != null ? 
            request.Cookies["SessionId"]?.Value : "";
         objParams.Add(cookieValue!);
      }
      
      objParams = method.GetParameters()
            .Select((p, i) => Convert.ChangeType(objParams?[i], p.ParameterType)).ToList();

      var task = (Task)method.Invoke(Activator.CreateInstance(controller), objParams.ToArray()) as dynamic;
      IActionResult? returnedValue = await task!;
      
      //TODO переделать под IActionResult
      /*buffer = returnedValue switch
      {
         not null => Encoding.ASCII.GetBytes(JsonSerializer.Serialize(returnedValue)),
         null when method.Name is "GetAccounts" or "GetAccountInfo" 
            => Encoding.ASCII.GetBytes("401 - not authorized"),
         null => Encoding.ASCII.GetBytes("404 - not found")
      };*/
      
      response.ContentLength64 = returnedValue.Buffer.Length;

      /*ResponseInfo = returnedValue switch
      {
         not null when method.Name is "Login" => GetLoginResponse(returnedValue, buffer),
         null when method.Name is "GetAccounts" or "GetAccountInfo" => 
            new Response {Buffer = buffer, Content = "Application/json", StatusCode = HttpStatusCode.Unauthorized},
         _ => new Response {Buffer = buffer, Content = "Application/json", StatusCode = HttpStatusCode.OK}
      };*/
      response.Headers.Set(HttpResponseHeader.ContentType, returnedValue.ContentType);
      response.StatusCode = (int) returnedValue.StatusCode;
      await using var output = response.OutputStream;
      await output.WriteAsync(returnedValue.Buffer);
      return true;
   }

   private static Response GetLoginResponse(object returnedValue, byte[] bytes)
   {
      var sessionId = (SessionId) returnedValue;
      var cookie = new Cookie("SessionId",
         $"Guid={sessionId.Guid}");
      return new Response { Buffer = bytes, Content = "Application/json", StatusCode = HttpStatusCode.OK, Cookie = cookie};
   }
   
   private static async Task<IEnumerable<string>> GetQueryStringAsync(HttpListenerRequest request)
   {
      //TODO разобраться, почему пустая строка приходит...
      var body = request.InputStream;
      var encoding = request.ContentEncoding;
      using var reader = new StreamReader(body, encoding);
      return ParseQuery(await reader.ReadToEndAsync());
   }

   private static IEnumerable<string> ParseQuery(string query)
      => query.Split('&').Select(element => element.Split('=')[^1]);
      
   
   
   
   public void Dispose() => Stop();
}