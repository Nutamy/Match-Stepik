using DG.Tweening;
using Game.Tiles;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Срабатывание цветной бомбы: бомба раскручивается, из неё к каждой своей фишке летит искра со шлейфом.
    /// Когда искра долетает, фишка исчезает или (в комбо) превращается в ракету или бомбу.
    /// </summary>
    public static class ColorBombFX
    {
        // Бомба крутится столько, прежде чем полетят первые искры
        public const float SparkLead = 0.45f;
        // Полёт одной искры
        public const float SparkFlight = 0.3f;
        // Искры вылетают по очереди, от ближних к дальним, с таким шагом...
        public const float SparkStagger = 0.06f;
        // ...но все вместе укладываются в столько секунд, даже если фишек много
        public const float MaxSparkSpread = 0.9f;

        private const float SparkSize = 0.45f;   // в клетках
        private const int SortingOrder = 21;     // над фишками и ракетами

        /// <summary>Шаг между искрами, если всего их count.</summary>
        public static float StaggerFor(int count) =>
            count <= 1 ? 0f : Mathf.Min(SparkStagger, MaxSparkSpread / (count - 1));

        /// <summary>Через сколько секунд после начала срабатывания долетит искра номер index.</summary>
        public static float ArrivalFor(int index, int count) => SparkLead + index * StaggerFor(count) + SparkFlight;

        /// <summary>
        /// Бомба крутится и чуть растёт duration секунд, ускоряясь. Поворот сбрасывается, когда фишку убирают
        /// (PopOut и пул фишек ставят её ровно), поэтому здесь его не возвращаем.
        /// </summary>
        public static void Spin(Tile tile, float duration)
        {
            if (tile == null) return;
            var t = tile.transform;
            float turns = Mathf.Max(1f, Mathf.Round(duration * 2.5f));
            t.DOLocalRotate(new Vector3(0f, 0f, -360f * turns), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.InQuad)
                .SetLink(tile.gameObject);
            t.DOScale(Vector3.one * 1.2f, Mathf.Min(0.3f, duration)).SetEase(Ease.OutBack).SetLink(tile.gameObject);
        }

        /// <summary>Искра из клетки from в клетку to (локальные координаты поля) в цвет фишки.</summary>
        public static void Spark(Transform board, Vector3 from, Vector3 to, Color color, float delay)
        {
            var go = new GameObject("ColorBombSpark");
            go.transform.SetParent(board, false);
            go.transform.localPosition = from;
            go.SetActive(false);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpecialSprites.Spark;
            renderer.sortingOrder = SortingOrder;
            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            var bright = Color.Lerp(color, Color.white, 0.5f);
            renderer.color = bright;
            go.transform.localScale = Vector3.one * (SparkSize / Mathf.Max(0.0001f, renderer.sprite.bounds.size.x));

            var trail = go.AddComponent<TrailRenderer>();
            trail.sharedMaterial = renderer.sharedMaterial;
            trail.sortingOrder = SortingOrder - 1;
            trail.time = 0.15f;
            trail.minVertexDistance = 0.04f;
            trail.widthMultiplier = 0.2f * board.lossyScale.x;
            trail.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            trail.colorGradient = new Gradient
            {
                colorKeys = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(color, 1f) },
                alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            };

            // Лёгкая дуга вбок, чтобы искры расходились веером, а не летели по линейке
            var side = Vector3.Cross(to - from, Vector3.forward).normalized * 0.35f;
            var mid = (from + to) * 0.5f + side * Random.Range(-1f, 1f);

            DOTween.Sequence().SetLink(go)
                .AppendInterval(delay)
                .AppendCallback(() => go.SetActive(true))
                .Append(go.transform.DOLocalPath(new[] { mid, to }, SparkFlight, PathType.CatmullRom).SetEase(Ease.InQuad))
                .Append(renderer.DOFade(0f, 0.06f))
                .OnComplete(() => Object.Destroy(go, trail.time));
        }
    }
}
