using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MatchTiles
{
    /// <summary>Особая фишка вместо обычной. Цвета у неё нет, в линии она не собирается.</summary>
    public enum TileSpecial
    {
        None,
        // Ракета убирает всю строку
        RocketHorizontal,
        // Ракета убирает весь столбец
        RocketVertical,
        // Бомба убирает квадрат 3×3 вокруг себя
        Bomb
    }

    /// <summary>Где появится особая фишка (ракета или бомба) и какая.</summary>
    public readonly struct RocketSpawn
    {
        public Vector2Int Cell { get; }
        public TileSpecial Special { get; }

        public RocketSpawn(Vector2Int cell, TileSpecial special)
        {
            Cell = cell;
            Special = special;
        }
    }

    /// <summary>Сработавшая ракета или бомба: откуда и какие клетки убрала (кроме тех, что и так собраны).</summary>
    public readonly struct RocketBlast
    {
        public Vector2Int Origin { get; }
        public TileSpecial Special { get; }
        public List<Vector2Int> Cleared { get; }

        public RocketBlast(Vector2Int origin, TileSpecial special, List<Vector2Int> cleared)
        {
            Origin = origin;
            Special = special;
            Cleared = cleared;
        }
    }

    /// <summary>Что убрать за шаг: собранные клетки, клетки от ракет и бомб, новые особые фишки.</summary>
    public class SpecialResolution
    {
        public HashSet<Vector2Int> Remove { get; } = new HashSet<Vector2Int>();
        public List<RocketSpawn> Spawns { get; } = new List<RocketSpawn>();
        public List<RocketBlast> Blasts { get; } = new List<RocketBlast>();
    }

    /// <summary>
    /// Чистая логика особых фишек без зависимостей от сцены.
    /// 4 в ряд оставляет на месте одной из фишек ракету поперёк линии,
    /// пересечение линий (T или L) — бомбу в клетке пересечения.
    /// Особая фишка срабатывает, когда её убирают (другой ракетой или бомбой) или ходят ею:
    /// ракета убирает свою строку или столбец, бомба — квадрат 3×3; задетые особые фишки срабатывают по цепочке.
    /// </summary>
    public static class SpecialTiles
    {
        /// <summary>Бомба убирает клетки не дальше BombRadius по каждой оси: 1 — квадрат 3×3.</summary>
        public const int BombRadius = 1;

        public static bool IsRocket(TileSpecial special) =>
            special == TileSpecial.RocketHorizontal || special == TileSpecial.RocketVertical;

        public static bool IsBomb(TileSpecial special) => special == TileSpecial.Bomb;

        /// <summary>Ракета или бомба: срабатывает от хода, двойного касания или чужого взрыва.</summary>
        public static bool IsSpecial(TileSpecial special) => special != TileSpecial.None;

        /// <summary>Какая особая фишка получается из группы; None — группа её не даёт.</summary>
        public static TileSpecial SpecialFor(MatchDirection direction) =>
            direction == MatchDirection.Multiply ? TileSpecial.Bomb : RocketFor(direction);

        /// <summary>Какая ракета получается из группы; None — группа ракету не даёт.</summary>
        public static TileSpecial RocketFor(MatchDirection direction)
        {
            // Ракета летит поперёк собранной линии: так она достаёт фишки, которых линия не касалась
            switch (direction)
            {
                case MatchDirection.LongHorizontal: return TileSpecial.RocketVertical;
                case MatchDirection.LongVertical: return TileSpecial.RocketHorizontal;
                default: return TileSpecial.None;
            }
        }

        /// <param name="groups">Собранные группы шага.</param>
        /// <param name="getSpecial">Особая ли фишка в клетке.</param>
        /// <param name="isClearable">Можно ли убрать клетку взрывом (есть подвижная фишка).</param>
        /// <param name="moved">Клетки хода игрока (особая фишка появится там, куда он поставил фишку); в каскаде пусто.</param>
        /// <param name="forced">Особые фишки, которые сработают и без совпадения (игрок сдвинул ракету или бомбу).</param>
        public static SpecialResolution Resolve(int width, int height, IList<MatchGroup> groups,
            Func<int, int, TileSpecial> getSpecial, Func<int, int, bool> isClearable,
            IList<Vector2Int> moved = null, IList<Vector2Int> forced = null)
        {
            var result = new SpecialResolution();
            var spawnCells = new HashSet<Vector2Int>();

            foreach (var group in groups)
            {
                var special = SpecialFor(group.Direction);
                if (special != TileSpecial.None && TryPickCell(group.Cells, special, getSpecial, moved, out var cell))
                {
                    result.Spawns.Add(new RocketSpawn(cell, special));
                    spawnCells.Add(cell);
                }

                foreach (var c in group.Cells)
                {
                    if (!spawnCells.Contains(c)) result.Remove.Add(c);
                }
            }

            // Цепочка: каждая убираемая особая фишка убирает свою область, задетые особые фишки тоже срабатывают
            var queue = new Queue<Vector2Int>();
            var fired = new HashSet<Vector2Int>();
            foreach (var c in result.Remove)
                if (IsSpecial(getSpecial(c.x, c.y))) queue.Enqueue(c);
            if (forced != null)
            {
                foreach (var c in forced)
                {
                    if (!IsSpecial(getSpecial(c.x, c.y)) || spawnCells.Contains(c)) continue;
                    result.Remove.Add(c);
                    queue.Enqueue(c);
                }
            }

            while (queue.Count > 0)
            {
                var origin = queue.Dequeue();
                if (!fired.Add(origin)) continue;

                var special = getSpecial(origin.x, origin.y);
                var cleared = new List<Vector2Int>();
                foreach (var c in Area(origin, special, width, height))
                {
                    // Только что созданная особая фишка выдерживает взрыв, иначе совпадение рядом с ракетой ничего бы не дало
                    if (c == origin || spawnCells.Contains(c) || !isClearable(c.x, c.y)) continue;
                    if (!result.Remove.Add(c)) continue;
                    cleared.Add(c);
                    if (IsSpecial(getSpecial(c.x, c.y))) queue.Enqueue(c);
                }
                result.Blasts.Add(new RocketBlast(origin, special, cleared));
            }

            return result;
        }

        /// <summary>Клетки, которые убирает особая фишка, от ближних к дальним (без самой фишки).</summary>
        public static IEnumerable<Vector2Int> Area(Vector2Int origin, TileSpecial special, int width, int height) =>
            IsBomb(special) ? Square(origin, BombRadius, width, height) : Line(origin, special, width, height);

        /// <summary>Квадрат вокруг бомбы в пределах поля: сначала соседи по стороне, потом по диагонали.</summary>
        public static IEnumerable<Vector2Int> Square(Vector2Int origin, int radius, int width, int height)
        {
            var cells = new List<Vector2Int>();
            for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                var c = new Vector2Int(origin.x + dx, origin.y + dy);
                if (c.x < 0 || c.y < 0 || c.x >= width || c.y >= height) continue;
                cells.Add(c);
            }
            cells.Sort((a, b) => Distance(a, origin).CompareTo(Distance(b, origin)));
            return cells;
        }

        private static int Distance(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        /// <summary>Клетки строки или столбца ракеты, от ближних к дальним (для волны исчезновения).</summary>
        public static IEnumerable<Vector2Int> Line(Vector2Int origin, TileSpecial special, int width, int height)
        {
            bool horizontal = special == TileSpecial.RocketHorizontal;
            int length = horizontal ? width : height;
            int start = horizontal ? origin.x : origin.y;
            for (int d = 1; d < length; d++)
            {
                int a = start - d, b = start + d;
                if (a < 0 && b >= length) yield break;
                if (a >= 0) yield return horizontal ? new Vector2Int(a, origin.y) : new Vector2Int(origin.x, a);
                if (b < length) yield return horizontal ? new Vector2Int(b, origin.y) : new Vector2Int(origin.x, b);
            }
        }

        private static bool TryPickCell(List<Vector2Int> cells, TileSpecial special,
            Func<int, int, TileSpecial> getSpecial, IList<Vector2Int> moved, out Vector2Int cell) =>
            IsBomb(special) && TryPickCrossing(cells, getSpecial, moved, out cell)
            || TryPickSpawnCell(cells, getSpecial, moved, out cell);

        // Бомба встаёт в клетку пересечения: у неё в группе есть соседи и по строке, и по столбцу.
        // Если пересечений несколько (крест из двух T), берём то, куда игрок поставил фишку.
        // Если пересечение занято особой фишкой, бомба встаёт как ракета — в клетку хода или любую свободную.
        private static bool TryPickCrossing(List<Vector2Int> cells, Func<int, int, TileSpecial> getSpecial,
            IList<Vector2Int> moved, out Vector2Int cell)
        {
            var set = new HashSet<Vector2Int>(cells);
            var crossings = new List<Vector2Int>();
            foreach (var c in cells)
            {
                bool inRow = set.Contains(c + Vector2Int.left) || set.Contains(c + Vector2Int.right);
                bool inColumn = set.Contains(c + Vector2Int.down) || set.Contains(c + Vector2Int.up);
                if (inRow && inColumn && getSpecial(c.x, c.y) == TileSpecial.None) crossings.Add(c);
            }

            if (moved != null)
            {
                foreach (var m in moved)
                {
                    if (!crossings.Contains(m)) continue;
                    cell = m;
                    return true;
                }
            }

            if (crossings.Count > 0)
            {
                cell = crossings[0];
                return true;
            }

            cell = default;
            return false;
        }

        // Ход игрока: ракета там, куда он передвинул фишку. Каскад: вторая клетка линии.
        // Клетки с уже готовой особой фишкой пропускаем — она сработает, а новая появится рядом.
        private static bool TryPickSpawnCell(List<Vector2Int> cells, Func<int, int, TileSpecial> getSpecial,
            IList<Vector2Int> moved, out Vector2Int cell)
        {
            if (moved != null)
            {
                foreach (var m in moved)
                {
                    if (cells.Contains(m) && getSpecial(m.x, m.y) == TileSpecial.None)
                    {
                        cell = m;
                        return true;
                    }
                }
            }

            for (int i = 0; i < cells.Count; i++)
            {
                var c = cells[(i + 1) % cells.Count];
                if (getSpecial(c.x, c.y) != TileSpecial.None) continue;
                cell = c;
                return true;
            }

            cell = default;
            return false;
        }
    }
}
