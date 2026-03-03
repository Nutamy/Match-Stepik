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
using System.Threading;
using Cysharp.Threading.Tasks;
using Grid = Game.GridSystem.Grid;

namespace EntryPoint
{
    // Используем IAsyncStartable для поддержки async/await при старте
    public class EntryPoint : IAsyncStartable
    {
        private LevelConfig _levelConfig;
        private readonly BlankTileSetup _blankTileSetup;
        private readonly GameBoard _gameBoard;
        private readonly GameData _gameData;
        private StateMachine _stateMachine;
        private readonly Grid _grid;
        private readonly IAnimation _animation;
        private readonly MatchFinder _matchFinder;
        private readonly TilePool _tilePool;
        private readonly GameProgress _gameProgress;
        private readonly ScoreCalculator _scoreCalculator;
        private readonly AudioManager _audioManager;
        private readonly IAsyncSceneLoading _sceneLoading;
        private readonly EndGamePanelView _endGame;
        private readonly GameDebug _gameDebug;
        private readonly GameResourcesLoader _gameResourcesLoader;
        private readonly SetupCamera _setupCamera;
        private readonly FXPool _fxPool;

        private bool _isDebuging = false;

        public EntryPoint(
            BlankTileSetup blankTileSetup,
            GameBoard gameBoard,
            GameData gameData,
            Grid grid,
            IAnimation animation,
            MatchFinder matchFinder,
            TilePool tilePool,
            GameProgress gameProgress,
            ScoreCalculator scoreCalculator,
            AudioManager audioManager,
            IAsyncSceneLoading sceneLoading,
            EndGamePanelView endGame,
            GameDebug gameDebug,
            GameResourcesLoader gameResourcesLoader,
            SetupCamera setupCamera,
            FXPool fxPool)
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

        // VContainer вызовет этот метод автоматически при старте сцены
        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _levelConfig = _gameData.CurrentLevel;

            // 1. ЖДЕМ ЗАГРУЗКУ РЕСУРСОВ
            // Это критически важно для Android, чтобы не спавнить пустые объекты
            await _gameResourcesLoader.Load();

            // 2. ИНИЦИАЛИЗАЦИЯ ДАННЫХ
            // Теперь ресурсы в Loader гарантированно есть, и TilePool их увидит
            _tilePool.SetCurrentLevelData(_levelConfig);

            if (_isDebuging)
            {
                _gameDebug.ShowDebug(_gameBoard.transform);
            }

            // 3. НАСТРОЙКА СЕТКИ И ГЕЙМПЛЕЯ
            _grid.SetupGrid(_levelConfig.Width, _levelConfig.Height);
            _gameProgress.LoadLevelConfig(_levelConfig.GoalScore, _levelConfig.Moves);

            _blankTileSetup.SetupBlanks(_levelConfig);
            _setupCamera.SetCamera(_grid.Width, _grid.Height, true);

            // 4. ЗАПУСК СТЕЙТ-МАШИНЫ
            _stateMachine = new StateMachine(
                _gameBoard,
                _grid,
                _animation,
                _matchFinder,
                _tilePool,
                _gameProgress,
                _scoreCalculator,
                _audioManager,
                _endGame,
                _fxPool);

            // 5. ЗАВЕРШЕНИЕ ЗАГРУЗКИ
            // Прячем экран загрузки только когда всё готово
            _sceneLoading.LoadingDone(true);
        }
    }
}