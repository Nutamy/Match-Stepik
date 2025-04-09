using System;
using System.Collections.Generic;
using System.Threading;
using Animations;
using Audio;
using Cysharp.Threading.Tasks;
using Game.Board;
using Game.MatchTiles;
using Game.Score;
using Game.Tiles;
using UI;
using Grid = Game.GridSystem.Grid;
namespace GameStateMachine.States
{
    public class RemoveTilesState : IState, IDisposable
    {
        private CancellationTokenSource _cts;
        private Grid _grid;
        private IStateSwitcher _stateSwitcher;
        private IAnimation _animation;
        private MatchFinder _matchFinder;
        private GameProgress _gameProgress;
        private ScoreCalculator _scoreCalculator;
        private AudioManager _audioManager;
        private FXPool _fxPool;
        private GameBoard _gameBoard;

        public RemoveTilesState(Grid grid, IStateSwitcher stateSwitcher, IAnimation animation, MatchFinder matchFinder, ScoreCalculator scoreCalculator, AudioManager audioManager, FXPool fxPool, GameBoard gameBoard)
        {
            _grid = grid;
            _stateSwitcher = stateSwitcher;
            _animation = animation;
            _matchFinder = matchFinder;
            _scoreCalculator = scoreCalculator;
            _audioManager = audioManager;
            _fxPool = fxPool;
            _gameBoard = gameBoard;
        }

        public async void Enter()
        {
            _cts = new CancellationTokenSource();
            // score ++
            _scoreCalculator.CalculateScoreToAdd(_matchFinder.CurrentMatchResult.MatchDirection);
            await RemoveTiles(_matchFinder.TilesToRemove, _grid);
            _stateSwitcher.SwichState<RefillGridState>();
        }

        public void Exit()
        {
            _matchFinder.ClearTilesToRemove();
            _cts?.Cancel();
        }

        public void Dispose()
        {
            _cts?.Dispose();
        }

        private async UniTask RemoveTiles(List<Tile> tilesToRemove, Grid grid)
        {
            foreach (var tile in tilesToRemove)
            {
                // play sound
                _audioManager.PlayRemove();
                var position = grid.WorldToGrid(tile.transform.position);
                grid.SetValue(position.x, position.y, null);
                await _animation.HideTile(tile.gameObject);
                // FX
                _fxPool.GetFXFromPool(tile.transform.position, _gameBoard.transform);
            }
            _cts.Cancel();
        }
    }
}