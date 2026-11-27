using System;
using System.Threading;
using Animations;
using Audio;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Board;
using Game.Tiles;
using Input;
using UnityEngine;
using Grid = Game.GridSystem.Grid;

namespace GameStateMachine.States
{
    public class PlayerTurnState : IState, IDisposable
    {
        private readonly Vector2Int _emptyPosition = Vector2Int.one * -1;
        private readonly InputReader _inputReader;
        private readonly Grid _grid;
        private readonly IStateSwitcher _stateSwitcher;
        private readonly Camera _camera;
        private IAnimation _animation;
        private AudioManager _audioManager;
        private readonly GameBoard _gameBoard;

        // Порог свайпа в клетках поля: меньше — это нажатие, больше — обмен с соседом
        private const float SwipeThreshold = 0.4f;

        // Подсказка: через HintDelay секунд без касаний две фишки хода тянутся навстречу друг другу
        private const float HintDelay = 5f;
        private const float HintPullDistance = 0.22f;   // в клетках
        private const float HintPullDuration = 0.36f;   // туда и обратно
        private const float HintPause = 1.1f;

        private CancellationTokenSource _hintCts;
        private Tween _hintPullA;
        private Tween _hintPullB;

        private bool _isPressed;
        private bool _pressedOnSelected;
        private Vector2Int _pressCell;
        private Vector2 _pressWorld;

        public PlayerTurnState(Grid grid, IStateSwitcher stateSwitcher, IAnimation animation, AudioManager audioManager, GameBoard gameBoard)
        {
            _gameBoard = gameBoard;
            _inputReader =  new InputReader();
            _grid = grid;
            _stateSwitcher = stateSwitcher;
            _animation = animation;
            _audioManager = audioManager;
            _camera = Camera.main;
            _inputReader.PointerDown += OnPointerDown;
            _inputReader.PointerMoved += OnPointerMoved;
            _inputReader.PointerUp += OnPointerUp;
        }

        private Vector2 PointerWorld() => _camera.ScreenToWorldPoint(_inputReader.Position());

        private void OnPointerDown()
        {
            // Любое касание убирает подсказку и заново запускает отсчёт бездействия
            RestartHintTimer();

            var world = PointerWorld();
            var cell = _grid.WorldToGrid(world);
            if (!IsPlayable(cell)) return;

            var selected = _grid.CurrentPosition;
            // Два нажатия: выбрана фишка, нажали соседнюю — меняем
            if (selected != _emptyPosition && IsSwapable(selected, cell))
            {
                Swap(selected, cell);
                return;
            }

            _pressedOnSelected = selected == cell;
            if (!_pressedOnSelected) Select(cell);

            _isPressed = true;
            _pressCell = cell;
            _pressWorld = world;
        }

        private void OnPointerMoved()
        {
            if (!_isPressed) return;

            // Свайп срабатывает, как только палец ушёл дальше порога, не дожидаясь отпускания
            var direction = SwipeGesture.Direction(PointerWorld() - _pressWorld, SwipeThreshold);
            if (direction == Vector2Int.zero) return;

            _isPressed = false;
            var target = _pressCell + direction;
            if (IsPlayable(target))
            {
                Swap(_pressCell, target);
            }
        }

        private void OnPointerUp()
        {
            if (!_isPressed) return;
            _isPressed = false;

            // Нажали на уже выбранную фишку и отпустили без свайпа — снимаем выбор
            if (_pressedOnSelected)
            {
                _audioManager.PlayDeselect();
                DeselectTile();
            }
        }

        private void Select(Vector2Int cell)
        {
            if (_grid.CurrentPosition != _emptyPosition)
            {
                _animation.AnimateTile(_grid.GetValue(_grid.CurrentPosition.x, _grid.CurrentPosition.y), 1f);
            }

            _audioManager.PlayClick();
            _grid.SetCurrentPosition(cell);
            _animation.AnimateTile(_grid.GetValue(cell.x, cell.y), 1.2f);
        }

        private void Swap(Vector2Int from, Vector2Int to)
        {
            _isPressed = false;
            if (_grid.CurrentPosition != _emptyPosition && _grid.CurrentPosition != from)
            {
                _animation.AnimateTile(_grid.GetValue(_grid.CurrentPosition.x, _grid.CurrentPosition.y), 1f);
            }

            _grid.SetCurrentPosition(from);
            _grid.SetTargetPosition(to);
            _animation.AnimateTile(_grid.GetValue(from.x, from.y), 1f);
            _stateSwitcher.SwichState<SwapTilesState>();
        }

        private bool IsPlayable(Vector2Int cell) => IsValidPosition(cell) && !IsBlankPosition(cell);

        public void Enter()
        {
            _isPressed = false;
            _inputReader.EnableInputs(true);
            DeselectTile();
            RestartHintTimer();
        }

        public void Exit()
        {
            _inputReader.EnableInputs(false);
            CancelHint();
        }

        private void RestartHintTimer()
        {
            CancelHint();
            _hintCts = new CancellationTokenSource();
            ShowHintAfterDelay(_hintCts.Token).Forget();
        }

        private async UniTaskVoid ShowHintAfterDelay(CancellationToken token)
        {
            bool cancelled = await UniTask.Delay(TimeSpan.FromSeconds(HintDelay), cancellationToken: token)
                .SuppressCancellationThrow();
            if (cancelled || !_gameBoard.TryFindMove(out var from, out var to)) return;

            var direction = to - from;
            _hintPullA = _animation.HintPull(_grid.GetValue(from.x, from.y), _grid.GridToWorld(from.x, from.y),
                direction, HintPullDistance, HintPullDuration, HintPause);
            _hintPullB = _animation.HintPull(_grid.GetValue(to.x, to.y), _grid.GridToWorld(to.x, to.y),
                -direction, HintPullDistance, HintPullDuration, HintPause);
        }

        private void CancelHint()
        {
            _hintCts?.Cancel();
            _hintCts?.Dispose();
            _hintCts = null;

            // Kill возвращает фишки ровно в их клетки и прежний размер
            _hintPullA?.Kill();
            _hintPullB?.Kill();
            _hintPullA = _hintPullB = null;
        }

        public void Dispose()
        {
            CancelHint();
            _inputReader.PointerDown -= OnPointerDown;
            _inputReader.PointerMoved -= OnPointerMoved;
            _inputReader.PointerUp -= OnPointerUp;
            _inputReader.Dispose();
        }
        
        private void DeselectTile()
        {
            _animation.AnimateTile(_grid.GetValue(_grid.CurrentPosition.x, _grid.CurrentPosition.y), 1f);
            _grid.SetCurrentPosition(_emptyPosition);
            _grid.SetTargetPosition(_emptyPosition);

        }

        private bool IsBlankPosition(Vector2Int gridPos)
        {
            return _grid.GetValue(gridPos.x, gridPos.y).TileConfig.TileKind == TileKind.Blank;
        }

        private bool IsValidPosition(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.x < _grid.Width && gridPos.y >= 0 && gridPos.y < _grid.Height;
        }

        private bool IsSwapable(Vector2Int currentTilePos, Vector2Int targetTilePos) => 
            Mathf.Abs(currentTilePos.x - targetTilePos.x) + 
            Mathf.Abs(currentTilePos.y - targetTilePos.y) == 1;
        
    }
}