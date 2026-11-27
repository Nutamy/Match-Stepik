using Animations;
using Audio;
using Cysharp.Threading.Tasks;
using Game.Board;
using Game.MatchTiles;
using Game.Score;
using Game.Tiles;
using System;
using System.Collections.Generic;
using System.Threading;
using UI;
using UnityEngine;
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

            foreach (var result in _matchFinder.AllMatchResults)
            {
                _scoreCalculator.CalculateScoreToAdd(result.Direction);
            }

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

        private const float PopDuration = 0.22f;
        private const int MaxRemoveSounds = 3;

        // Все совпавшие фишки исчезают одновременно; один звук на группу (не больше трёх)
        private async UniTask RemoveTiles(List<Tile> tilesToRemove, Grid grid)
        {
            int sounds = Math.Min(_matchFinder.AllMatchResults.Count, MaxRemoveSounds);
            for (int i = 0; i < sounds; i++) _audioManager.PlayRemove();

            var animations = new List<UniTask>(tilesToRemove.Count);
            foreach (var tile in tilesToRemove)
            {
                var position = grid.WorldToGrid(tile.transform.position);
                grid.SetValue(position.x, position.y, null);
                _fxPool.GetFXFromPool(tile.transform.position, _gameBoard.transform);
                animations.Add(_animation.PopOut(tile.gameObject, PopDuration));
            }

            await UniTask.WhenAll(animations);
            _cts.Cancel();
        }
    }
}