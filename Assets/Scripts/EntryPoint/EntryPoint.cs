using Animations;
using Audio;
using Game.MatchTiles;
using Game.Board;
using Game.Score;
using Game.Tiles;
using GameStateMachine;
using Levels;
using UnityEngine;
using VContainer;
using Grid = Game.GridSystem.Grid;

namespace EntryPoint
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private GameBoard _gameBoard;
        [SerializeField] private LevelConfig _levelConfig;
        private StateMachine _stateMachine;
        private Grid _grid;
        private IAnimation _animation;
        private MatchFinder _matchFinder;
        private TilePool _tilePool;
        private GameProgress _gameProgress;
        private ScoreCalculator _scoreCalculator;
        private AudioManager _audioManager;

        private void Start()
        {
            _stateMachine = new StateMachine(_gameBoard, _grid, _animation, _matchFinder, _tilePool, _gameProgress, _scoreCalculator, _audioManager);
            _gameProgress.LoadLevelConfig(_gameBoard.LevelConfig.GoalScore, _gameBoard.LevelConfig.Moves);
        }

        [Inject]
        private void Construct(Grid grid, IAnimation animation, MatchFinder matchFinder, TilePool tilePool, GameProgress gameProgress, ScoreCalculator scoreCalculator, AudioManager audioManager)
        {
            _grid = grid;
            _animation = animation;
            _matchFinder = matchFinder;
            _tilePool = tilePool;
            _gameProgress = gameProgress;
            _scoreCalculator = scoreCalculator;
            _audioManager = audioManager;
        }
    }
}