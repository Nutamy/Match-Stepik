using System;
using System.Collections.Generic;
using System.Threading;
using Animations;
using Audio;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace UI.Menu
{
    public class MenuView : MonoBehaviour
    {
        [Header("Castle Parts")]
        [SerializeField] private RectTransform _leftTower;
        [SerializeField] private RectTransform _rightTower;
        [SerializeField] private RectTransform _wall;
        [SerializeField] private RectTransform _logo;

        private IAnimation _animation;
        private AudioManager _audioManager;
        private CancellationTokenSource _cts;

        [Inject]
        private void Construct(IAnimation animation, AudioManager audioManager)
        {
            _animation = animation;
            _audioManager = audioManager;
        }

        private void OnDestroy()
        {
            // Важно: отменяем все задержки при уничтожении объекта
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();

                _leftTower.DOKill();
                _rightTower.DOKill();
                _wall.DOKill();
                _logo.DOKill();
            }
        }

        public async UniTask StartAnimation(List<StartLevelButton> spawnedButtons)
        {
            // Инициализируем токен отмены
            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            try
            {
                // 1. Анимация элементов замка
                // Запускаем башни одновременно
                _animation.MoveUI(_leftTower, new Vector3(-560f, -440f, 0), 0.9f, Ease.InBounce);
                _animation.MoveUI(_rightTower, new Vector3(-380f, 200f, 0), 0.9f, Ease.OutCubic);

                await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: _cts.Token);

                // Стена и Лого
                _animation.MoveUI(_wall, new Vector3(0f, 360f, 0), 0.9f, Ease.OutCubic);
                _animation.MoveUI(_logo, new Vector3(370f, 630f, 0), 0.9f, Ease.OutBounce);

                // Ждем завершения основной анимации фасада
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: _cts.Token);

                // 2. Анимация кнопок уровней
                if (spawnedButtons != null && spawnedButtons.Count > 0)
                {
                    await AnimateButtons(spawnedButtons);
                }
            }
            catch (OperationCanceledException)
            {
                // Игнорируем ошибку отмены задачи
            }
        }

        private async UniTask AnimateButtons(List<StartLevelButton> buttons)
        {
            List<UniTask> animationTasks = new List<UniTask>();

            foreach (var button in buttons)
            {
                if (button == null) continue;

                // Если в SetupButtonsView ты делала кнопку неактивной, включаем её здесь
                button.gameObject.SetActive(true);                

                // Запускаем Reveal. Твой метод Reveal должен внутри себя 
                // плавно менять Scale с 0 до 1 или Alpha с 0 до 1.
                animationTasks.Add(_animation.Reveal(button.gameObject, 0.5f));
            }
            _audioManager.PlayPop();

            // Ждем, пока все кнопки "вырастут" одновременно
            await UniTask.WhenAll(animationTasks);
        }
    }
}