using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MatchTiles
{
    /// <summary>Особая фишка поверх обычной. Цвет у неё остаётся, она собирается в линии как обычная.</summary>
    public enum TileSpecial
    {
        None,
        // Ракета убирает всю строку
        RocketHorizontal,
        // Ракета убирает весь столбец
        RocketVertical
    }

    /// <summary>Где появится ракета и куда она смотрит.</summary>
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

    /// <summary>Сработавшая ракета: откуда летит и какие клетки убрала (кроме тех, что и так собраны).</summary>
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

    /// <summary>Что убрать за шаг: собранные клетки, клетки от ракет, новые ракеты.</summary>
    public class SpecialResolution
    {
        public HashSet<Vector2Int> Remove { get; } = new HashSet<Vector2Int>();
        public List<RocketSpawn> Spawns { get; } = new List<RocketSpawn>();
        public List<RocketBlast> Blasts { get; } = new List<RocketBlast>();
    }

    /// <summary>
    /// Чистая логика ракет без зависимостей от сцены.
    /// 4 в ряд оставляет на месте одной из фишек ракету того же цвета, поперёк линии.
    /// Ракета срабатывает, когда её фишку убирают (в совпадении, другой ракетой или ходом ракетой),
    /// и убирает всю свою строку или столбец; задетые ракеты срабатывают по цепочке.
    /// </summary>
    public static class SpecialTiles
    {
        public static bool IsRocket(TileSpecial special) =>
            special == TileSpecial.RocketHorizontal || special == TileSpecial.RocketVertical;

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
        /// <param name="isClearable">Можно ли убрать клетку ракетой (есть подвижная фишка).</param>
        /// <param name="moved">Клетки хода игрока (ракета появится там, куда он поставил фишку); в каскаде пусто.</param>
        /// <param name="forced">Ракеты, которые сработают и без совпадения (игрок сдвинул ракету).</param>
        public static SpecialResolution Resolve(int width, int height, IList<MatchGroup> groups,
            Func<int, int, TileSpecial> getSpecial, Func<int, int, bool> isClearable,
            IList<Vector2Int> moved = null, IList<Vector2Int> forced = null)
        {
            var result = new SpecialResolution();
            var spawnCells = new HashSet<Vector2Int>();

            foreach (var group in groups)
            {
                var rocket = RocketFor(group.Direction);
                if (rocket != TileSpecial.None && TryPickSpawnCell(group.Cells, getSpecial, moved, out var cell))
                {
                    result.Spawns.Add(new RocketSpawn(cell, rocket));
                    spawnCells.Add(cell);
                }

                foreach (var c in group.Cells)
                {
                    if (!spawnCells.Contains(c)) result.Remove.Add(c);
                }
            }

            // Цепочка: каждая убираемая ракета убирает свою линию, задетые ракеты тоже срабатывают
            var queue = new Queue<Vector2Int>();
            var fired = new HashSet<Vector2Int>();
            foreach (var c in result.Remove)
                if (IsRocket(getSpecial(c.x, c.y))) queue.Enqueue(c);
            if (forced != null)
            {
                foreach (var c in forced)
                {
                    if (!IsRocket(getSpecial(c.x, c.y)) || spawnCells.Contains(c)) continue;
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
                foreach (var c in Line(origin, special, width, height))
                {
                    // Только что созданная ракета выдерживает взрыв, иначе 4 в ряд рядом с ракетой ничего бы не дал
                    if (c == origin || spawnCells.Contains(c) || !isClearable(c.x, c.y)) continue;
                    if (!result.Remove.Add(c)) continue;
                    cleared.Add(c);
                    if (IsRocket(getSpecial(c.x, c.y))) queue.Enqueue(c);
                }
                result.Blasts.Add(new RocketBlast(origin, special, cleared));
            }

            return result;
        }

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

        // Ход игрока: ракета там, куда он передвинул фишку. Каскад: вторая клетка линии.
        // Клетки с уже готовой ракетой пропускаем — она сработает, а новая появится рядом.
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
