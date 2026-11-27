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

            var pool = new List<Vector2Int>(cells.Count);
            var keys = new object[width, height];
            var map = new Dictionary<Vector2Int, Vector2Int>(cells.Count);

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                // Случайный порядок фишек, затем раскладка по клеткам без образования троек
                pool.Clear();
                pool.AddRange(cells);
                for (int i = pool.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    (pool[i], pool[j]) = (pool[j], pool[i]);
                }

                Array.Clear(keys, 0, keys.Length);
                map.Clear();
                bool placedAll = true;
                foreach (var cell in cells)
                {
                    int pick = -1;
                    for (int i = 0; i < pool.Count; i++)
                    {
                        var key = original[pool[i].x, pool[i].y];
                        if (!CreatesLineWithPlaced((x, y) => keys[x, y], width, height, cell.x, cell.y, key))
                        {
                            pick = i;
                            break;
                        }
                    }

                    if (pick < 0)
                    {
                        placedAll = false;
                        break;
                    }

                    var source = pool[pick];
                    pool.RemoveAt(pick);
                    keys[cell.x, cell.y] = original[source.x, source.y];
                    map[cell] = source;
                }

                if (!placedAll || !TryFindMove(keys, out _, out _)) continue;
                return new Dictionary<Vector2Int, Vector2Int>(map);
            }

            return null;
        }

        /// <summary>
        /// Даст ли фишка key тройку с двумя уже разложенными соседями слева или снизу.
        /// Раскладка идёт по столбцам слева направо и снизу вверх, поэтому этих соседей достаточно.
        /// </summary>
        public static bool CreatesLineWithPlaced(Func<int, int, object> getKey, int width, int height, int x, int y, object key)
        {
            if (key == null) return false;
            if (x >= 2 && Equals(getKey(x - 1, y), key) && Equals(getKey(x - 2, y), key)) return true;
            if (y >= 2 && Equals(getKey(x, y - 1), key) && Equals(getKey(x, y - 2), key)) return true;
            return false;
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
