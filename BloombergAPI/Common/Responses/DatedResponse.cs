namespace BloombergAPI.Common.Responses;

public sealed class DatedResponse
{
    public Dictionary<string, Dictionary<string, Dictionary<DateTime, string>>> Data { get; set; } = new();
}