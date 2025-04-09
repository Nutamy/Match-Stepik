using Animations;
using Audio;
using Data;
using Game.MatchTiles;
using Game.Board;
using Game.GridSystem;
using Game.Score;
using Game.Tiles;
using Game.Utils;
using GameStateMachine;
using Levels;
using ResourcesLoading;
using SceneLoading;
using UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Grid = Game.GridSystem.Grid;

namespace EntryPoint
{
    public class EntryPoint : IInitializable
    {
        // BG Tile Setup
        private LevelConfig _levelConfig;
        private BlankTileSetup _blankTileSetup;
        private GameBoard _gameBoard;
        private GameData _gameData;
        private StateMachine _stateMachine;
        private Grid _grid;
        private IAnimation _animation;
        private MatchFinder _matchFinder;
        private TilePool _tilePool;
        private GameProgress _gameProgress;
        private ScoreCalculator _scoreCalculator;
        private AudioManager _audioManager;
        private IAsyncSceneLoading _sceneLoading;
        private EndGamePanelView _endGame;
        private GameDebug _gameDebug;
        private GameResourcesLoader _gameResourcesLoader;
        private SetupCamera _setupCamera;
        private FXPool _fxPool;

        private bool _isDebuging;
        //FX pool

        public EntryPoint(BlankTileSetup blankTileSetup, GameBoard gameBoard, GameData gameData, Grid grid, IAnimation animation, MatchFinder matchFinder, TilePool tilePool, GameProgress gameProgress, ScoreCalculator scoreCalculator, AudioManager audioManager, IAsyncSceneLoading sceneLoading, EndGamePanelView endGame, GameDebug gameDebug, GameResourcesLoader gameResourcesLoader, SetupCamera setupCamera, FXPool fxPool)
        {
            _blankTileSetup = blankTileSetup;
            _gameBoard = gameBoard;
            _gameData = gameData;
            _grid = grid;
            _animation = animation;
            _matchFinder = matchFinder;
            _tilePool = tilePool;
            _gameProgress = gameProgress;
            _scoreCalculator = scoreCalculator;
            _audioManager = audioManager;
            _sceneLoading = sceneLoading;
            _endGame = endGame;
            _gameDebug = gameDebug;
            _gameResourcesLoader = gameResourcesLoader;
            _setupCamera = setupCamera;
            _fxPool = fxPool;
        }

        public void Initialize()
        {
            _levelConfig = _gameData.CurrentLevel;
            if (_isDebuging)
            {
                _gameDebug.ShowDebug(_gameBoard.transform);
            }
            _grid.SetupGrid(_levelConfig.Width, _levelConfig.Height);
            _gameProgress.LoadLevelConfig(_levelConfig.GoalScore, _levelConfig.Moves);
            // await resources
            _blankTileSetup.SetupBlanks(_levelConfig);
            _setupCamera.SetCamera(_grid.Width, _grid.Height, true);
            _stateMachine = new StateMachine(_gameBoard, _grid, _animation, _matchFinder, _tilePool, _gameProgress, _scoreCalculator, _audioManager, _endGame, _fxPool);
            _sceneLoading.LoadingDone(true);
            
        }
    }
}