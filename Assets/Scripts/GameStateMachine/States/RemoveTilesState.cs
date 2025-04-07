using System;
using System.Collections.Generic;
using System.Threading;
using Animations;
using Cysharp.Threading.Tasks;
using Game.MatchTiles;
using Game.Tiles;
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

        public RemoveTilesState(Grid grid, IStateSwitcher stateSwitcher, IAnimation animation, MatchFinder matchFinder)
        {
            _grid = grid;
            _stateSwitcher = stateSwitcher;
            _animation = animation;
            _matchFinder = matchFinder;
        }

        public async void Enter()
        {
            _cts = new CancellationTokenSource();
            // score ++
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
                var position = grid.WorldToGrid(tile.transform.position);
                grid.SetValue(position.x, position.y, null);
                await _animation.HideTile(tile.gameObject);
                // FX
            }
            _cts.Cancel();
        }
    }
}