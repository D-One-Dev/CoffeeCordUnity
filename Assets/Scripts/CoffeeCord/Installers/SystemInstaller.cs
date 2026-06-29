using TMPro;
using UnityEngine;
using Zenject;

public class SystemInstaller : MonoInstaller
{
    [SerializeField] private EventHandler eventHandler;

    [SerializeField] private TMP_InputField loginField;
    [SerializeField] private TMP_InputField passwordField;

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
    }
}