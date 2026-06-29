using System;
using TMPro;
using UnityEngine;
using Zenject;

public class InputHandler: IDisposable
{
    private EventHandler _eventHandler;
    private CoffeeCordManager _coffeeCordManager;
    

    [Inject(Id = "LoginField")]
    private readonly TMP_InputField _loginField;

    [Inject(Id = "PasswordField")]
    private readonly TMP_InputField _passwordField;

    [Inject]
    public void Construct(EventHandler eventHandler, CoffeeCordManager coffeeCordManager)
    {
        _coffeeCordManager = coffeeCordManager;

        _eventHandler = eventHandler;
        _eventHandler.OnTryLogin += TryLogin;

        _coffeeCordManager.OnServersList += OnServersList;
    }

    private async void TryLogin()
    {
        Debug.Log("Connecting...");
        await _coffeeCordManager.LoginAndConnect(_loginField.text, _passwordField.text);
        Debug.Log("Fetching server list...");
        await _coffeeCordManager.WsGetServers();
    }

    private void OnServersList(WsServersList serversList)
    {
        Debug.Log("Server list fetched");

        foreach (ServerInfo server in serversList.servers)
        {
            Debug.Log($"Server: {server.serverName} - {server.serverId}");
        }
    }

    public void Dispose()
    {
        _eventHandler.OnTryLogin -= TryLogin;
        _coffeeCordManager.OnServersList -= OnServersList;
    }
}
