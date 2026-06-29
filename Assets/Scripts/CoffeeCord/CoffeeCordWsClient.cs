using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class CoffeeCordWsClient : IDisposable
{
    private ClientWebSocket _ws;
    private CancellationTokenSource _cts;
    private string _token;
    private bool _isConnected;

    private const string WsUrl = "ws://176.123.169.111:8080/chat";

    public bool IsConnected => _isConnected;

    public event Action OnConnected;
    public event Action<WsServersList> OnServersList;
    public event Action<WsChannelsList> OnChannelsList;
    public event Action<WsDirectChatsList> OnDirectChatsList;
    public event Action<WsChannelCreated> OnChannelCreated;
    public event Action<WsSent> OnSent;
    public event Action<WsNewMessage> OnNewMessage;
    public event Action<WsServerCreated> OnServerCreated;
    public event Action<WsHistory> OnHistory;
    public event Action<WsMemberAdded> OnMemberAdded;
    public event Action<WsError> OnWsError;
    public event Action<string> OnDisconnected;
    public event Action<Exception> OnError;

    // ---- Connect ----

    public async Task Connect(string token)
    {
        if (_isConnected)
        {
            Debug.LogWarning("[CoffeeCordWs] Already connected, disconnecting first");
            await DisconnectAsync();
        }

        _token = token;
        _cts = new CancellationTokenSource();

        try
        {
            _ws = new ClientWebSocket();
            var uri = new Uri($"{WsUrl}?token={Uri.EscapeDataString(token)}");
            await _ws.ConnectAsync(uri, _cts.Token);
            _isConnected = true;
            Debug.Log("[CoffeeCordWs] Connected");

            _ = ReceiveLoopAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CoffeeCordWs] Connection failed: {ex.Message}");
            OnError?.Invoke(ex);
            throw;
        }
    }

    // ---- Send ----

    public async Task SendAsync(WsMessage message)
    {
        if (!_isConnected || _ws == null)
            throw new InvalidOperationException("WebSocket is not connected");

        var json = JsonConvert.SerializeObject(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        var segment = new ArraySegment<byte>(bytes);

        try
        {
            await _ws.SendAsync(segment, WebSocketMessageType.Text, true, _cts.Token);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CoffeeCordWs] Send failed: {ex.Message}");
            OnError?.Invoke(ex);
        }
    }

    public Task SendPingAsync() =>
        SendAsync(new WsMessage { type = "ping" });

    public Task SendGetServersAsync() =>
        SendAsync(new WsMessage { type = "get_servers" });

    public Task SendGetChannelsAsync(string serverId) =>
        SendAsync(new WsGetChannels { type = "get_channels", serverId = serverId });

    public Task SendGetDirectChatsAsync() =>
        SendAsync(new WsMessage { type = "get_direct_chats" });

    public Task SendCreateChannelAsync(string serverId, string name) =>
        SendAsync(new WsCreateChannel { type = "create_channel", serverId = serverId, name = name });

    public Task SendMessageAsync(string channelId, string text) =>
        SendAsync(new WsSendMessage { type = "send_message", channelId = channelId, text = text });

    public Task SendGetHistoryAsync(string channelId, int limit = 30, long? beforeTimestamp = null) =>
        SendAsync(new WsGetHistory
        {
            type = "get_history",
            channelId = channelId,
            limit = limit,
            beforeTimestamp = beforeTimestamp
        });

    public Task SendAddMemberAsync(string serverId, string userId) =>
        SendAsync(new WsAddMember { type = "add_member", serverId = serverId, userId = userId });

    // ---- Receive Loop ----

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[1024 * 16];
        var sb = new StringBuilder();

        try
        {
            while (!ct.IsCancellationRequested && _ws.State == WebSocketState.Open)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("[CoffeeCordWs] Server closed connection");
                    break;
                }

                sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                if (result.EndOfMessage)
                {
                    var raw = sb.ToString();
                    sb.Clear();
                    HandleMessage(raw);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Debug.LogError($"[CoffeeCordWs] Receive error: {ex.Message}");
            OnError?.Invoke(ex);
        }
        finally
        {
            _isConnected = false;
            OnDisconnected?.Invoke("Connection closed");
        }
    }

    // ---- Handle Incoming ----

    private void HandleMessage(string raw)
    {
        if (raw == "connected")
        {
            Debug.Log("[CoffeeCordWs] Handshake: connected");
            OnConnected?.Invoke();
            return;
        }

        try
        {
            var baseMsg = JsonConvert.DeserializeObject<WsMessage>(raw);

            switch (baseMsg.type)
            {
                case "servers_list":
                    OnServersList?.Invoke(JsonConvert.DeserializeObject<WsServersList>(raw));
                    break;

                case "channels_list":
                    OnChannelsList?.Invoke(JsonConvert.DeserializeObject<WsChannelsList>(raw));
                    break;

                case "direct_chats_list":
                    OnDirectChatsList?.Invoke(JsonConvert.DeserializeObject<WsDirectChatsList>(raw));
                    break;

                case "channel_created":
                    OnChannelCreated?.Invoke(JsonConvert.DeserializeObject<WsChannelCreated>(raw));
                    break;

                case "sent":
                    OnSent?.Invoke(JsonConvert.DeserializeObject<WsSent>(raw));
                    break;

                case "new_message":
                    OnNewMessage?.Invoke(JsonConvert.DeserializeObject<WsNewMessage>(raw));
                    break;

                case "server_created":
                    OnServerCreated?.Invoke(JsonConvert.DeserializeObject<WsServerCreated>(raw));
                    break;

                case "history":
                    OnHistory?.Invoke(JsonConvert.DeserializeObject<WsHistory>(raw));
                    break;

                case "member_added":
                    OnMemberAdded?.Invoke(JsonConvert.DeserializeObject<WsMemberAdded>(raw));
                    break;

                case "error":
                    var err = JsonConvert.DeserializeObject<WsError>(raw);
                    Debug.LogError($"[CoffeeCordWs] Server error: {err.message}");
                    OnWsError?.Invoke(err);
                    break;

                default:
                    Debug.LogWarning($"[CoffeeCordWs] Unknown message type: {baseMsg.type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CoffeeCordWs] Failed to parse: {ex.Message}\nRaw: {raw}");
        }
    }

    // ---- Disconnect / Dispose ----

    public async Task DisconnectAsync()
    {
        _isConnected = false;
        _cts?.Cancel();

        if (_ws != null && _ws.State == WebSocketState.Open)
        {
            try
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None);
            }
            catch { }
        }

        _ws?.Dispose();
        _ws = null;
        Debug.Log("[CoffeeCordWs] Disconnected");
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _ws?.Dispose();
    }
}
