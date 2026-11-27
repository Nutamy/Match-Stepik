using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UI.Menu;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Всплывающие «+20» над группой, которые улетают к счётчику, и похвала на длинном каскаде.
    /// Надписи — копии текста счётчика (тот же шрифт и стиль), живут в его канвасе и переиспользуются.
    /// </summary>
    public class ScorePopups
    {
        private const float PointsScale = 0.8f;
        private const float PraiseScale = 1.4f;
        private static readonly Color PraiseColor = new Color(1f, 0.86f, 0.3f);

        private readonly GameProgressView _view;
        private readonly List<TMP_Text> _pool = new List<TMP_Text>();
        // Свой оверлей-канвас поверх фишек: канвас счётчика фоновый и рисуется позади поля
        private RectTransform _canvasRect;
        private Camera _scoreCamera;
        private const int PopupSortingOrder = 100;

        public ScorePopups(GameProgressView view)
        {
            _view = view;
        }

        public void ShowPoints(Vector3 worldPosition, int points)
        {
            if (points <= 0 || !TryPrepare()) return;

            var text = Get($"+{points}", PointsScale, _view.ScoreText.color);
            var rect = text.rectTransform;
            var start = WorldToCanvas(worldPosition);
            var target = ScreenToCanvas(RectTransformUtility.WorldToScreenPoint(_scoreCamera, _view.ScoreText.rectTransform.position));

            rect.anchoredPosition = start;
            rect.localScale = Vector3.zero;

            DOTween.Sequence().SetLink(text.gameObject)
                // Появление с небольшим перелётом масштаба
                .Append(rect.DOScale(1.15f, 0.14f).SetEase(Ease.OutBack))
                .Append(rect.DOScale(1f, 0.08f))
                // Короткая пауза с подъёмом, чтобы цифру успели прочитать
                .Append(rect.DOAnchorPos(start + new Vector2(0f, 40f), 0.25f).SetEase(Ease.OutQuad))
                // Полёт к счётчику
                .Append(rect.DOAnchorPos(target, 0.42f).SetEase(Ease.InQuad))
                .Join(rect.DOScale(0.5f, 0.42f).SetEase(Ease.InQuad))
                .Join(text.DOFade(0.4f, 0.42f).SetEase(Ease.InQuad))
                .OnComplete(() =>
                {
                    text.gameObject.SetActive(false);
                    if (_view != null) PunchCounter();
                });
        }

        public void ShowPraise(Vector3 worldPosition, string message)
        {
            if (!TryPrepare()) return;

            var text = Get(message, PraiseScale, PraiseColor);
            var rect = text.rectTransform;
            var start = WorldToCanvas(worldPosition);
            rect.anchoredPosition = start;
            rect.localScale = Vector3.zero;
            rect.localRotation = Quaternion.Euler(0f, 0f, -8f);

            DOTween.Sequence().SetLink(text.gameObject)
                .Append(rect.DOScale(1.2f, 0.22f).SetEase(Ease.OutBack))
                .Join(rect.DOLocalRotate(Vector3.zero, 0.22f).SetEase(Ease.OutBack))
                .Append(rect.DOScale(1f, 0.12f))
                .AppendInterval(0.55f)
                .Append(rect.DOAnchorPos(start + new Vector2(0f, 60f), 0.35f).SetEase(Ease.InQuad))
                .Join(text.DOFade(0f, 0.35f))
                .OnComplete(() =>
                {
                    rect.localRotation = Quaternion.identity;
                    text.gameObject.SetActive(false);
                });
        }

        private bool TryPrepare()
        {
            if (_view == null || _view.ScoreText == null) return false;
            if (_canvasRect != null) return true;

            var source = _view.ScoreText.canvas.rootCanvas;
            _scoreCamera = source.renderMode == RenderMode.ScreenSpaceOverlay ? null : source.worldCamera;

            var go = new GameObject("ScorePopupsCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            // Канвас живёт в сцене уровня и выгружается вместе с ней
            SceneManager.MoveGameObjectToScene(go, _view.gameObject.scene);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = PopupSortingOrder;

            // Тот же масштаб интерфейса, что у счётчика, чтобы размер надписей совпадал
            var scaler = go.GetComponent<CanvasScaler>();
            var sourceScaler = source.GetComponent<CanvasScaler>();
            if (sourceScaler != null)
            {
                scaler.uiScaleMode = sourceScaler.uiScaleMode;
                scaler.referenceResolution = sourceScaler.referenceResolution;
                scaler.screenMatchMode = sourceScaler.screenMatchMode;
                scaler.matchWidthOrHeight = sourceScaler.matchWidthOrHeight;
                scaler.referencePixelsPerUnit = sourceScaler.referencePixelsPerUnit;
            }

            _canvasRect = (RectTransform)go.transform;
            return true;
        }

        private TMP_Text Get(string message, float sizeScale, Color color)
        {
            TMP_Text text = null;
            foreach (var pooled in _pool)
            {
                if (pooled != null && !pooled.gameObject.activeSelf)
                {
                    text = pooled;
                    break;
                }
            }

            if (text == null)
            {
                var template = _view.ScoreText;
                text = Object.Instantiate(template, _canvasRect);
                text.name = "ScorePopup";
                text.raycastTarget = false;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                text.alignment = TextAlignmentOptions.Center;
                var rect = text.rectTransform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(600f, 200f);
                _pool.Add(text);
            }

            text.transform.DOKill();
            text.DOKill();
            text.text = message;
            text.fontSize = _view.ScoreText.fontSize * sizeScale;
            text.color = color;
            text.alpha = 1f;
            text.transform.SetAsLastSibling();
            text.gameObject.SetActive(true);
            return text;
        }

        private void PunchCounter()
        {
            var counter = _view.ScoreText.transform;
            counter.DOKill(true);
            counter.localScale = Vector3.one;
            counter.DOPunchScale(Vector3.one * 0.25f, 0.2f, 8, 1f).SetLink(counter.gameObject);
        }

        private Vector2 WorldToCanvas(Vector3 worldPosition) =>
            ScreenToCanvas(Camera.main.WorldToScreenPoint(worldPosition));

        private Vector2 ScreenToCanvas(Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPoint, null, out var local);
            return local;
        }
    }
}
