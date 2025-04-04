using Game.Board;
using UnityEngine;

namespace GameStateMachine.States
{
    public class PrepareState :IState
    {
        private readonly IStateSwitcher _stateSwitcher;
        private GameBoard _gameBoard;

        public PrepareState(IStateSwitcher stateSwitcher, GameBoard gameBoard)
        {
            _stateSwitcher = stateSwitcher;
            _gameBoard = gameBoard;
        }

        public void Enter()
        {
            _gameBoard.CreateBoard();
            _stateSwitcher.SwichState<PlayerTurnState>();
        }

        public void Exit()
        {
            Debug.Log("Game has started");
        }
    }
}