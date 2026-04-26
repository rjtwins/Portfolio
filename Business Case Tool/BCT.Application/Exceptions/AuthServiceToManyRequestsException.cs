namespace BCT.Application.Exceptions;

public class AuthServiceToManyRequestsException : Exception
{
    public AuthServiceToManyRequestsException(string message) : base(message) { }
}