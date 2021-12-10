using BloombergAPI.Common.Models;
using BloombergAPI.Common.Responses;

namespace BloombergAPI.Common.Requests;

public sealed class HistoricalRequest : BaseRequest<DatedResponse>
{
    private readonly DatedResponse _response = new();

    public override DatedResponse GetResponse()
    {
        return _response;
    }

    public override void ProcessFields(Message msg)
    {
        var dataObj = msg.GetElement(Elements.SecurityData);
        var securityName = dataObj.GetElementAsString(Elements.SecurityName);
        var securityData = dataObj.GetElement(Elements.FieldData);

        if (!_response.Data.ContainsKey(securityName))
        {
            _response.Data.Add(securityName, new Dictionary<string, Dictionary<DateTime, string>>());
        }

        for (var i = 0; i < securityData.NumValues; ++i)
        {
            var fieldData = securityData.GetValueAsElement(i);
            var date = fieldData.GetElementAsDatetime(Elements.Date).ToSystemDateTime();

            for (var j = 0; j < fieldData.NumElements; j++)
            {
                var field = fieldData.GetElement(j);

                if (field.Name.Equals(Elements.Date))
                {
                    continue;
                }

                if (!_response.Data[securityName]
                        .ContainsKey(field.Name.ToString()))
                {
                    _response.Data[securityName].Add(field.Name.ToString(), new Dictionary<DateTime, string>());
                }

                _response.Data[securityName][field.Name.ToString()].Add(date, field.GetValueAsString());
            }
        }
    }
}