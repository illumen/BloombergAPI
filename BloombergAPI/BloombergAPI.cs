using Microsoft.Extensions.Configuration;

namespace BloombergAPI;

public sealed class BloombergAPI : IDisposable
{
    private const string ServiceName = "//blp/refdata";
    private readonly bool _keepAlive; // = true;
    private Service? _service;
    private Session? _session;
    private SessionOptions? _sessionOptions;

    public BloombergAPI(IConfiguration config, bool keepAlive = true)
    {
        Configure(config);
        _keepAlive = keepAlive;

        if (_keepAlive)
        {
            _session = Connect();
            _service = _session.GetService(ServiceName);
        }
    }

    private void Configure(IConfiguration configuration)
    {
        if (string.IsNullOrEmpty(configuration["Vendors:Bloomberg:Host"]))
        {
            throw new InvalidConfigurationException("Configuration 'Vendors:Bloomberg:Host' is not set or invalid.");
        }

        if (string.IsNullOrEmpty(configuration["Vendors:Bloomberg:Port"]) ||
            !int.TryParse(configuration["Vendors:Bloomberg:Port"], out var port))
        {
            throw new InvalidConfigurationException("Configuration 'Vendors:Bloomberg:Port' is not set or invalid.");
        }

        _sessionOptions = new SessionOptions
        {
            ServerHost = configuration["Vendors:Bloomberg:Host"], // localhost
            ServerPort = port // 8194
        };
    }

    private Session Connect()
    {
        var session = new Session(_sessionOptions);

        if (!session.Start())
        {
            throw new ConnectionFailedException("Failed to start session.");
        }

        if (!session.OpenService(ServiceName))
        {
            throw new ConnectionFailedException("Failed to open service: " + ServiceName);
        }

        return session;
    }

    public UndatedResponse CurrentRequest(IEnumerable<string> securities, IEnumerable<string> fields,
        Dictionary<string, string>? options = null)
    {
        var request = new CurrentRequest();
        SendRequest("ReferenceDataRequest", request, securities, fields, options);

        return request.GetResponse();
    }

    public DatedResponse HistoricalRequest(IEnumerable<string> securities, IEnumerable<string> fields,
        Dictionary<string, string>? options = null)
    {
        if (options == null || !options.ContainsKey("startDate") || !options.ContainsKey("endDate"))
        {
            throw new RequestErrorException("Missing start/end dates.");
        }

        var request = new HistoricalRequest();
        SendRequest("HistoricalDataRequest", request, securities, fields, options);

        return request.GetResponse();
    }

    private void SendRequest<TResponse>(string requestType, IRequest<TResponse> requestObj,
        IEnumerable<string> securities, IEnumerable<string> fields,
        Dictionary<string, string>? options = null)
    {
        using var session = _keepAlive && _session != null ? _session : Connect();
        var service = _keepAlive && _service != null ? _service : session.GetService(ServiceName);

        var request = service.CreateRequest(requestType);
        request.GetElement(Elements.Securities).AppendValues(securities);
        request.GetElement(Elements.Fields).AppendValues(fields);

        if (options is {Count: > 0})
        {
            request.SetOptions(options);
        }

        session.SendRequest(request, null);
        requestObj.ProcessResponse(session);


        if (!_keepAlive)
        {
            session.Stop();
        }
    }

    public void Dispose()
    {
        if (_session == null)
        {
            return;
        }

        foreach (var subscription in _session.GetSubscriptions())
        {
            _session.Cancel(subscription.CorrelationID);
        }

        _session.Stop(AbstractSession.StopOption.SYNC);
        _service = null;
        _session = null;
        _sessionOptions = null;
    }
}