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
    [Inject(Id = "LoginScreenCanvas")]
    private readonly CanvasGroup _loginScreenCanvas;
    [Inject(Id = "ServerListCanvas")]
    private readonly CanvasGroup _serverListCanvas;
    [Inject(Id = "ChannelsListCanvas")]
    private readonly CanvasGroup _channelsListCanvas;
    [Inject(Id = "ChatCanvas")]
    private readonly CanvasGroup _chatCanvas;

    [Inject]
    public void Construct(EventHandler eventHandler, CoffeeCordManager coffeeCordManager)
    {
        _coffeeCordManager = coffeeCordManager;

        _eventHandler = eventHandler;
        _eventHandler.OnTryLogin += TryLogin;
        _eventHandler.OnSelectServer += SelectServer;
    }

    private async void TryLogin()
    {
        Debug.Log("Connecting...");
        await _coffeeCordManager.LoginAndConnect(_loginField.text, _passwordField.text);
        Debug.Log("Fetching server list...");
        await _coffeeCordManager.WsGetServers();

        _eventHandler.HideCanvas(_loginScreenCanvas);
        _eventHandler.ShowCanvas(_serverListCanvas);
        _eventHandler.ShowCanvas(_channelsListCanvas);
        _eventHandler.ShowCanvas(_chatCanvas);
    }

    private async void SelectServer(string serverID)
    {
        Debug.Log($"Fetching channel list for server {serverID}");
        await _coffeeCordManager.WsGetChannels(serverID);
    }

    public void Dispose()
    {
        _eventHandler.OnTryLogin -= TryLogin;
    }
}
