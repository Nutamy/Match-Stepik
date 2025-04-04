using System.Collections.Generic;
using System.Linq;
using Animations;
using Game.Board;
using Game.GridSystem;
using GameStateMachine.States;

namespace GameStateMachine
{
    public class StateMachine : IStateSwitcher
    {
        private List<IState> _states;
        private IState _currentState;
        private GameBoard _gameBoard;
        private Grid _grid;
        private IAnimation _animation;

        public StateMachine(GameBoard gameBoard, Grid grid, IAnimation animation)
        {
            _gameBoard = gameBoard;
            _grid = grid;
            _animation = animation;
            _states = new List<IState>()
            {
                new PrepareState(this, _gameBoard),
                new PlayerTurnState(_grid, this, _animation),
                new SwapTilesState(_grid, this, _animation),
            };
            _currentState = _states[0];
            _currentState.Enter();
        }

        public void SwichState<T>() where T : IState
        {
            var state = _states.FirstOrDefault(state => state is T);
            _currentState.Exit();
            _currentState = state;
            _currentState?.Enter();
        }
    }
}