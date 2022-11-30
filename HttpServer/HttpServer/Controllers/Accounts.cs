using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HttpServer.Attributes;
using HttpServer.Models;
using HttpServer.Models.DI;
using HttpServer.ORM;
using HttpServer.Services;

namespace HttpServer.Controllers;


[HttpController("accounts")]
public class Accounts
{
    private readonly UserRepository _userRepo;
        
    public Accounts() => _userRepo = new UserRepository();

    /*[HttpGET("")]
    public async Task<List<Account>?> GetAccounts(HttpListenerContext context,string? cookie)
    {
        try
        {
            var cookieInfo = cookie?.Split('=')[^1];
            if (Guid.TryParse(cookieInfo, out var guid) && await SessionManager.CheckSession(guid))
                return await _userRepo.GetEntities();
        }
        catch (KeyNotFoundException e)
        {
            return null;
        }
        
        return null;
    }*/

    [HttpGET("myaccount")]
    public async Task<IActionResult> GetMyAccount(HttpListenerContext context)
    {
        try
        {
            var cookie = context.Request.Cookies["SessionId"]?.Value;
            var cookieInfo = cookie?.Split('=')[^1];
            if (Guid.TryParse(cookieInfo, out var guid) && await SessionManager.CheckSession(guid))
                return new MyAuthorization
                {
                    StatusCode = HttpStatusCode.Redirect,
                    ContentType = "",
                    Buffer = Encoding.ASCII.GetBytes(
                        JsonSerializer.Serialize(
                            await _userRepo.GetById((await SessionManager.GetInfo(guid))!.UserId))),

                };
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
    public async Task<IActionResult> Login(HttpListenerContext context,string email, string password)
    {
        if(!await IsValid(email))
            return new MyAuthorization
                {Buffer = Encoding.ASCII.GetBytes("Incorrect email. Try enter again"),ContentType = "Application/json"};
        try
        {
            var user = await _userRepo.GetEntityByProperties(email);
            var saltedPass = HashManager.GetSHA256(user?.Salt + password);
            if (user?.SaltedPassword == saltedPass)
            {
                var guid = await SessionManager.GetOrAdd(email, user.User_Id);
                return new MyAuthorization
                {
                    Buffer = Encoding.ASCII.GetBytes("Success"),
                    Cookie = new Cookie("SessionId",$"Guid={guid}"),
                    ContentType = "Application/json"
                };
            }
            return new MyAuthorization
                {Buffer = Encoding.ASCII.GetBytes("Incorrect password")};
        }
        catch (SqlException)
        {
            return new ServerException
                {Buffer = Encoding.ASCII.GetBytes("Something is wrong in DataBase")};
        }
    }

    //TODO Сделать форму регистрации
    [HttpPOST("register")]
    public async Task<IActionResult> Register(HttpListenerContext context, string email, string password)
    {
        if(!await IsValid(email))
            return new MyAuthorization
            {Buffer = Encoding.ASCII.GetBytes("Incorrect email.Try enter again"),};
        try
        {
            var user = await _userRepo.GetEntityByProperties(email);
            if (user is { })
                return new MyAuthorization
                    {Buffer = Encoding.ASCII.GetBytes("This email already registered.Try login or use another email")};
        
            var salt = HashManager.CreateSalt();
            var hashedPassword = HashManager.GetSHA256(salt + password);
            await _userRepo.Create(new User_ID{Email = email, SaltedPassword = hashedPassword, Salt = salt});
            return await Login(context, email, hashedPassword);
        }
        catch (SqlException)
        {
            return new ServerException
                {Buffer = Encoding.ASCII.GetBytes("Something is wrong in DataBase")};
        }
    }
    
    private async Task<bool> IsValid(string emailAddress)
    {
        try
        {
            var m = new MailAddress(emailAddress);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}