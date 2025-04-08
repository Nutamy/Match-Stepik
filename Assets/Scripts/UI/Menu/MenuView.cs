using System;
using System.Collections.Generic;
using System.Threading;
using Animations;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace UI.Menu
{
    public class MenuView : MonoBehaviour
    {
        [SerializeField] private RectTransform _leftTower;
        [SerializeField] private RectTransform _rightTower;
        [SerializeField] private RectTransform _wall;
        [SerializeField] private RectTransform _logo;
        [SerializeField] private List<GameObject> _levelButtons = new List<GameObject>();
        private CancellationTokenSource _cts;
        // audio
        private IAnimation _animation;

        public async UniTask StartAnimation()
        {
            _cts = new CancellationTokenSource();
            // play audio
            _animation.MoveUI(_leftTower, new Vector3(-10f, -70f, 0), 0.9f, Ease.InBounce);
            _animation.MoveUI(_rightTower, new Vector3(10f, -70f, 0), 0.9f, Ease.OutCubic);
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), _cts.IsCancellationRequested);
            _animation.MoveUI(_wall, new Vector3(0f, 300f, 0), 0.9f, Ease.OutCubic);
            _animation.MoveUI(_logo, new Vector3(0f, 140f, 0), 0.9f, Ease.OutBounce);
            
            // Ждём, пока закончится анимация логотипа (можно синхронизировать через TaskCompletionSource при желании)
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), _cts.IsCancellationRequested);

            // Анимация кнопок
            await AnimateButtons();
            // foreach (var button in _levelButtons)
            // {
            //     button.SetActive(true);
            //     await 
            // }
        }
        
        private async UniTask AnimateButtons()
        {
            float delayBetweenButtons = 0.1f;

            foreach (var buttonObj in _levelButtons)
            {
                if (buttonObj == null) continue;

                // Работаем с RectTransform (для UI-кнопок)
                // var rectTransform = buttonObj.GetComponent<RectTransform>();
                // if (rectTransform == null) continue;

                // Сохраняем целевую позицию
                // Vector2 originalPos = rectTransform.anchoredPosition;

                // Убираем кнопку вниз
                //rectTransform.anchoredPosition = new Vector2(originalPos.x, -1000f);
                
                buttonObj.SetActive(true);
                // Анимируем возврат к оригинальной позиции
                //rectTransform.DOAnchorPos(originalPos, 0.6f).SetEase(Ease.OutBack);
                await _animation.Reveal(buttonObj, 0.1f);

                await UniTask.Delay(TimeSpan.FromSeconds(delayBetweenButtons), cancellationToken: _cts.Token);
            }
        }



        [Inject]
        private void Construct(IAnimation animation)
        {
            _animation = animation;
        }
    }
}