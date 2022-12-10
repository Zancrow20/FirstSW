using System.Security.Cryptography;
using System.Text;

namespace HttpServer.Services;

public class HashManager
{
    public static string CreateSalt()
    {
        var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        var rand = new Random();
        var result = "";
        for (var j = 1; j <= 32; j++)
        {
            var number = rand.Next(0, letters.Length - 1);
            result += letters[number];
        }

        return result;

    }

    public static string GetSHA256(string saltedPassword)
    {
        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

        var builder = new StringBuilder();
        foreach (var t in bytes)
            builder.Append(t.ToString("x2"));
        
        return builder.ToString();
    }

}