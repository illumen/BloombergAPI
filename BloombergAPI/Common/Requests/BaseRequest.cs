using BloombergAPI.Common.Exceptions;
using BloombergAPI.Common.Interfaces;
using BloombergAPI.Common.Models;

namespace BloombergAPI.Common.Requests;

public abstract class BaseRequest<TResponse> : IRequest<TResponse>
{
    public abstract TResponse GetResponse();

    public abstract void ProcessFields(Message msg);

    public void ProcessErrors(Message msg)
    {
        var securityData = msg.GetElement(Elements.SecurityData);
        if (!securityData.HasElement(Elements.SecurityError))
        {
            return;
        }

        var securityError = securityData.GetElement(Elements.SecurityError);
        var errorMessage = securityError.GetElement(Elements.Message);

        throw new RequestErrorException($"{securityError}, {errorMessage}.");
    }

    public void ProcessExceptions(Message msg)
    {
        var securityData = msg.GetElement(Elements.SecurityData);
        
        if (!securityData.HasElement(Elements.FieldExceptions))
        {
            return;
        }
        
        var fieldExceptions = securityData.GetElement(Elements.FieldExceptions);

        if (fieldExceptions.NumValues <= 0)
        {
            return;
        }

        var element = fieldExceptions.GetValueAsElement(0);
        var fieldId = element.GetElement(Elements.FieldId);
        var errorInfo = element.GetElement(Elements.ErrorInfo);
        var errorMessage = errorInfo.GetElement(Elements.Message);

        throw new RequestErrorException($"{errorInfo}, {errorMessage} ({fieldId}).");
    }

    public void ProcessResponse(AbstractSession session)
    {
        var streaming = true;
        while (streaming)
        {
            var eventObj = session.NextEvent();
            switch (eventObj.Type)
            {
                case Event.EventType.PARTIAL_RESPONSE:
                    ProcessResponseEvent(eventObj);
                    break;
                case Event.EventType.RESPONSE:
                    ProcessResponseEvent(eventObj);
                    streaming = false;
                    break;
                case Event.EventType.ADMIN:
                    break;
                case Event.EventType.SESSION_STATUS:
                    foreach (var msg in eventObj)
                    {
                        if (!msg.MessageType.Equals(Elements.SessionTerminated))
                        {
                            continue;
                        }

                        streaming = false;
                        break;
                    }

                    break;
                case Event.EventType.SUBSCRIPTION_STATUS:
                    break;
                case Event.EventType.REQUEST_STATUS:
                    break;
                case Event.EventType.SERVICE_STATUS:
                    break;
                case Event.EventType.SUBSCRIPTION_DATA:
                    break;
                case Event.EventType.TIMEOUT:
                    break;
                case Event.EventType.AUTHORIZATION_STATUS:
                    break;
                case Event.EventType.RESOLUTION_STATUS:
                    break;
                case Event.EventType.TOPIC_STATUS:
                    break;
                case Event.EventType.TOKEN_STATUS:
                    break;
                case Event.EventType.REQUEST:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Invalid argument " + nameof(eventObj.Type));
            }
        }
    }

    public void ProcessResponseEvent(Event eventObj)
    {
        foreach (var msg in eventObj)
        {
            if (msg.HasElement(Elements.ResponseError))
            {
                throw new RequestErrorException(msg.GetElement(Elements.ResponseError).GetValueAsString());
            }

            if (eventObj.Type != Event.EventType.PARTIAL_RESPONSE && eventObj.Type != Event.EventType.RESPONSE)
            {
                continue;
            }

            ProcessErrors(msg);
            ProcessExceptions(msg);
            ProcessFields(msg);
        }
    }
}