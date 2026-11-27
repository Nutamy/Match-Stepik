using DG.Tweening;
using Game.MatchTiles;
using Game.Tiles;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Полёт ракеты: из клетки ракеты в обе стороны по строке или столбцу вылетают две половинки
    /// со шлейфом в цвет фишки и долетают до края поля.
    /// </summary>
    public static class RocketFlightFX
    {
        // Время пролёта одной клетки; фишки на пути исчезают с той же скоростью
        public const float CellTime = 0.035f;

        private const float FlightSize = 0.7f;   // размер летящей ракеты в клетках
        private const int SortingOrder = 20;     // над фишками и вспышками

        /// <summary>Через сколько секунд ракета долетит до клетки на расстоянии distance.</summary>
        public static float DelayFor(int distance) => distance * CellTime;

        /// <param name="board">Поле: координаты клеток — локальные координаты поля.</param>
        /// <param name="origin">Клетка ракеты.</param>
        /// <returns>Длительность самого долгого полёта.</returns>
        public static float Play(Transform board, Vector2Int origin, TileSpecial special, int width, int height, Color color)
        {
            bool horizontal = special == TileSpecial.RocketHorizontal;
            var start = new Vector3(origin.x, origin.y, 0f);
            var axis = horizontal ? Vector3.right : Vector3.up;
            int along = horizontal ? origin.x : origin.y;
            int length = horizontal ? width : height;

            // До края поля и на полклетки за него
            float forward = length - 1 - along + 0.5f;
            float backward = along + 0.5f;

            float a = Launch(board, start, axis, forward, color);
            float b = Launch(board, start, -axis, backward, color);
            return Mathf.Max(a, b);
        }

        private static float Launch(Transform board, Vector3 start, Vector3 direction, float distance, Color color)
        {
            var go = new GameObject("RocketFlight");
            go.transform.SetParent(board, false);
            go.transform.localPosition = start;
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.right, direction);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpecialSprites.RocketFlight;
            renderer.sortingOrder = SortingOrder;
            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            float iconSize = Mathf.Max(renderer.sprite.bounds.size.x, renderer.sprite.bounds.size.y);
            go.transform.localScale = Vector3.one * (FlightSize / Mathf.Max(0.0001f, iconSize));

            var trail = go.AddComponent<TrailRenderer>();
            // Тот же материал, что у спрайтов: есть в любой сборке и в любом конвейере рендера
            trail.sharedMaterial = renderer.sharedMaterial;
            trail.sortingOrder = SortingOrder - 1;
            trail.time = 0.18f;
            trail.minVertexDistance = 0.05f;
            trail.widthMultiplier = 0.32f * board.lossyScale.x;
            trail.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            var bright = Color.Lerp(color, Color.white, 0.45f);
            trail.colorGradient = new Gradient
            {
                colorKeys = new[] { new GradientColorKey(bright, 0f), new GradientColorKey(color, 1f) },
                alphaKeys = new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) }
            };

            float duration = Mathf.Max(CellTime, distance * CellTime);
            go.transform.DOLocalMove(start + direction * distance, duration)
                .SetEase(Ease.Linear)
                .SetLink(go)
                .OnComplete(() => Object.Destroy(go, trail.time));
            renderer.DOFade(0f, 0.08f).SetDelay(duration).SetLink(go);
            return duration;
        }
    }
}
