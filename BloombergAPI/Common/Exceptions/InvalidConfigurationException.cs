namespace BloombergAPI.Common.Exceptions;

public sealed class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException(string message) : base(message)
    {
    }
}