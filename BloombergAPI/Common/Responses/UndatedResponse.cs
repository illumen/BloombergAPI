namespace BloombergAPI.Common.Responses;

public sealed class UndatedResponse
{
    public Dictionary<string, Dictionary<string, object>> Data { get; set; } = new();
}