using Game.Board;
using GameStateMachine;
using UnityEngine;

namespace EntryPoint
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private GameBoard _gameBoard;
        private StateMachine _stateMachine;

        private void Start()
        {
            _stateMachine = new StateMachine(_gameBoard);
        }
    }
}