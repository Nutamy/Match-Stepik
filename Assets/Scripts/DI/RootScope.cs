using Boot;
using SceneLoading;
using Animations;
using Audio;
using Data;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DI
{
    public class RootScope : LifetimeScope
    {
        [SerializeField] private LoadingView _loadingView;
        [SerializeField] private AudioManager _audioManager;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<BootEntryPoint>();
            builder.Register<GameData>(Lifetime.Singleton);
            builder.Register<IAsyncSceneLoading, AsyncSceneLoading>(Lifetime.Singleton);
            builder.Register<IAnimation, AnimationManager>(Lifetime.Singleton);
            builder.RegisterInstance(_loadingView);
            builder.RegisterInstance(_audioManager);
        }
    }
}