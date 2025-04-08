using UI.Menu;
using UI.Menu.Levels;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DI
{
    public class MenuScope : LifetimeScope
    {
        [SerializeField] private LevelSequenceView _levelSequenceView;
        [SerializeField] private MenuView _menuView;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<MenuEntryPoint>();
            builder.Register<SetupLevelSequence>(Lifetime.Singleton);
            builder.Register<StartGame>(Lifetime.Singleton);
            builder.RegisterInstance(_levelSequenceView);
            builder.RegisterInstance(_menuView);
        }
    }
}