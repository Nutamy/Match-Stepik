using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MatchTiles
{
    /// <summary>
    /// Поиск возможного хода и перемешивание поля без зависимостей от сцены.
    /// Ключ клетки null — пустая или неподвижная клетка: её нельзя двигать, она прерывает линии.
    /// </summary>
    public static class MoveFinder
    {
        public static bool HasPossibleMove(int width, int height, Func<int, int, object> getKey) =>
            TryFindMove(width, height, getKey, out _, out _);

        /// <summary>Первый найденный обмен соседних клеток, который даёт линию из 3+.</summary>
        public static bool TryFindMove(int width, int height, Func<int, int, object> getKey, out Vector2Int from, out Vector2Int to)
        {
            var keys = Snapshot(width, height, getKey);
            return TryFindMove(keys, out from, out to);
        }

        /// <summary>
        /// Ищет перестановку подвижных клеток, после которой на поле нет готовых совпадений и есть хотя бы один ход.
        /// Возвращает соответствие «новая клетка → клетка, откуда берётся фишка» или null, если за attempts попыток не вышло.
        /// </summary>
        public static Dictionary<Vector2Int, Vector2Int> TryShuffle(int width, int height, Func<int, int, object> getKey, System.Random random, int attempts = 100)
        {
            var original = Snapshot(width, height, getKey);
            var cells = new List<Vector2Int>();
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (original[x, y] != null) cells.Add(new Vector2Int(x, y));

            var sources = new List<Vector2Int>(cells);
            var keys = new object[width, height];

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                for (int i = sources.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    (sources[i], sources[j]) = (sources[j], sources[i]);
                }

                Array.Clear(keys, 0, keys.Length);
                for (int i = 0; i < cells.Count; i++)
                    keys[cells[i].x, cells[i].y] = original[sources[i].x, sources[i].y];

                if (MatchGrouping.FindGroups(width, height, (x, y) => keys[x, y]).Count > 0) continue;
                if (!TryFindMove(keys, out _, out _)) continue;

                var map = new Dictionary<Vector2Int, Vector2Int>(cells.Count);
                for (int i = 0; i < cells.Count; i++) map[cells[i]] = sources[i];
                return map;
            }

            return null;
        }

        private static object[,] Snapshot(int width, int height, Func<int, int, object> getKey)
        {
            var keys = new object[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                keys[x, y] = getKey(x, y);
            return keys;
        }

        private static bool TryFindMove(object[,] keys, out Vector2Int from, out Vector2Int to)
        {
            int width = keys.GetLength(0), height = keys.GetLength(1);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (keys[x, y] == null) continue;
                if (TrySwap(keys, x, y, x + 1, y) || TrySwap(keys, x, y, x, y + 1))
                {
                    from = new Vector2Int(x, y);
                    to = TrySwap(keys, x, y, x + 1, y) ? new Vector2Int(x + 1, y) : new Vector2Int(x, y + 1);
                    return true;
                }
            }

            from = to = default;
            return false;
        }

        private static bool TrySwap(object[,] keys, int ax, int ay, int bx, int by)
        {
            if (bx >= keys.GetLength(0) || by >= keys.GetLength(1)) return false;
            var a = keys[ax, ay];
            var b = keys[bx, by];
            if (a == null || b == null || Equals(a, b)) return false;

            keys[ax, ay] = b;
            keys[bx, by] = a;
            bool result = MakesLine(keys, ax, ay) || MakesLine(keys, bx, by);
            keys[ax, ay] = a;
            keys[bx, by] = b;
            return result;
        }

        private static bool MakesLine(object[,] keys, int x, int y)
        {
            var key = keys[x, y];
            return RunLength(keys, x, y, 1, 0, key) + RunLength(keys, x, y, -1, 0, key) + 1 >= 3
                || RunLength(keys, x, y, 0, 1, key) + RunLength(keys, x, y, 0, -1, key) + 1 >= 3;
        }

        private static int RunLength(object[,] keys, int x, int y, int dx, int dy, object key)
        {
            int count = 0;
            int width = keys.GetLength(0), height = keys.GetLength(1);
            for (int cx = x + dx, cy = y + dy; cx >= 0 && cy >= 0 && cx < width && cy < height; cx += dx, cy += dy)
            {
                if (!Equals(keys[cx, cy], key)) break;
                count++;
            }
            return count;
        }
    }
}
