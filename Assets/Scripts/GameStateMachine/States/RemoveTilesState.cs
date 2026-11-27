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
        private readonly ScorePopups _scorePopups;
        private readonly TurnCombo _turnCombo;
        private const string PraiseText = "Керемет!";     

        public RemoveTilesState(Grid grid, IStateSwitcher stateSwitcher, IAnimation animation, MatchFinder matchFinder, ScoreCalculator scoreCalculator, AudioManager audioManager, FXPool fxPool, GameBoard gameBoard, ScorePopups scorePopups, TurnCombo turnCombo)
        {
            _grid = grid;
            _stateSwitcher = stateSwitcher;
            _animation = animation;
            _matchFinder = matchFinder;
            _scoreCalculator = scoreCalculator;
            _audioManager = audioManager;
            _fxPool = fxPool;
            _gameBoard = gameBoard;
            _scorePopups = scorePopups;
            _turnCombo = turnCombo;
        }

        public async void Enter()
        {
            _cts = new CancellationTokenSource();

            _turnCombo.Next();
            foreach (var result in _matchFinder.AllMatchResults)
            {
                int points = _scoreCalculator.CalculateScoreToAdd(result.Direction);
                _scorePopups.ShowPoints(GroupCenter(result), points);
            }

            // Длинный каскад — похвала по центру поля
            if (_turnCombo.IsPraiseStep)
            {
                var center = _grid.GridToWorld(0, 0) + new Vector3((_grid.Width - 1) / 2f, (_grid.Height - 1) / 2f, 0f);
                _scorePopups.ShowPraise(_gameBoard.transform.TransformPoint(center), PraiseText);
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

        private static Vector3 GroupCenter(MatchResult result)
        {
            var sum = Vector3.zero;
            foreach (var tile in result.ConnectedTiles) sum += tile.transform.position;
            return sum / Mathf.Max(1, result.ConnectedTiles.Count);
        }

        private const float PopDuration = 0.22f;
        // Один звук на группу; тон выше на каждом шаге каскада, особые группы звучат последними
        private void PlayGroupSounds()
        {
            float pitch = CascadeSound.PitchForStep(_turnCombo.Step);
            var directions = new List<MatchDirection>(_matchFinder.AllMatchResults.Count);
            foreach (var result in _matchFinder.AllMatchResults) directions.Add(result.Direction);

            var order = CascadeSound.PlaybackOrder(directions);
            for (int i = 0; i < order.Count; i++)
                _audioManager.PlayGroup(order[i], pitch, i * CascadeSound.GroupStagger);
        }

        // Все совпавшие фишки исчезают одновременно
        private async UniTask RemoveTiles(List<Tile> tilesToRemove, Grid grid)
        {
            PlayGroupSounds();

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