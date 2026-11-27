using System;
using System.Collections.Generic;
using System.Threading;
using Animations;
using Audio;
using Game.Board;
using Game.MatchTiles;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Score;
using Game.Tiles;
using UnityEngine;
using Grid = Game.GridSystem.Grid;

namespace GameStateMachine.States
{
    public class RefillGridState : IState, IDisposable
    {
        private CancellationTokenSource _cts;
        private Grid _grid;
        private IStateSwitcher _stateSwitcher;
        private IAnimation _animation;
        private MatchFinder _matchFinder;
        private TilePool _tilePool;
        private readonly Transform _parent;
        private GameProgress _gameProgress;
        private AudioManager _audioManager;
        private GameBoard _gameBoard;
        
        public RefillGridState(Grid grid, IStateSwitcher stateSwitcher, IAnimation animation, 
            MatchFinder matchFinder, TilePool tilePool, Transform parent, GameProgress gameProgress, AudioManager audioManager, GameBoard gameBoard)
        {
            _grid = grid;
            _stateSwitcher = stateSwitcher;
            _animation = animation;
            _matchFinder = matchFinder;
            _tilePool = tilePool;
            _parent = parent;
            _gameProgress = gameProgress;
            _audioManager= audioManager;
            _gameBoard = gameBoard;
        }

        public async void Enter()
        {
            //_cts = new CancellationTokenSource();
            Debug.Log("Enter in RefillGridState");
            await DropAndRefill();
            if (_matchFinder.CheckBoardForMatches(_grid))
            {
                // Звук удаления проигрывает RemoveTilesState — здесь не дублируем
                _stateSwitcher.SwichState<RemoveTilesState>();
            }
            else
            {
                // Play sound check game over
                _audioManager.PlayNoMatch();
                await CheckEndGame();
            }
        }

        public void Exit()
        {
            _cts?.Cancel();
        }

        public void Dispose()
        {
            _cts?.Dispose();
        }

        // Скорость падения: базовое время + время на каждую клетку пути
        private const float DropBaseDuration = 0.14f;
        private const float DropPerCell = 0.045f;
        private const float DropMaxDuration = 0.45f;
        // Небольшая волна слева направо
        private const float ColumnStagger = 0.025f;

        /// <summary>
        /// Один проход по столбцам: оставшиеся фишки падают в пустоты, новые появляются над полем
        /// и падают следом. Все движения идут одновременно, ждём только самое долгое.
        /// </summary>
        private async UniTask DropAndRefill()
        {
            _cts = new CancellationTokenSource();
            float longest = 0f;
            bool anyMoved = false;

            for (int x = 0; x < _grid.Width; x++)
            {
                float delay = x * ColumnStagger;
                int spawned = 0;

                for (int y = 0; y < _grid.Height; y++)
                {
                    if (_grid.GetValue(x, y)) continue;

                    // Ищем ближайшую подвижную фишку выше (пустые клетки уровня пропускаем)
                    int from = -1;
                    for (int i = y + 1; i < _grid.Height; i++)
                    {
                        var above = _grid.GetValue(x, i);
                        if (above == null || above.IsInteractable == false) continue;
                        from = i;
                        break;
                    }

                    Tile tile;
                    int distance;
                    if (from >= 0)
                    {
                        tile = _grid.GetValue(x, from);
                        _grid.SetValue(x, from, null);
                        distance = from - y;
                    }
                    else
                    {
                        // Новая фишка стартует над полем, каждая следующая в столбце — ещё выше
                        int startY = _grid.Height + spawned;
                        spawned++;
                        tile = _tilePool.GetTile(_grid.GridToWorld(x, startY), _parent);
                        tile.gameObject.SetActive(true);
                        distance = startY - y;
                    }

                    _grid.SetValue(x, y, tile);
                    float duration = Mathf.Min(DropBaseDuration + DropPerCell * distance, DropMaxDuration);
                    _animation.DropTile(tile, _grid.GridToWorld(x, y), duration, delay);
                    longest = Mathf.Max(longest, delay + duration);
                    anyMoved = true;
                }
            }

            if (!anyMoved) return;

            _audioManager.PlayWhoosh();
            await UniTask.Delay(TimeSpan.FromSeconds(longest), cancellationToken: _cts.Token)
                .SuppressCancellationThrow();
            _audioManager.PlayPop();
        }

        private async UniTask CheckEndGame()
        {
            if (_gameProgress.CheckGoalScore())
            {
                _audioManager.PlayWin();
                _stateSwitcher.SwichState<WinState>();
            }
            else if (_gameProgress.Moves <= 0)
            {
                _audioManager.PlayLose();
                _stateSwitcher.SwichState<LoseState>();
            }
            else
            {
                // Ходов на поле не осталось — перемешиваем, иначе игрок застрянет
                if (!_gameBoard.HasPossibleMove())
                {
                    _audioManager.PlayWhoosh();
                    await _gameBoard.ShuffleAsync();
                }
                _stateSwitcher.SwichState<PlayerTurnState>();
            }
            
        }
    }
}