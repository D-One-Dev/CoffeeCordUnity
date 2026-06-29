using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SystemInstaller : MonoInstaller
{
    [SerializeField] private EventHandler eventHandler;

    [SerializeField] private TMP_InputField loginField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private CanvasGroup loginScreenCanvas;
    [SerializeField] private CanvasGroup serverListCanvas;
    [SerializeField] private CanvasGroup channelsListCanvas;
    [SerializeField] private CanvasGroup chatCanvas;
    [SerializeField] private Transform serverIconParent;
    [SerializeField] private Transform serverIconPrefab;
    [SerializeField] private Transform channelParent;
    [SerializeField] private Transform channelPrefab;
    [SerializeField] private Transform chatContentParent;
    [SerializeField] private Transform messagePrefab;
    [SerializeField] private ScrollRect chatScrollRect;

    public override void InstallBindings()
    {
        Container.Bind<CoffeeCordManager>()
            .FromNew()
            .AsSingle()
            .NonLazy();
        
        Container.Bind<EventHandler>()
            .FromInstance(eventHandler)
            .AsSingle();
        
        Container.Bind<InputHandler>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        Container.Bind<TMP_InputField>()
            .WithId("LoginField")
            .FromInstance(loginField)
            .AsCached();

        Container.Bind<TMP_InputField>()
            .WithId("PasswordField")
            .FromInstance(passwordField)
            .AsCached();
        
        Container.Bind<CanvasSwitcher>()
            .FromNew()
            .AsSingle()
            .NonLazy();
        
        Container.Bind<CanvasGroup>()
            .WithId("LoginScreenCanvas")
            .FromInstance(loginScreenCanvas)
            .AsCached();
        
        Container.Bind<CanvasGroup>()
            .WithId("ServerListCanvas")
            .FromInstance(serverListCanvas)
            .AsCached();
        
        Container.Bind<CanvasGroup>()
            .WithId("ChannelsListCanvas")
            .FromInstance(channelsListCanvas)
            .AsCached();
        
        Container.Bind<CanvasGroup>()
            .WithId("ChatCanvas")
            .FromInstance(chatCanvas)
            .AsCached();

        Container.Bind<ServerList>()
            .FromNew()
            .AsSingle()
            .NonLazy();
        
        Container.Bind<Transform>()
            .WithId("ServerIconParent")
            .FromInstance(serverIconParent)
            .AsCached();
        
        Container.Bind<Transform>()
            .WithId("ServerIconPrefab")
            .FromInstance(serverIconPrefab)
            .AsCached();
        
        Container.Bind<Transform>()
            .WithId("ChannelParent")
            .FromInstance(channelParent)
            .AsCached();
        
        Container.Bind<Transform>()
            .WithId("ChannelPrefab")
            .FromInstance(channelPrefab)
            .AsCached();

        Container.Bind<Transform>()
            .WithId("ChatContentParent")
            .FromInstance(chatContentParent)
            .AsCached();

        Container.Bind<Transform>()
            .WithId("MessagePrefab")
            .FromInstance(messagePrefab)
            .AsCached();

        Container.Bind<ScrollRect>()
            .WithId("ChatScrollRect")
            .FromInstance(chatScrollRect)
            .AsCached();
        
        Container.Bind<ChannelList>()
            .FromNew()
            .AsSingle()
            .NonLazy();

        Container.Bind<ChatHistoryController>()
            .FromNew()
            .AsSingle()
            .NonLazy();
    }
}