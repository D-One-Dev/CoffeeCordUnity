using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ChatHistoryController : IDisposable
{
    private const int PageSize = 30;

    private readonly List<MessageData> _messages = new();
    private readonly List<GameObject> _pool = new();

    private string _currentChannelId;
    private bool _hasMoreHistory = true;
    private bool _isLoadingHistory;
    private bool _isFirstLoad = true;

    private CoffeeCordManager _coffeeCordManager;
    private EventHandler _eventHandler;
    private DiContainer _container;
    private Transform _contentParent;
    private Transform _messagePrefab;
    private ScrollRect _scrollRect;

    [Inject]
    public void Construct(
        CoffeeCordManager coffeeCordManager,
        EventHandler eventHandler,
        DiContainer container,
        [Inject(Id = "ChatContentParent")] Transform contentParent,
        [Inject(Id = "MessagePrefab")] Transform messagePrefab,
        [Inject(Id = "ChatScrollRect")] ScrollRect scrollRect)
    {
        _coffeeCordManager = coffeeCordManager;
        _eventHandler = eventHandler;
        _container = container;
        _contentParent = contentParent;
        _messagePrefab = messagePrefab;
        _scrollRect = scrollRect;

        _eventHandler.OnSelectChannel += OnChannelSelected;
        _coffeeCordManager.OnHistory += OnHistoryReceived;
        _coffeeCordManager.OnNewMessage += OnNewMessageReceived;
        _scrollRect.onValueChanged.AddListener(OnScrollChanged);
    }

    private async void OnChannelSelected(string channelId)
    {
        _currentChannelId = channelId;
        _messages.Clear();
        _hasMoreHistory = true;
        _isLoadingHistory = false;
        _isFirstLoad = true;

        ResetPool();

        await _coffeeCordManager.WsGetHistory(channelId, PageSize);
    }

    private void OnHistoryReceived(WsHistory history)
    {
        if (history.channelId != _currentChannelId)
            return;

        _isLoadingHistory = false;
        _hasMoreHistory = history.hasMore;

        if (_isFirstLoad)
        {
            _messages.AddRange(history.messages);
            _messages.Sort((a, b) => a.createdAt.CompareTo(b.createdAt));

            BuildPoolForAllMessages();
            _isFirstLoad = false;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentParent as RectTransform);
            _scrollRect.verticalNormalizedPosition = 0f;

            _ = ResolveSenderNamesAsync();
        }
        else
        {
            float prevHeight = _contentParent.GetComponent<RectTransform>().sizeDelta.y;
            float prevPos = _scrollRect.verticalNormalizedPosition;

            _messages.InsertRange(0, history.messages);
            _messages.Sort((a, b) => a.createdAt.CompareTo(b.createdAt));

            BuildPoolForAllMessages();

            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentParent as RectTransform);

            float newHeight = _contentParent.GetComponent<RectTransform>().sizeDelta.y;
            float offset = newHeight - prevHeight;

            if (prevPos > 0.99f)
                _scrollRect.verticalNormalizedPosition = 1f;
            else
            {
                var contentRt = _contentParent as RectTransform;
                contentRt.anchoredPosition += new Vector2(0, offset);
            }

            _ = ResolveSenderNamesAsync();
        }
    }

    private void OnNewMessageReceived(WsNewMessage msg)
    {
        if (msg.channelId != _currentChannelId)
            return;

        _messages.Add(msg.message);

        GameObject view = GetOrCreateView(_messages.Count - 1);
        view.transform.SetAsLastSibling();
        view.SetActive(true);

        string name = _coffeeCordManager.GetUserDisplayName(msg.message.senderId) 
                      ?? ShortSender(msg.message.senderId);
        view.GetComponent<MessageView>().Bind(msg.message, name);

        LayoutRebuilder.MarkLayoutForRebuild(_contentParent as RectTransform);

        if (_scrollRect.verticalNormalizedPosition < 0.05f)
        {
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        if (_coffeeCordManager.GetUserDisplayName(msg.message.senderId) == null)
            _ = FetchSenderNameAsync(msg.message.senderId);
    }

    private void OnScrollChanged(Vector2 position)
    {
        if (position.y > 0.98f && _hasMoreHistory && !_isLoadingHistory && !_isFirstLoad)
            LoadMoreHistory();
    }

    private async void LoadMoreHistory()
    {
        _isLoadingHistory = true;
        long oldest = _messages[0].createdAt;
        await _coffeeCordManager.WsGetHistory(_currentChannelId, PageSize, oldest);
    }

    private async Task ResolveSenderNamesAsync()
    {
        var unknown = _messages
            .Select(m => m.senderId)
            .Distinct()
            .Where(id => _coffeeCordManager.GetUserDisplayName(id) == null)
            .ToList();

        foreach (string id in unknown)
            await FetchSenderNameAsync(id);
    }

    private async Task FetchSenderNameAsync(string senderId)
    {
        try
        {
            await _coffeeCordManager.GetUserById(senderId);
        }
        catch { }

        string name = _coffeeCordManager.GetUserDisplayName(senderId) ?? ShortSender(senderId);

        for (int i = 0; i < _messages.Count && i < _pool.Count; i++)
        {
            if (_pool[i].activeSelf && _messages[i].senderId == senderId)
                _pool[i].GetComponent<MessageView>().Bind(_messages[i], name);
        }
    }

    private void BuildPoolForAllMessages()
    {
        for (int i = 0; i < _messages.Count; i++)
        {
            GameObject view = GetOrCreateView(i);
            view.SetActive(true);
            string name = _coffeeCordManager.GetUserDisplayName(_messages[i].senderId) 
                          ?? ShortSender(_messages[i].senderId);
            view.GetComponent<MessageView>().Bind(_messages[i], name);
        }

        for (int i = _messages.Count; i < _pool.Count; i++)
            _pool[i].SetActive(false);
    }

    private GameObject GetOrCreateView(int index)
    {
        while (index >= _pool.Count)
        {
            GameObject go = _container.InstantiatePrefab(_messagePrefab, _contentParent);
            _pool.Add(go);
        }
        return _pool[index];
    }

    private void ResetPool()
    {
        foreach (GameObject go in _pool)
            go.SetActive(false);
    }

    private static string ShortSender(string senderId)
    {
        return senderId.Length > 8 ? senderId[..8] : senderId;
    }

    public void Dispose()
    {
        _eventHandler.OnSelectChannel -= OnChannelSelected;
        _coffeeCordManager.OnHistory -= OnHistoryReceived;
        _coffeeCordManager.OnNewMessage -= OnNewMessageReceived;
        _scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
    }
}
