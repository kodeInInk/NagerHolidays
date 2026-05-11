namespace NagerHolidays.Models;

public class ApiError
{
    public int Code {get;}
    public string Message {get;}

    public ApiError(int code, string message)
    {
        Code = code;
        Message = message;
    }
}