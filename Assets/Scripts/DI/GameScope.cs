using Game.Board;
using Game.GridSystem;
using Game.MatchTiles;
using Game.Score;
using Game.Tiles;
using Game.Utils;
using ResourcesLoading;
using UI;
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
        [SerializeField] private EndGamePanelView _endGame;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<EntryPoint.EntryPoint>();
            builder.Register<Grid>(Lifetime.Singleton);
            builder.Register<GameDebug>(Lifetime.Singleton);
            builder.RegisterInstance(_gameBoard);
            builder.RegisterInstance(_resourcesLoader);
            builder.RegisterInstance(_endGame);
            builder.Register<BlankTileSetup>(Lifetime.Singleton);
            builder.Register<SetupCamera>(Lifetime.Singleton);
            builder.Register<TilePool>(Lifetime.Singleton);
            builder.Register<MatchFinder>(Lifetime.Singleton);
            builder.Register<GameProgress>(Lifetime.Singleton);
            builder.Register<ScoreCalculator>(Lifetime.Singleton);
            builder.Register<EndGame>(Lifetime.Singleton);
        }
    }
}