using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MatchTiles
{
    /// <summary>
    /// Группа совпавших клеток и её тип для начисления очков.
    /// </summary>
    public readonly struct MatchGroup
    {
        public List<Vector2Int> Cells { get; }
        public MatchDirection Direction { get; }

        public MatchGroup(List<Vector2Int> cells, MatchDirection direction)
        {
            Cells = cells;
            Direction = direction;
        }
    }

    /// <summary>
    /// Чистая логика поиска совпадений без зависимостей от сцены.
    /// Линия — непрерывный отрезок из 3+ клеток одного типа.
    /// Пересекающиеся горизонталь и вертикаль одного типа образуют одну группу Multiply.
    /// </summary>
    public static class MatchGrouping
    {
        /// <param name="getKey">Тип клетки; null — пустая или неактивная клетка, в совпадениях не участвует.</param>
        public static List<MatchGroup> FindGroups(int width, int height, Func<int, int, object> getKey)
        {
            var segments = new List<Segment>();
            CollectRuns(width, height, getKey, true, segments);
            CollectRuns(height, width, getKey, false, segments);

            // Объединяем отрезки с общей клеткой (union-find по индексам отрезков)
            var parent = new int[segments.Count];
            for (int i = 0; i < parent.Length; i++) parent[i] = i;

            var owner = new Dictionary<Vector2Int, int>();
            for (int i = 0; i < segments.Count; i++)
            {
                foreach (var cell in segments[i].Cells)
                {
                    if (owner.TryGetValue(cell, out var other))
                        Union(parent, i, other);
                    else
                        owner[cell] = i;
                }
            }

            var clusters = new Dictionary<int, List<int>>();
            for (int i = 0; i < segments.Count; i++)
            {
                int root = Find(parent, i);
                if (!clusters.TryGetValue(root, out var list))
                {
                    list = new List<int>();
                    clusters[root] = list;
                }
                list.Add(i);
            }

            var result = new List<MatchGroup>();
            foreach (var cluster in clusters.Values)
            {
                if (cluster.Count == 1)
                {
                    var s = segments[cluster[0]];
                    result.Add(new MatchGroup(s.Cells, LineDirection(s.Cells.Count, s.IsHorizontal)));
                    continue;
                }

                var cells = new List<Vector2Int>();
                var seen = new HashSet<Vector2Int>();
                foreach (var index in cluster)
                {
                    foreach (var cell in segments[index].Cells)
                    {
                        if (seen.Add(cell)) cells.Add(cell);
                    }
                }
                result.Add(new MatchGroup(cells, MatchDirection.Multiply));
            }

            return result;
        }

        private static MatchDirection LineDirection(int length, bool isHorizontal)
        {
            if (length >= 5) return MatchDirection.FiveInARow;
            if (length == 4) return isHorizontal ? MatchDirection.LongHorizontal : MatchDirection.LongVertical;
            return isHorizontal ? MatchDirection.Horizontal : MatchDirection.Vertical;
        }

        // lineCount линий длиной lineLength: для горизонталей линии — строки, для вертикалей — колонки.
        private static void CollectRuns(int lineLength, int lineCount, Func<int, int, object> getKey, bool isHorizontal, List<Segment> segments)
        {
            for (int line = 0; line < lineCount; line++)
            {
                int start = 0;
                while (start < lineLength)
                {
                    var key = KeyAt(getKey, isHorizontal, start, line);
                    int end = start + 1;
                    if (key != null)
                    {
                        while (end < lineLength && Equals(KeyAt(getKey, isHorizontal, end, line), key)) end++;

                        if (end - start >= 3)
                        {
                            var cells = new List<Vector2Int>(end - start);
                            for (int i = start; i < end; i++)
                                cells.Add(isHorizontal ? new Vector2Int(i, line) : new Vector2Int(line, i));
                            segments.Add(new Segment(cells, isHorizontal));
                        }
                    }
                    start = end;
                }
            }
        }

        private static object KeyAt(Func<int, int, object> getKey, bool isHorizontal, int along, int line) =>
            isHorizontal ? getKey(along, line) : getKey(line, along);

        private static int Find(int[] parent, int i)
        {
            while (parent[i] != i)
            {
                parent[i] = parent[parent[i]];
                i = parent[i];
            }
            return i;
        }

        private static void Union(int[] parent, int a, int b) => parent[Find(parent, a)] = Find(parent, b);

        private readonly struct Segment
        {
            public List<Vector2Int> Cells { get; }
            public bool IsHorizontal { get; }

            public Segment(List<Vector2Int> cells, bool isHorizontal)
            {
                Cells = cells;
                IsHorizontal = isHorizontal;
            }
        }
    }
}
