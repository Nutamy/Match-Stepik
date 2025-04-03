using Game.Board;
using Game.GridSystem;
using Game.Tiles;
using Game.Utils;
using ResourcesLoading;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Grid = Game.GridSystem.Grid;

namespace DI
{
    public class GameScope :LifetimeScope
    {
        [SerializeField] private GameBoard _gameBoard;
        [SerializeField] private GameResourcesLoader _resourcesLoader;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<Grid>(Lifetime.Singleton);
            builder.Register<GameDebug>(Lifetime.Singleton);
            builder.RegisterInstance(_gameBoard);
            builder.RegisterInstance(_resourcesLoader);
            builder.Register<SetupCamera>(Lifetime.Singleton);
            builder.Register<TilePool>(Lifetime.Singleton);
        }
    }
}