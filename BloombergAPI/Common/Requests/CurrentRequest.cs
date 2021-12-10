using BloombergAPI.Common.Models;
using BloombergAPI.Common.Responses;

namespace BloombergAPI.Common.Requests;

public sealed class CurrentRequest : BaseRequest<UndatedResponse>
{
    private readonly UndatedResponse _response = new();

    public override UndatedResponse GetResponse()
    {
        return _response;
    }

    public override void ProcessFields(Message msg)
    {
        var dataObj = msg.GetElement(Elements.SecurityData);

        for (var i = 0; i < dataObj.NumValues; ++i)
        {
            var securityData = dataObj.GetValueAsElement(i);
            var securityName = securityData.GetElementAsString(Elements.SecurityName);
            var fieldData = securityData.GetElement(Elements.FieldData);

            if (!_response.Data.ContainsKey(securityName))
            {
                _response.Data.Add(securityName, new Dictionary<string, object>());
            }

            for (var j = 0; j < fieldData.NumElements; ++j)
            {
                var field = fieldData.GetElement(j);

                if (field.IsArray)
                {
                    var list = new List<object>();
                    for (var k = 0; k < field.NumValues; ++k)
                    {
                        var bulkElement = field.GetValueAsElement(k);
                        for (var l = 0; l < bulkElement.NumElements; ++l)
                        {
                            list.Add(bulkElement.GetElement(l).GetValueAsString());
                        }
                    }
                    
                    _response.Data[securityName].Add(field.Name.ToString(), list);
                }
                else
                {
                    _response.Data[securityName].Add(field.Name.ToString(), field.GetValueAsString());
                }
            }
        }
    }
}