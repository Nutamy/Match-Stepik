using System;
using System.Collections.Generic;
using System.Threading;
using Animations;
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
        
        private List<Vector2Int> _tilesToRefillPos = new List<Vector2Int>();
        public RefillGridState(Grid grid, IStateSwitcher stateSwitcher, IAnimation animation, 
            MatchFinder matchFinder, TilePool tilePool, Transform parent, GameProgress gameProgress)
        {
            _grid = grid;
            _stateSwitcher = stateSwitcher;
            _animation = animation;
            _matchFinder = matchFinder;
            _tilePool = tilePool;
            _parent = parent;
            _gameProgress = gameProgress;
        }

        public async void Enter()
        {
            //_cts = new CancellationTokenSource();
            Debug.Log("Enter in RefillGridState");
            await FallTiles();
            await RefillGrid();
            if (_matchFinder.CheckBoardForMatches(_grid))
            {
                // play sound Remove tiles
                _stateSwitcher.SwichState<RemoveTilesState>();
            }
            else
            {
                // Play sound check game over
                CheckEndGame();
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

        private async UniTask FallTiles()
        {
            _cts = new CancellationTokenSource();
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    if(_grid.GetValue(x,y)) continue;
                    for (int i = y + 1; i < _grid.Height; i++)
                    {
                        if(_grid.GetValue(x,i) == null) continue;
                        if (_grid.GetValue(x,i).IsInteractable == false) continue;
                        var tile = _grid.GetValue(x, i);
                        _grid.SetValue(x,y, tile);
                        _animation.MoveTile(tile, _grid.GridToWorld(x,y), Ease.InBack);
                        _grid.SetValue(x,i, null);
                        _tilesToRefillPos.Add(new Vector2Int(x,i));
                        break;
                    }
                }
            }
            // play sound refill grid
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), _cts.IsCancellationRequested);
            _cts?.Cancel();
        }

        private async UniTask RefillGrid()
        {
            _cts = new CancellationTokenSource();
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    if (_grid.GetValue(x, y)) continue;
                    var tile = _tilePool.GetTile(_grid.GridToWorld(x, y), _parent);
                    tile.gameObject.SetActive(true);
                    _grid.SetValue(x,y, tile);
                    _animation.Reveal(tile.gameObject, 0.2f);
                    // play sound create tiles
                    await UniTask.Delay(TimeSpan.FromSeconds(0.1f), _cts.IsCancellationRequested);
                }
            }
            _cts?.Cancel();
        }
        
        private void CheckEndGame()
        {
            if (_gameProgress.CheckGoalScore())
            {
                _stateSwitcher.SwichState<WinState>();
            }
            else if (_gameProgress.Moves <= 0)
            {
                _stateSwitcher.SwichState<LoseState>();
            }
            else
            {
                _stateSwitcher.SwichState<PlayerTurnState>();
            }
            
        }
    }
}