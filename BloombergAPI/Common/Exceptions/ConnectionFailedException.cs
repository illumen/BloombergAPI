namespace BloombergAPI.Common.Exceptions;

public sealed class ConnectionFailedException : Exception
{
    public ConnectionFailedException(string message) : base(message)
    {
    }
}