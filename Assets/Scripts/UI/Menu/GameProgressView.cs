using System;
using Animations;
using Game.Score;
using TMPro;
using UnityEngine;
using VContainer;

namespace UI.Menu
{
    public class GameProgressView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _score;
        [SerializeField] private TMP_Text _goalScore;
        [SerializeField] private TMP_Text _moves;

        private GameProgress _gameProgress;
        private IAnimation _animation;

        private void OnEnable()
        {
            _gameProgress.OnScoreChanged += UpdateScore;
            _gameProgress.OnMove += UpdateMoves;
        }

        private void OnDisable()
        {
            _gameProgress.OnScoreChanged -= UpdateScore;
            _gameProgress.OnScoreChanged -= UpdateScore;
        }

        private void Start()
        {
            _score.text = _gameProgress.Score.ToString();
            _goalScore.text = _gameProgress.GoalScore.ToString();
            _moves.text = _gameProgress.Moves.ToString();
        }

        private void UpdateScore()
        {
            _score.text = _gameProgress.Score.ToString();
            AnimateText(_score.gameObject);
        }
        
        private void UpdateMoves()
        {
            _moves.text = _gameProgress.Moves.ToString();
            AnimateText(_moves.gameObject);
        }

        private void AnimateText(GameObject target)
        {
            _animation.DoPunchAnimate(target, Vector3.one * 0.3f, 0.3f);
        }
        
        [Inject]
        private void Construct(GameProgress gameProgress, IAnimation animation)
        {
            _gameProgress = gameProgress;
            _animation = animation;
        }
    }
}