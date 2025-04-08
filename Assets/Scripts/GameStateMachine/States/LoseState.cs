using UI;
using UnityEngine;

namespace GameStateMachine.States
{
    public class LoseState : IState
    {
        private EndGamePanelView _endGame;

        public LoseState(EndGamePanelView endGame)
        {
            _endGame = endGame;
        }

        public void Enter()
        {
            _endGame.ShowEndGamePanel(false);
            Debug.Log("Lose");
        }

        public void Exit()
        {
            
        }
    }
}