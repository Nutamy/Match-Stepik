using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Tiles;
using UnityEngine;

namespace Animations
{
    public class AnimationManager : IAnimation, IDisposable
    {
        private CancellationTokenSource _cts;
        
        public async UniTask Reveal(GameObject target, float delay)
        {
            _cts = new CancellationTokenSource();
            target.transform.localScale = Vector3.one * 0.1f;
            await target.transform.DOScale(Vector3.one, delay).SetEase(Ease.OutBounce).SetLink(target);
            _cts.Cancel();
        }

        public async UniTask HideTile(GameObject target)
        {
            _cts = new CancellationTokenSource();
            target.transform.DOScale(Vector3.zero, 0.05f).SetEase(Ease.OutBounce).SetLink(target);
            target.SetActive(false);
            target.transform.localScale = Vector3.one;
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f), _cts.IsCancellationRequested);
            _cts.Cancel();
        }

        public void PunchScoreText(GameObject target)
        {
            // ОСТАНАВЛИВАЕМ все текущие анимации на этом объекте
            target.transform.DOKill();

            // Сбрасываем размер в 1, чтобы анимация всегда начиналась с нормы
            target.transform.localScale = Vector3.one;

            // Запускаем пульсацию
            target.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.2f, 10, 1).SetLink(target);
        }

        public void DoPunchAnimate(GameObject target, Vector3 scale, float duration)
        {
            target.transform.DOPunchScale(scale, duration, 1, 0.5f).SetLink(target);
        }

        public void MoveUI(RectTransform target, Vector3 position, float duration, Ease ease)
        {
            target.DOAnchorPos(position, duration).SetEase(ease).SetLink(target.gameObject);
        }

        public void AnimateTile(Tile tile, float value)
        {
            tile.transform.DOScale(value, 0.3f).SetEase(Ease.OutCubic).SetLink(tile.gameObject);
        }

        public void MoveTile(Tile tile, Vector3 position, Ease ease)
        {
            tile.transform.DOLocalMove(position, 0.2f).SetEase(ease).SetLink(tile.gameObject);
        }

        public async UniTask PopOut(GameObject target, float duration)
        {
            var t = target.transform;
            t.DOKill();
            var sequence = DOTween.Sequence().SetLink(target)
                .Append(t.DOScale(Vector3.one * 1.15f, duration * 0.35f).SetEase(Ease.OutQuad))
                .Append(t.DOScale(Vector3.zero, duration * 0.65f).SetEase(Ease.InBack));
            await sequence.ToUniTask();
            if (target == null) return;
            target.SetActive(false);
            t.localScale = Vector3.one;
        }

        public void DropTile(Tile tile, Vector3 localPosition, float duration, float delay)
        {
            var t = tile.transform;
            t.DOKill();
            t.localScale = Vector3.one;
            t.DOLocalMove(localPosition, duration).SetDelay(delay)
                .SetEase(Ease.OutBack, 1.1f).SetLink(tile.gameObject);
        }

        public void Dispose()
        {
            _cts?.Dispose();
        }
    }
}