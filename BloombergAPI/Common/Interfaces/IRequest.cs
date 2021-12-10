namespace BloombergAPI.Common.Interfaces;

public interface IRequest<out TResponse>
{
    TResponse GetResponse();

    void ProcessErrors(Message msg);

    void ProcessExceptions(Message msg);

    /// <summary>
    ///     Process the fields of the request message into a response object.
    /// </summary>
    /// <param name="msg"></param>
    void ProcessFields(Message msg);

    void ProcessResponse(AbstractSession session);

    void ProcessResponseEvent(Event eventObj);
}