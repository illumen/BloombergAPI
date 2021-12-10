namespace BloombergAPI.Common.Exceptions;

public sealed class RequestErrorException : Exception
{
    public RequestErrorException(string message) : base(message)
    {
    }
}