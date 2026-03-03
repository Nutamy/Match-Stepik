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

        [Inject]
        private void Construct(GameProgress gameProgress, IAnimation animation)
        {
            _gameProgress = gameProgress;
            _animation = animation;
        }

        private void OnEnable()
        {
            // Чтобы не падал NullRef, если OnEnable вызвался раньше Construct
            if (_gameProgress != null) Subscribe();
        }

        private void Start()
        {
            // Если OnEnable пропустил подписку, подписываемся здесь
            Subscribe();

            // ПРИНУДИТЕЛЬНО обновляем текст при старте
            RefreshAllText();
        }

        private void Subscribe()
        {
            // Отписываемся перед подпиской, чтобы не было дублей
            Unsubscribe();
            _gameProgress.OnScoreChanged += UpdateScore;
            _gameProgress.OnMove += UpdateMoves;
        }

        private void Unsubscribe()
        {
            if (_gameProgress == null) return;
            _gameProgress.OnScoreChanged -= UpdateScore;
            _gameProgress.OnMove -= UpdateMoves; // Исправлено (было дублирование Score)
        }

        private void OnDisable() => Unsubscribe();

        private void RefreshAllText()
        {
            _score.text = _gameProgress.Score.ToString();
            _goalScore.text = _gameProgress.GoalScore.ToString(); // Здесь должна появиться цифра!
            _moves.text = _gameProgress.Moves.ToString();
            Debug.Log($"[GameProgressView] UI обновлен: Goal={_goalScore.text}");
        }

        private void UpdateScore()
        {
            _score.text = _gameProgress.Score.ToString();
            _goalScore.text = _gameProgress.GoalScore.ToString();
            AnimateText(_score.gameObject);
        }

        private void UpdateMoves()
        {
            _moves.text = _gameProgress.Moves.ToString();
            AnimateText(_moves.gameObject);
        }

        private void AnimateText(GameObject target)
        {
            if (_animation != null)
                _animation.DoPunchAnimate(target, Vector3.one * 0.3f, 0.3f);
        }
    }
}