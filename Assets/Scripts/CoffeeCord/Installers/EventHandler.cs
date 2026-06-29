using System;
using UnityEngine;

public class EventHandler : MonoBehaviour
{
    public event Action OnTryLogin;

    public void TryLogin()
    {
        OnTryLogin?.Invoke();
    }
}
