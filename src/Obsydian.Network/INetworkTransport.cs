namespace Obsydian.Network;

/// <summary>
/// Abstraction for network transport (TCP, UDP, WebSocket, etc).
/// </summary>
public interface INetworkTransport : IDisposable
{
    bool IsConnected { get; }

    void Connect(string address, int port);
    void Disconnect();
    void Send(byte[] data);
    byte[]? Receive();

    void HostStart(int port);
    void HostStop();

    event Action? OnConnected;
    event Action? OnDisconnected;
    event Action<byte[]>? OnDataReceived;
}
