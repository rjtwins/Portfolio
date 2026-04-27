namespace BCT.Application.Exceptions;
public class AuthServiceFailedToGetTokenException : Exception
{
    public AuthServiceFailedToGetTokenException(string message, Exception? e = null) : base(message, e) { }
}
