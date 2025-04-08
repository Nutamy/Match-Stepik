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
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<MenuEntryPoint>();
            builder.Register<SetupLevelSequence>(Lifetime.Singleton);
            builder.RegisterInstance(_levelSequenceView);
        }
    }
}