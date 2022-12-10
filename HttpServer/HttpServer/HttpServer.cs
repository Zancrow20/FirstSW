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
using static System.GC;

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
      Console.WriteLine($"Running at {_settings?.Port} port");
      _listener.Start();

      _serverStatus = ServerStatus.Start;

      await ListenAsync();
   }

   public void Stop()
   {
      if (_serverStatus == ServerStatus.Stop) return;

      Console.WriteLine("Stop server");
      _listener.Stop();

      _serverStatus = ServerStatus.Stop;
      Console.WriteLine("Server stopped");
   }

   private async Task ListenAsync()
   {
      while (_serverStatus == ServerStatus.Start)
      {
         try
         {
            var context = await _listener.GetContextAsync();
            var response = context.Response;
            var request = context.Request;
            var returnedValue = await MethodHandler(context);
            if (returnedValue == null)
            {
               await StaticProvider.GetPage(context, _settings);
               continue;   
            } 
            response.ContentLength64 = returnedValue.Buffer.Length;
            response.StatusCode = (int) returnedValue.StatusCode;
            if (returnedValue.Location != null)
               response.Headers.Add("Location", returnedValue.Location);
            if (returnedValue.Cookie != null)
            {
               //request.Cookies.Add(returnedValue.Cookie);
               response.Cookies.Add(returnedValue.Cookie);
            }
            response.Headers.Set(HttpResponseHeader.ContentType, returnedValue.ContentType);
            await using var output = response.OutputStream;
            await output.WriteAsync(returnedValue.Buffer);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
         }
      }
   }
   

   private async Task<IActionResult?> MethodHandler(HttpListenerContext httpContext)
   {
      var request = httpContext.Request;
      var response = httpContext.Response;
      
      var uri = string.Join("", request.Url.Segments!);
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
            var httpMethodUri = request.Url?.AbsolutePath.Split('/',StringSplitOptions.RemoveEmptyEntries)[1];
            
            if (methodUri == string.Empty)
               return httpMethodUri == methodUri;
            
            return Regex.IsMatch(httpMethodUri, methodUri);
         });
      
      if (method is null) return null;
      
      var objParams = new List<object?> {httpContext};
      switch (httpMethod)
      {
         case "HttpGET":
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
      objParams = method.GetParameters()
            .Select((p, i) => Convert.ChangeType(objParams?[i], p.ParameterType)).ToList();

      var task = (Task)method.Invoke(Activator.CreateInstance(controller), objParams.ToArray()) as dynamic;
      IActionResult? returnedValue = await task!;
      return returnedValue;
   }
   
   private static async Task<IEnumerable<string>> GetQueryStringAsync(HttpListenerRequest request)
   {
      var body = request.InputStream;
      var encoding = request.ContentEncoding;
      using var reader = new StreamReader(body, encoding);
      return ParseQuery(await reader.ReadToEndAsync());
   }

   private static IEnumerable<string> ParseQuery(string query)
      => query.Split('&')
         .Select(element => element.Split('=')[^1].Replace("%40", "@").Replace("%2B","+"));

   public void Dispose()
   {
      Stop();
   }
}