using System.Collections.Immutable;

namespace Windvale.Bootstrap;

public enum Boundedˉserviceˉexchangeˉstate
{
    Empty,
    Requestˉready,
    Serviceˉprocessing,
    Replyˉready,
    Completed,
    Peerˉexited,
    Closed,
}

public static class Boundedˉserviceˉexchangeˉcontract
{
    public const int MAXIMUM_MESSAGE_BYTES = 4_096;
    public const uint CLIENT_ENDPOINT = 0x0001_0000;
    public const uint SERVICE_ENDPOINT = 0x0001_0001;
    public const uint RIGHT_SEND_REQUEST = 1U << 0;
    public const uint RIGHT_RECEIVE_REPLY = 1U << 1;
    public const uint RIGHT_RECEIVE_REQUEST = 1U << 2;
    public const uint RIGHT_SEND_REPLY = 1U << 3;
    public const uint CLIENT_RIGHTS = RIGHT_SEND_REQUEST | RIGHT_RECEIVE_REPLY;
    public const uint SERVICE_RIGHTS = RIGHT_RECEIVE_REQUEST | RIGHT_SEND_REPLY;
}

public sealed class Boundedˉserviceˉexchangeˉexception(
    string code,
    string message) : Exception(message)
{
    public string Code { get; } = code;
}

// This Stage 0 oracle intentionally knows only endpoint authority, one copied
// message, bounds, and terminal lifecycle. Service-specific formats stay above it.
public sealed class Boundedˉserviceˉexchange
{
    private ImmutableArray<byte> Message = ImmutableArray<byte>.Empty;

    public Boundedˉserviceˉexchangeˉstate State { get; private set; }

    public void Sendˉrequest(uint endpoint, uint rights, ReadOnlySpan<byte> request)
    {
        Requireˉendpoint(endpoint, rights,
            Boundedˉserviceˉexchangeˉcontract.CLIENT_ENDPOINT,
            Boundedˉserviceˉexchangeˉcontract.RIGHT_SEND_REQUEST);
        Requireˉstate(Boundedˉserviceˉexchangeˉstate.Empty);
        Message = Copyˉmessage(request);
        State = Boundedˉserviceˉexchangeˉstate.Requestˉready;
    }

    public ImmutableArray<byte> Receiveˉrequest(uint endpoint, uint rights)
    {
        Requireˉendpoint(endpoint, rights,
            Boundedˉserviceˉexchangeˉcontract.SERVICE_ENDPOINT,
            Boundedˉserviceˉexchangeˉcontract.RIGHT_RECEIVE_REQUEST);
        Requireˉstate(Boundedˉserviceˉexchangeˉstate.Requestˉready);
        var Result = Message.ToArray().ToImmutableArray();
        Message = ImmutableArray<byte>.Empty;
        State = Boundedˉserviceˉexchangeˉstate.Serviceˉprocessing;
        return Result;
    }

    public void Sendˉreply(uint endpoint, uint rights, ReadOnlySpan<byte> reply)
    {
        Requireˉendpoint(endpoint, rights,
            Boundedˉserviceˉexchangeˉcontract.SERVICE_ENDPOINT,
            Boundedˉserviceˉexchangeˉcontract.RIGHT_SEND_REPLY);
        Requireˉstate(Boundedˉserviceˉexchangeˉstate.Serviceˉprocessing);
        Message = Copyˉmessage(reply);
        State = Boundedˉserviceˉexchangeˉstate.Replyˉready;
    }

    public ImmutableArray<byte> Receiveˉreply(uint endpoint, uint rights)
    {
        Requireˉendpoint(endpoint, rights,
            Boundedˉserviceˉexchangeˉcontract.CLIENT_ENDPOINT,
            Boundedˉserviceˉexchangeˉcontract.RIGHT_RECEIVE_REPLY);
        Requireˉstate(Boundedˉserviceˉexchangeˉstate.Replyˉready);
        var Result = Message.ToArray().ToImmutableArray();
        Message = ImmutableArray<byte>.Empty;
        State = Boundedˉserviceˉexchangeˉstate.Completed;
        return Result;
    }

    public void Peerˉexit()
    {
        if (State is Boundedˉserviceˉexchangeˉstate.Peerˉexited or
            Boundedˉserviceˉexchangeˉstate.Closed)
        {
            throw new Boundedˉserviceˉexchangeˉexception(
                "WVSI4002", "The bounded service exchange is already terminal.");
        }
        Message = ImmutableArray<byte>.Empty;
        State = Boundedˉserviceˉexchangeˉstate.Peerˉexited;
    }

    public void Close()
    {
        if (State is not (Boundedˉserviceˉexchangeˉstate.Completed or
            Boundedˉserviceˉexchangeˉstate.Peerˉexited))
        {
            throw new Boundedˉserviceˉexchangeˉexception(
                "WVSI4002", "The bounded service exchange is not terminal.");
        }
        Message = ImmutableArray<byte>.Empty;
        State = Boundedˉserviceˉexchangeˉstate.Closed;
    }

    private static ImmutableArray<byte> Copyˉmessage(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length is < 1 or > Boundedˉserviceˉexchangeˉcontract.MAXIMUM_MESSAGE_BYTES)
        {
            throw new Boundedˉserviceˉexchangeˉexception(
                "WVSI4003", "The opaque service message exceeds the transport limit.");
        }
        return bytes.ToArray().ToImmutableArray();
    }

    private static void Requireˉendpoint(
        uint endpoint,
        uint rights,
        uint expectedˉendpoint,
        uint requiredˉright)
    {
        if (endpoint != expectedˉendpoint || (rights & requiredˉright) == 0)
        {
            throw new Boundedˉserviceˉexchangeˉexception(
                "WVSI4001", "The service endpoint is unauthorized for this operation.");
        }
    }

    private void Requireˉstate(Boundedˉserviceˉexchangeˉstate expected)
    {
        if (State != expected)
        {
            throw new Boundedˉserviceˉexchangeˉexception(
                "WVSI4002", "The bounded service exchange transition is invalid.");
        }
    }
}
