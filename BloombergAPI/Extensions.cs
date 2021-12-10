using System.Collections;

namespace BloombergAPI;

public static class Extensions
{
    internal static Element AppendValues(this Element source,
        IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            source.AppendValue(value);
        }

        return source;
    }

    public static List<object> AsListOfObjects(this object source)
    {
        if (!IsListOfObjects(source))
        {
            throw new Exception($"{nameof(source)} is not a list of objects.");
        }

        var list = source as List<object>;

        return list is null or {Count: 0} ? new List<object>() : list;
    }

    public static bool IsListOfObjects(this object source)
    {
        return source is IList && source.GetType().IsGenericType;
    }

    internal static void SetOptions(this Request source,
        Dictionary<string, string> options)
    {
        foreach (var (key, value) in options)
        {
            source.Set(key, value);
        }
    }
}