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

            foreach (var blast in _matchFinder.RocketBlasts)
            {
                int points = _scoreCalculator.AddRocketScore(blast.Cleared.Count);
                if (points > 0) _scorePopups.ShowPoints(CellWorld(blast.Origin), points);
            }

            // Длинный каскад — похвала по центру поля
            if (_turnCombo.IsPraiseStep)
            {
                var center = _grid.GridToWorld(0, 0) + new Vector3((_grid.Width - 1) / 2f, (_grid.Height - 1) / 2f, 0f);
                _scorePopups.ShowPraise(_gameBoard.transform.TransformPoint(center), PraiseText);
            }

            CreateRockets();
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

        private Vector3 CellWorld(Vector2Int cell) =>
            _gameBoard.transform.TransformPoint(_grid.GridToWorld(cell.x, cell.y));

        private const float RocketPunch = 0.35f;

        // 4 в ряд: одна фишка группы остаётся на месте и становится ракетой
        private void CreateRockets()
        {
            foreach (var (tile, special) in _matchFinder.RocketsToCreate)
            {
                tile.SetMatched(false);
                tile.SetSpecial(special);
                _animation.DoPunchAnimate(tile.gameObject, Vector3.one * RocketPunch, 0.35f);
            }
        }

        // Ракеты вылетают; фишки на их пути исчезают, когда до них долетает ракета.
        // Возвращает задержку исчезновения для каждой задетой фишки.
        private Dictionary<Tile, float> LaunchRockets()
        {
            var delays = new Dictionary<Tile, float>();
            if (_matchFinder.RocketBlasts.Count == 0) return delays;

            _audioManager.PlayWhoosh();
            var originDelay = new Dictionary<Vector2Int, float>();
            foreach (var blast in _matchFinder.RocketBlasts)
            {
                // Ракета, задетая другой ракетой, стартует, когда до неё долетели
                originDelay.TryGetValue(blast.Origin, out float start);
                var color = Color.white;
                var originTile = _grid.GetValue(blast.Origin.x, blast.Origin.y);
                if (originTile != null) color = TileColor(originTile);
                LaunchAfter(start, blast.Origin, blast.Special, color).Forget();

                foreach (var tile in blast.Cleared)
                {
                    var cell = _grid.WorldToGrid(tile.transform.localPosition);
                    int distance = Mathf.Abs(cell.x - blast.Origin.x) + Mathf.Abs(cell.y - blast.Origin.y);
                    float delay = start + RocketFlightFX.DelayFor(distance);
                    if (!delays.TryGetValue(tile, out var existing) || delay < existing) delays[tile] = delay;
                    if (SpecialTiles.IsRocket(tile.Special) && !originDelay.ContainsKey(cell)) originDelay[cell] = delay;
                }
            }
            return delays;
        }

        private async UniTaskVoid LaunchAfter(float delay, Vector2Int origin, TileSpecial special, Color color)
        {
            if (delay > 0f)
            {
                bool cancelled = await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: _gameBoard.destroyCancellationToken)
                    .SuppressCancellationThrow();
                if (cancelled) return;
            }
            RocketFlightFX.Play(_gameBoard.transform, origin, special, _grid.Width, _grid.Height, color);
        }

        private async UniTask PopOutAfter(Tile tile, float delay)
        {
            if (delay > 0f)
            {
                bool cancelled = await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: _gameBoard.destroyCancellationToken)
                    .SuppressCancellationThrow();
                if (cancelled || tile == null) return;
            }
            _fxPool.PlayFX(tile.transform.position, _gameBoard.transform, TileColor(tile));
            await _animation.PopOut(tile.gameObject, PopDuration);
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

        // У ракеты своего цвета нет — шлейф и вспышка золотые, как сама ракета
        private static readonly Color RocketColor = new Color(1f, 0.78f, 0.3f);

        private static Color TileColor(Tile tile)
        {
            if (tile.Special != TileSpecial.None) return RocketColor;
            return tile.TileConfig != null ? tile.TileConfig.FxColor : Color.white;
        }

        // 4, 5 в ряд и пересечение — большой взрыв в центре группы поверх обычных вспышек
        private void PlayBigExplosions()
        {
            foreach (var result in _matchFinder.AllMatchResults)
            {
                if (!TileFXStyle.IsBig(result.Direction) || result.ConnectedTiles.Count == 0) continue;
                _fxPool.PlayFX(GroupCenter(result), _gameBoard.transform,
                    TileColor(result.ConnectedTiles[0]), TileFXStyle.BigScale(result.Direction));
            }
        }

        // Все совпавшие фишки исчезают одновременно
        private async UniTask RemoveTiles(List<Tile> tilesToRemove, Grid grid)
        {
            PlayGroupSounds();
            PlayBigExplosions();

            var delays = LaunchRockets();

            var animations = new List<UniTask>(tilesToRemove.Count);
            foreach (var tile in tilesToRemove)
            {
                var position = grid.WorldToGrid(tile.transform.localPosition);
                grid.SetValue(position.x, position.y, null);
                delays.TryGetValue(tile, out float delay);
                animations.Add(PopOutAfter(tile, delay));
            }

            await UniTask.WhenAll(animations);
            _cts.Cancel();
        }
    }
}