using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Text.Json;
using HttpServer.Attributes;
using HttpServer.Models;
using HttpServer.Models.DI;
using HttpServer.ORM;
using HttpServer.Services;
using Scriban;

namespace HttpServer.Controllers;


[HttpController("accounts")]
public class Accounts
{
    private MyEmailAttribute _emailAttribute = new();
    private PasswordAttribute _passwordAttribute = new();
    private readonly UserRepository _userRepo = new();
    [HttpGET("myaccount")]
    public async Task<IActionResult> GetMyAccount(HttpListenerContext context)
    {
        try
        {
            //TODO Сделать страницу пользователя scriban(тут всё переделать)
            if (IsAuthorized(context, out var guid) && await SessionManager.CheckSession(guid))
            {
                var id = ((await SessionManager.GetInfo(guid))!).UserId;
                var user = await _userRepo.GetById(new User_ID() {Id = id});
                const string path = "../../../Views/MyAccountPage.html";
                var data = await File.ReadAllTextAsync(path);
        
                var tpl = Template.Parse(data);
                var res = await tpl.RenderAsync(new {User = user}, member => member.Name);
                return new HtmlCode
                {
                    Buffer = Encoding.ASCII.GetBytes(res),
                    StatusCode = HttpStatusCode.OK,
                    ContentType = "text/html"
                };
            }
                // return new MyAuthorization
                // {
                //     StatusCode = HttpStatusCode.OK,
                //     ContentType = "text/html",
                //     Buffer = Encoding.ASCII.GetBytes(
                //         JsonSerializer.Serialize(
                //             await _userRepo.GetById(new User_ID()
                //             {
                //                 Id = (await SessionManager.GetInfo(guid))!.UserId
                //             })))
                //
                // };
        }
        catch (KeyNotFoundException)
        {
            return new MyAuthorization
            {
                Buffer = Encoding.ASCII.GetBytes("Please authorize again"),
                ContentType = "Application/json",
            };
        }
        catch (SqlException)
        {
            return new ServerException
                {Buffer = Encoding.ASCII.GetBytes("Can't get account info"), ContentType = "Application/json"};
        }
        catch (Exception)
        {
            return new ServerException
                {Buffer = Encoding.ASCII.GetBytes("Something is wrong in server"), ContentType = "Application/json"};
        }

        return new MyAuthorization
            {Buffer = Encoding.ASCII.GetBytes("Please authorize"),ContentType = "Application/json"};
    }
    
    [HttpPOST("login")]
    public async Task<IActionResult> Login(HttpListenerContext context,[MyEmail] string email,[Password] string password, bool remember)
    {
        try
        {
            if (IsAuthorized(context, out var guid))
                return new Redirect()
                {
                    Location = "/anime/all/",
                    Buffer = Array.Empty<byte>(),
                };
            if (!_emailAttribute.IsValid(email) || !_passwordAttribute.IsValid(password))
                return await CreateLoginPageWithSpan(_emailAttribute.ErrorMessage + _passwordAttribute.ErrorMessage);
                
            
            var user = await _userRepo.GetEntityByProperties(new User_ID(){Email = email});
            if (user == null)
                return await CreateLoginPageWithSpan("Incorrect login or password");
            var saltedPass = HashManager.GetSHA256(user?.Salt + password);
            if (user?.SaltedPassword == saltedPass)
            {
                if (guid == Guid.Empty)
                    guid = Guid.NewGuid();
                //TODO Разобраться, почему после редиректа нет куки
                var session = await SessionManager.GetOrAdd(email, user.Id, guid);
                var cookie = new Cookie("SessionId", $"Guid={session?.Guid}")
                {
                    Path = "/"
                };
                if (remember)
                    cookie.Expires = DateTime.Today + TimeSpan.FromDays(2);

                return new Redirect
                {
                    Buffer = Encoding.ASCII.GetBytes(""),
                    Cookie = cookie,
                    ContentType = "text/html",
                    Location = "/anime/all/"
                };
            }
            return await CreateLoginPageWithSpan("Incorrect login or password");
        }
        catch (SqlException)
        {
            return new ServerException
                {Buffer = Encoding.ASCII.GetBytes("Something is wrong in DataBase")};
        }
    }
    
    
    [HttpPOST("register")]
    public async Task<IActionResult> Register(HttpListenerContext context,[MyEmail] string email,
        [Nickname] string nickname,[Genre] string genre,
        string status,[MyPhone] string phoneNumber,[Password] string password)
    {
        try
        {
            var userToCheck = new User_ID()
            {
                Email = email,
                User_Nickname = nickname,
                Favourite_Genre = genre,
                Phone_Number = phoneNumber,
                SaltedPassword = password
            };
            var listOfErrors = Validate(userToCheck)
                .Select(res => res.ErrorMessage);

            if (listOfErrors.FirstOrDefault() != null)
            {
                var str = listOfErrors
                    .Aggregate((current, next) => $"<br>{current}<br>{next}");
                return await CreateRegisterPageWithSpan(str);
            }
            
            var user = await _userRepo.GetEntityByProperties(userToCheck);
            
            if (user is { })
                return await CreateRegisterPageWithSpan(
                    "This email already used.Try login or use another email to register");

            var salt = HashManager.CreateSalt();
            var hashedPassword = HashManager.GetSHA256(salt + password);
            await _userRepo.Create(new User_ID
            {
                Email = email, SaltedPassword = hashedPassword,
                Salt = salt, User_Nickname = nickname,
                Favourite_Genre = genre, Status = status, Phone_Number = phoneNumber
            });
            return await Login(context, email, password, true);
            //TODO проблема с id и его проверкой
        }
        catch (SqlException)
        {
            return new ServerException
                {Buffer = Encoding.ASCII.GetBytes("Something is wrong in DataBase")};
        }
    }

    [HttpGET("register-page")]
    public async Task<IActionResult> RegisterPage(HttpListenerContext context)
    {
        return await CreateRegisterPageWithSpan("");
    }
    
    [HttpGET("login-page")]
    public async Task<IActionResult> LoginPage(HttpListenerContext context)
    {
        if (IsAuthorized(context, out var guid))
            return new Redirect()
            {
                Location = "/accounts/myaccount/",
                Buffer = Array.Empty<byte>(),
            };
        return await CreateLoginPageWithSpan("");
    }

    private async Task<IActionResult> CreateLoginPageWithSpan(string span)
    {
        const string path = "../../../Views/Login.html";
        var data = await File.ReadAllTextAsync(path);
        var tpl = Template.Parse(data);
        var res = tpl.RenderAsync(new { Span = span}, member => member.Name);
        return new MyAuthorization
        {
            Buffer = Encoding.ASCII.GetBytes(await res),
            StatusCode = HttpStatusCode.OK,
            ContentType = "text/html"
        };
    }
    
    private async Task<IActionResult> CreateRegisterPageWithSpan(string span)
    {
        const string path = "../../../Views/Register.html";
        var data = await File.ReadAllTextAsync(path);
        var tpl = Template.Parse(data);
        var res = tpl.RenderAsync(new {Span = span}, member => member.Name);
        return new MyAuthorization
        {
            Buffer = Encoding.ASCII.GetBytes(await res),
            StatusCode = HttpStatusCode.OK,
            ContentType = "text/html"
        };
    }
    
    private bool IsAuthorized(HttpListenerContext context,out Guid guid)
        => Guid.TryParse(context.Request.Cookies["SessionId"]?.Value.Split('=')[^1], out guid);
    
    public List<ValidationResult>? Validate(User_ID userId)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(userId);
        return !Validator.TryValidateObject(userId, context, results, true) ? results : null;
    }
    
}