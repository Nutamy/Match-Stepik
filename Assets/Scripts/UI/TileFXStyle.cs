using Game.MatchTiles;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Чистая логика вспышки при сборе фишек: цвет берётся у фишки,
    /// у крупных совпадений (4, 5 в ряд, пересечение) — дополнительный большой взрыв.
    /// </summary>
    public static class TileFXStyle
    {
        // Обычная вспышка над каждой фишкой
        public const float TileScale = 1f;

        // Размер большого взрыва в центре группы
        public const float FourScale = 1.7f;
        public const float CrossScale = 2.1f;
        public const float FiveScale = 2.5f;
        // Взрыв бомбы накрывает её квадрат 3×3
        public const float BombScale = 3.2f;
        // Бомба взрывается через столько секунд после того, как её задело или ею сходили
        public const float BombFuse = 0.12f;

        /// <summary>Нужен ли группе большой взрыв.</summary>
        public static bool IsBig(MatchDirection direction) => BigScale(direction) > TileScale;

        /// <summary>Во сколько раз большой взрыв больше обычной вспышки; 1 — большого взрыва нет.</summary>
        public static float BigScale(MatchDirection direction)
        {
            switch (direction)
            {
                case MatchDirection.LongHorizontal:
                case MatchDirection.LongVertical:
                    return FourScale;
                case MatchDirection.Multiply:
                    return CrossScale;
                case MatchDirection.FiveInARow:
                    return FiveScale;
                default:
                    return TileScale;
            }
        }

        /// <summary>
        /// Окрашивает исходный цвет частицы в цвет фишки: RGB умножается, прозрачность остаётся как в префабе.
        /// </summary>
        public static Color Tint(Color baseColor, Color tileColor)
        {
            return new Color(baseColor.r * tileColor.r, baseColor.g * tileColor.g, baseColor.b * tileColor.b, baseColor.a);
        }
    }
}
