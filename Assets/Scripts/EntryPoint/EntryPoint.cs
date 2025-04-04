using Animations;
using Game.Board;
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

        private void Start()
        {
            _stateMachine = new StateMachine(_gameBoard, _grid, _animation);
        }

        [Inject]
        private void Construct(Grid grid, IAnimation animation)
        {
            _grid = grid;
            _animation = animation;
        }
    }
}