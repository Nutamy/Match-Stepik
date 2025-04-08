using System;
using System.Threading;
using Animations;
using Audio;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Score;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI
{
    public class EndGamePanelView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private RectTransform _window;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _title;
        private IAnimation _animation;
        private AudioManager _audioManager;
        private EndGame _endGame;
        private CancellationTokenSource _cts;
        private bool _isWinCondition;

        private readonly string _win = "Victory!\nYou have won!";
        private readonly string _lose = "Defeat!\nYou have lost!";

        private void OnEnable()
        {
            Debug.Log("OnEnableEndGamePanel");
            _closeButton.onClick.AddListener(ExitGame);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(ExitGame);
        }

        public async void ShowEndGamePanel(bool isWinCondition)
        {
            Debug.Log("ShowEndGamePanel isWinCondition = " + isWinCondition);
            _isWinCondition = isWinCondition;
            if (_isWinCondition)
            {
                _title.text = _win;
            }
            else
            {
                _title.text = _lose;
            }

            await StartAnimation();
            _closeButton.interactable = true;
        }

        private async UniTask StartAnimation()
        {
            Debug.Log("StartAnimation END Game Panel");
            _cts = new CancellationTokenSource();
            _panel.SetActive(true);
            _audioManager.PlayWhoosh();
            _animation.MoveUI(_window, new Vector3(0, -151f, 0), 0.9f, Ease.InOutBack);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), _cts.IsCancellationRequested);
        }

        private void ExitGame()
        {
            _endGame.End(_isWinCondition);
            _cts?.Cancel();
        }

        [Inject]
        private void Construct(AudioManager audioManager, EndGame endGame, IAnimation animation)
        {
            _animation = animation;
            _audioManager = audioManager;
            _endGame = endGame;
        }
    }
}