using System.Collections.Generic;
using Game.Tiles;
using Grid = Game.GridSystem.Grid;

namespace Game.MatchTiles
{
    public class MatchFinder
    {
        public List<Tile> TilesToRemove { get; } = new List<Tile>();
        // Все совпадения за один ход: каждая группа начисляет очки отдельно
        public List<MatchResult> AllMatchResults { get; private set; } = new List<MatchResult>();

        public bool CheckBoardForMatches(Grid grid)
        {
            ClearTilesToRemove();

            var groups = MatchGrouping.FindGroups(grid.Width, grid.Height, (x, y) => GetMatchKey(grid.GetValue(x, y)));
            if (groups.Count == 0) return false;

            foreach (var group in groups)
            {
                var tiles = new List<Tile>(group.Cells.Count);
                foreach (var cell in group.Cells)
                {
                    tiles.Add(grid.GetValue(cell.x, cell.y));
                }

                AllMatchResults.Add(new MatchResult(tiles, group.Direction));
                TilesToRemove.AddRange(tiles);
            }

            // Помечаем плитки для анимации удаления
            foreach (var t in TilesToRemove)
            {
                t.SetMatched(true);
            }

            return true;
        }

        public static object GetMatchKey(Tile tile)
        {
            if (tile == null || !tile.IsInteractable) return null;
            return tile.TileConfig;
        }

        public void ClearTilesToRemove()
        {
            foreach (var tile in TilesToRemove) tile.SetMatched(false);
            TilesToRemove.Clear();
            AllMatchResults.Clear();
        }
    }
}
