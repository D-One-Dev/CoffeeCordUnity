using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class CoffeeCordManager: IDisposable
{
    private CoffeeCordApiClient _api;
    private CoffeeCordWsClient _ws;
    private string _token;
    private UserProfile _currentUser;

    public CoffeeCordApiClient Api => _api;
    public CoffeeCordWsClient Ws => _ws;
    public string Token => _token;
    public UserProfile CurrentUser => _currentUser;
    public bool IsLoggedIn => !string.IsNullOrEmpty(_token);

    // Auth events
    public event Action<UserProfile> OnLoginSuccess;
    public event Action<string> OnLoginFailed;
    public event Action<UserProfile> OnRegisterSuccess;
    public event Action<string> OnRegisterFailed;

    // WebSocket events
    public event Action OnWsConnected;
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

    [Inject]
    public void Construct()
    {

        _api = new CoffeeCordApiClient();
        _ws = new CoffeeCordWsClient();

        _ws.OnConnected += () => OnWsConnected?.Invoke();
        _ws.OnServersList += e => OnServersList?.Invoke(e);
        _ws.OnChannelsList += e => OnChannelsList?.Invoke(e);
        _ws.OnDirectChatsList += e => OnDirectChatsList?.Invoke(e);
        _ws.OnChannelCreated += e => OnChannelCreated?.Invoke(e);
        _ws.OnSent += e => OnSent?.Invoke(e);
        _ws.OnNewMessage += e => OnNewMessage?.Invoke(e);
        _ws.OnServerCreated += e => OnServerCreated?.Invoke(e);
        _ws.OnHistory += e => OnHistory?.Invoke(e);
        _ws.OnMemberAdded += e => OnMemberAdded?.Invoke(e);
        _ws.OnWsError += e => OnWsError?.Invoke(e);
        _ws.OnDisconnected += reason => OnDisconnected?.Invoke(reason);
    }

    public void Dispose()
    {
        if (_ws != null)
            _ = _ws.DisconnectAsync();
        _ws?.Dispose();
    }

    // ---- Auth ----

    public async Task RegisterUser(string name, string lastName, string email, string password)
    {
        try
        {
            var response = await _api.RegisterUser(name, lastName, email, password);
            _currentUser = response.user;
            OnRegisterSuccess?.Invoke(response.user);
        }
        catch (ApiException ex)
        {
            OnRegisterFailed?.Invoke(ex.Message);
        }
        catch (Exception ex)
        {
            OnRegisterFailed?.Invoke(ex.Message);
        }
    }

    public async Task LoginAndConnect(string email, string password)
    {
        try
        {
            var loginResponse = await _api.Login(email, password);
            _token = loginResponse.token;

            var profileResponse = await _api.GetProfile(_token);
            _currentUser = profileResponse.user;

            OnLoginSuccess?.Invoke(_currentUser);

            await _ws.Connect(_token);
        }
        catch (ApiException ex)
        {
            OnLoginFailed?.Invoke(ex.Message);
        }
        catch (Exception ex)
        {
            OnLoginFailed?.Invoke(ex.Message);
        }
    }

    public async Task ConnectWs()
    {
        if (!string.IsNullOrEmpty(_token))
            await _ws.Connect(_token);
    }

    public async Task DisconnectWs()
    {
        await _ws.DisconnectAsync();
    }

    public void Logout()
    {
        _token = null;
        _currentUser = null;
        _ = _ws.DisconnectAsync();
    }

    // ---- REST shortcuts ----

    public async Task<UserProfile> SearchUserByEmail(string email)
    {
        var response = await _api.SearchUserByEmail(_token, email);
        return response?.user;
    }

    public async Task<UserProfile> GetUserById(string userId)
    {
        var response = await _api.GetUserById(_token, userId);
        return response?.user;
    }

    public async Task<string> CreateServer(string name)
    {
        var response = await _api.CreateServer(_token, name);
        return response?.serverId;
    }

    public async Task<List<string>> GetServerMembers(string serverId)
    {
        return await _api.GetServerMembers(_token, serverId);
    }

    public async Task<string> AddServerMember(string serverId, string userId = null, string email = null)
    {
        var response = await _api.AddServerMember(_token, serverId, userId, email);
        return response?.status;
    }

    public async Task<CreateChannelResponse> CreateChannel(string serverId, string name)
    {
        return await _api.CreateChannel(_token, serverId, name);
    }

    // ---- WebSocket shortcuts ----

    public Task WsPing() => _ws.SendPingAsync();
    public Task WsGetServers() => _ws.SendGetServersAsync();
    public Task WsGetChannels(string serverId) => _ws.SendGetChannelsAsync(serverId);
    public Task WsGetDirectChats() => _ws.SendGetDirectChatsAsync();
    public Task WsCreateChannel(string serverId, string name) => _ws.SendCreateChannelAsync(serverId, name);
    public Task WsSendMessage(string channelId, string text) => _ws.SendMessageAsync(channelId, text);
    public Task WsGetHistory(string channelId, int limit = 30, long? beforeTimestamp = null) => _ws.SendGetHistoryAsync(channelId, limit, beforeTimestamp);
    public Task WsAddMember(string serverId, string userId) => _ws.SendAddMemberAsync(serverId, userId);
}
