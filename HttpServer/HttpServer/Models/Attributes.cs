using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace HttpServer.Models;

public class PasswordAttribute : ValidationAttribute
{
    Regex hasNumber = new(@"[0-9]+");
    Regex hasUpperChar = new(@"[A-Z]+");
    Regex hasMinimum8Chars = new(@".{8,}");
    public override bool IsValid(object? value)
    {
        if(value is string password)
        {
            if (hasNumber.IsMatch(password) && hasUpperChar.IsMatch(password) && hasMinimum8Chars.IsMatch(password))
                return true;
            
            ErrorMessage = "Invalid password! It should has 8 symb min, upper case symbols and numbers";
            return false;
        }
        return false;
    }
}

public class MyPhoneAttribute : ValidationAttribute
{
    private Regex phoneRegex = new("^\\+?[1-9][0-9]{7,14}$");
    public override bool IsValid(object? value)
    {
        if(value is string phone)
        {
            if (!phoneRegex.IsMatch(phone))
            {
                ErrorMessage = "Invalid phone!";
                return false;
            }
            return true;
        }
        return false;
    }
}

public class GenreAttribute : ValidationAttribute
{
    private Regex genreRegex = new("^[A-Za-z]{2,}$");
    public override bool IsValid(object? value)
    {
        if(value is string genre)
        {
            if (!genreRegex.IsMatch(genre))
            {
                ErrorMessage = "Invalid genre! Don`t use numbers and other special symbols";
                return false;
            }
            return true;
        }
        return value == null;
    }
}

public class NicknameAttribute : ValidationAttribute
{
    private Regex nicknameRegex = new("^[A-Za-z]{2,}[_-]?[A-Za-z0-9]{2,}$");
    public override bool IsValid(object? value)
    {
        if(value is string nickname)
        {
            if (!nicknameRegex.IsMatch(nickname))
            {
                ErrorMessage = "Invalid nickname! Don`t use special symbols";
                return false;
            }
            return true;
        }
        return false;
    }
}

public class MyEmailAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        try
        {
            var m = new MailAddress(value?.ToString());
            return true;
        }
        catch (FormatException)
        {
            ErrorMessage = "Invalid Email!";
            return false;
        }
    }
}