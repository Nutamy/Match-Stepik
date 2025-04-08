using System.Collections.Generic;
using System.Linq;
using Animations;
using Audio;
using Game.MatchTiles;
using Game.Board;
using Game.GridSystem;
using Game.Score;
using Game.Tiles;
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
        private MatchFinder _matchFinder;
        private TilePool _tilePool;
        private GameProgress _gameProgress;
        private ScoreCalculator _scoreCalculator;
        private AudioManager _audioManager;

        public StateMachine(GameBoard gameBoard, Grid grid, IAnimation animation, MatchFinder matchFinder, TilePool tilePool, GameProgress gameProgress, ScoreCalculator scoreCalculator, AudioManager audioManager)
        {
            _gameBoard = gameBoard;
            _grid = grid;
            _animation = animation;
            _matchFinder = matchFinder;
            _tilePool = tilePool;
            _gameProgress = gameProgress;
            _scoreCalculator = scoreCalculator;
            _audioManager = audioManager;
            _states = new List<IState>()
            {
                new PrepareState(this, _gameBoard),
                new PlayerTurnState(_grid, this, _animation, _audioManager),
                new SwapTilesState(_grid, this, _animation, _matchFinder, _gameProgress, _audioManager),
                new RemoveTilesState(_grid, this, _animation, _matchFinder, _scoreCalculator, _audioManager),
                new RefillGridState(_grid, this, _animation, _matchFinder, _tilePool, _gameBoard.transform, _gameProgress, _audioManager),
                new WinState(),
                new LoseState()
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