using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ChannelList : IDisposable
{
    [Inject(Id = "ChannelParent")]
    private readonly Transform _channelParent;
    [Inject(Id = "ChannelPrefab")]
    private readonly Transform _channelPrefab;

    private List<GameObject> _channels;

    private DiContainer _container;
    private EventHandler _eventHandler;
    private CoffeeCordManager _coffeeCordManager;

    [Inject]
    public void Construct(CoffeeCordManager coffeeCordManager, EventHandler eventHandler, DiContainer container)
    {
        _channels = new List<GameObject>();

        _container = container;

        _coffeeCordManager = coffeeCordManager;
        _coffeeCordManager.OnChannelsList += RefreshChannelsList;

        _eventHandler = eventHandler;
    }

    private void RefreshChannelsList(WsChannelsList channelsList)
    {
        foreach(ChannelInfo channel in channelsList.channels)
        {
            Debug.Log($"{channel.channelName} - {channel.channelId}");
        }

        ResetChannelIcons();

        for(int i = 0; i < channelsList.channels.Count; i++)
        {
            if(i >= _channels.Count)
            {
                _channels.Add(_container.InstantiatePrefab(_channelPrefab, _channelParent));
            }
            _channels[i].SetActive(true);
            ChannelInfo channel = channelsList.channels[i];
            _channels[i].GetComponentInChildren<TMP_Text>().text = channel.channelName;
            _channels[i].GetComponent<Button>().onClick.AddListener(() =>
                _eventHandler.SelectChannel(channel.channelId));
        }
    }

    private void ResetChannelIcons()
    {
        foreach(GameObject channel in _channels)
        {
            channel.SetActive(false);
            channel.GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }

    public void Dispose()
    {
        _coffeeCordManager.OnChannelsList -= RefreshChannelsList;
    }
}
