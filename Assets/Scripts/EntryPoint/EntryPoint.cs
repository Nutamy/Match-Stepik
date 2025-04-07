using Animations;
using Game.MatchTiles;
using Game.Board;
using Game.Tiles;
using GameStateMachine;
using UnityEngine;
using VContainer;
using Grid = Game.GridSystem.Grid;

namespace EntryPoint
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private GameBoard _gameBoard;
        private StateMachine _stateMachine;
        private Grid _grid;
        private IAnimation _animation;
        private MatchFinder _matchFinder;
        private TilePool _tilePool;

        private void Start()
        {
            _stateMachine = new StateMachine(_gameBoard, _grid, _animation, _matchFinder, _tilePool);
        }

        [Inject]
        private void Construct(Grid grid, IAnimation animation, MatchFinder matchFinder, TilePool tilePool)
        {
            _grid = grid;
            _animation = animation;
            _matchFinder = matchFinder;
            _tilePool = tilePool;
        }
    }
}