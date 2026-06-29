using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ServerList : IDisposable
{
    [Inject(Id = "ServerIconParent")]
    private Transform _serverIconParent;
    [Inject(Id = "ServerIconPrefab")]
    private Transform _serverIconPrefab;

    private List<GameObject> _serverIcons;

    private CoffeeCordManager _coffeeCordManager;
    private DiContainer _container;
    private EventHandler _eventHandler;

    [Inject]
    public void Construct(CoffeeCordManager coffeeCordManager, DiContainer container, EventHandler eventHandler)
    {
        _serverIcons = new List<GameObject>();

        _eventHandler = eventHandler;

        _container = container;

        _coffeeCordManager = coffeeCordManager;
        _coffeeCordManager.OnServersList += RefreshServersList;
    }

    private void RefreshServersList(WsServersList serverList)
    {
        ResetServerIcons();

        for(int i = 0; i < serverList.servers.Count; i++)
        {
            if(i >= _serverIcons.Count)
            {
                _serverIcons.Add(_container.InstantiatePrefab(_serverIconPrefab, _serverIconParent));
            }
            _serverIcons[i].SetActive(true);
            ServerInfo server = serverList.servers[i];
            _serverIcons[i].GetComponentInChildren<TMP_Text>().text = 
                server.serverName.Length > 3 ? server.serverName[..3] : server.serverName;
            _serverIcons[i].GetComponent<Button>().onClick.AddListener(() =>
                _eventHandler.SelectServer(server.serverId));
        }
    }

    private void ResetServerIcons()
    {
        foreach(GameObject icon in _serverIcons)
        {
            icon.SetActive(false);
        }
    }

    public void Dispose()
    {
        _coffeeCordManager.OnServersList -= RefreshServersList;
    }
}
