using System.Collections.Generic;
using System.Linq;
using Game.Tiles;
using UnityEngine;
using Grid = Game.GridSystem.Grid;

namespace Game.MatchTiles
{
    public class MatchFinder
    {
        public List<Tile> TilesToRemove { get; } = new List<Tile>();
        // Теперь здесь хранятся ВСЕ совпадения за один ход
        public List<MatchResult> AllMatchResults { get; private set; } = new List<MatchResult>();

        public bool CheckBoardForMatches(Grid grid)
        {
            var hasMatched = false;
            ClearTilesToRemove();
            AllMatchResults.Clear();

            HashSet<Tile> horizontalMatches = new HashSet<Tile>();
            HashSet<Tile> verticalMatches = new HashSet<Tile>();

            // 1. Поиск горизонталей (сканируем всё поле)
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width - 2; x++)
                {
                    var t1 = grid.GetValue(x, y);
                    var t2 = grid.GetValue(x + 1, y);
                    var t3 = grid.GetValue(x + 2, y);

                    if (IsValidMatch(t1, t2, t3))
                    {
                        horizontalMatches.Add(t1);
                        horizontalMatches.Add(t2);
                        horizontalMatches.Add(t3);
                        hasMatched = true;
                    }
                }
            }

            // 2. Поиск вертикалей (сканируем всё поле)
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height - 2; y++)
                {
                    var t1 = grid.GetValue(x, y);
                    var t2 = grid.GetValue(x, y + 1);
                    var t3 = grid.GetValue(x, y + 2);

                    if (IsValidMatch(t1, t2, t3))
                    {
                        verticalMatches.Add(t1);
                        verticalMatches.Add(t2);
                        verticalMatches.Add(t3);
                        hasMatched = true;
                    }
                }
            }

            if (!hasMatched) return false;

            // 3. Группируем найденные плитки в AllMatchResults
            ProcessAllFoundMatches(horizontalMatches, verticalMatches, grid);

            // Помечаем плитки для анимации удаления
            foreach (var t in TilesToRemove)
            {
                t.SetMatched(true);
            }

            return true;
        }

        private void ProcessAllFoundMatches(HashSet<Tile> hMatches, HashSet<Tile> vMatches, Grid grid)
        {
            var intersection = hMatches.Intersect(vMatches).ToList();

            // Если есть пересечение (Multiply), обрабатываем его как одну группу
            if (intersection.Count > 0)
            {
                var combined = hMatches.Union(vMatches).ToList();
                AllMatchResults.Add(new MatchResult(combined, MatchDirection.Multiply));
                TilesToRemove.AddRange(combined);
            }
            else
            {
                // Иначе разбиваем на отдельные линии (чтобы засчитать 3+4 одновременно)
                AddLineResults(hMatches, true, grid);
                AddLineResults(vMatches, false, grid);
            }
        }

        private void AddLineResults(HashSet<Tile> matches, bool isHorizontal, Grid grid)
        {
            if (matches.Count < 3) return;

            // Группируем плитки, чтобы отличить две разные линии в разных частях поля
            var groups = matches.GroupBy(t => isHorizontal ?
                grid.WorldToGrid(t.transform.position).y :
                grid.WorldToGrid(t.transform.position).x);

            foreach (var group in groups)
            {
                var tiles = group.ToList();
                MatchDirection dir;

                if (tiles.Count >= 5) dir = MatchDirection.FiveInARow;
                else if (tiles.Count == 4) dir = isHorizontal ? MatchDirection.LongHorizontal : MatchDirection.LongVertical;
                else dir = isHorizontal ? MatchDirection.Horizontal : MatchDirection.Vertical;

                AllMatchResults.Add(new MatchResult(tiles, dir));
                TilesToRemove.AddRange(tiles);
            }
        }

        private bool IsValidMatch(Tile t1, Tile t2, Tile t3)
        {
            if (t1 == null || t2 == null || t3 == null) return false;
            if (!t1.IsInteractable || !t2.IsInteractable || !t3.IsInteractable) return false;
            return t1.TileConfig == t2.TileConfig && t2.TileConfig == t3.TileConfig;
        }

        public void ClearTilesToRemove()
        {
            foreach (var tile in TilesToRemove) tile.SetMatched(false);
            TilesToRemove.Clear();
            AllMatchResults.Clear();
        }
    }
}