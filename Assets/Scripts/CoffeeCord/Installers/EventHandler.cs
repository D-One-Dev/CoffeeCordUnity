using System;
using UnityEngine;

public class EventHandler : MonoBehaviour
{
    public event Action OnTryLogin;
    public event Action<CanvasGroup> OnHideCanvas;
    public event Action<CanvasGroup> OnShowCanvas;
    public event Action<string> OnSelectServer;
    public event Action<string> OnSelectChannel;

    public void SelectServer(string serverID)
    {
        OnSelectServer?.Invoke(serverID);
    }

    public void SelectChannel(string channelId)
    {
        OnSelectChannel?.Invoke(channelId);
    }

    public void ShowCanvas(CanvasGroup canvas)
    {
        OnShowCanvas?.Invoke(canvas);
    }

    public void HideCanvas(CanvasGroup canvas)
    {
        OnHideCanvas?.Invoke(canvas);
    }

    public void TryLogin()
    {
        OnTryLogin?.Invoke();
    }
}
