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
            // Цветная бомба крутится перед срабатыванием: в пул фишка возвращается ровно
            t.localRotation = Quaternion.identity;
        }

        public void DropTile(Tile tile, Vector3 localPosition, float duration, float delay)
        {
            var t = tile.transform;
            t.DOKill();
            t.localScale = Vector3.one;
            t.DOLocalMove(localPosition, duration).SetDelay(delay)
                .SetEase(Ease.OutBack, 1.1f).SetLink(tile.gameObject);
        }

        public async UniTask ShakeTile(Tile tile, Vector3 localPosition, Vector3 strength, float duration)
        {
            var t = tile.transform;
            // Доводим до конца перемещение и масштаб выбора, чтобы тряска шла от точной клетки
            t.DOKill(true);
            t.localPosition = localPosition;
            await t.DOShakePosition(duration, strength, 16, 0f, false, true)
                .SetEase(Ease.OutQuad).SetLink(tile.gameObject).ToUniTask();
            if (tile == null) return;
            t.localPosition = localPosition;
        }

        public Tween HintPull(Tile tile, Vector3 home, Vector2Int direction, float distance, float pullDuration, float pause)
        {
            var t = tile.transform;
            var baseScale = t.localScale;
            t.localPosition = home;

            var dir = new Vector3(direction.x, direction.y, 0f);
            var pulled = home + dir * distance;
            // Растяжение вдоль направления хода и лёгкое сужение поперёк — фишка «тянется» к соседу
            var stretch = direction.x != 0
                ? Vector3.Scale(baseScale, new Vector3(1.14f, 0.9f, 1f))
                : Vector3.Scale(baseScale, new Vector3(0.9f, 1.14f, 1f));

            float half = pullDuration / 2f;
            var sequence = DOTween.Sequence().SetLink(tile.gameObject);
            for (int i = 0; i < 2; i++)
            {
                sequence.Append(t.DOLocalMove(pulled, half).SetEase(Ease.OutQuad))
                    .Join(t.DOScale(stretch, half).SetEase(Ease.OutQuad))
                    .Append(t.DOLocalMove(home, half).SetEase(Ease.InOutQuad))
                    .Join(t.DOScale(baseScale, half).SetEase(Ease.InOutQuad));
            }

            return sequence.AppendInterval(pause)
                .SetLoops(-1)
                .OnKill(() =>
                {
                    if (tile == null) return;
                    t.localPosition = home;
                    t.localScale = baseScale;
                });
        }

        public void Dispose()
        {
            _cts?.Dispose();
        }
    }
}