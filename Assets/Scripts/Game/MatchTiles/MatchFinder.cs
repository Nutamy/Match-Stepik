using System.Collections.Generic;
using Game.Tiles;
using UnityEngine;
using Grid = Game.GridSystem.Grid;

namespace Game.MatchTiles
{
    public class MatchFinder
    {
        public List<Tile> TilesToRemove { get; } = new List<Tile>();
        // Все совпадения за один ход: каждая группа начисляет очки отдельно
        public List<MatchResult> AllMatchResults { get; private set; } = new List<MatchResult>();
        // Фишки, которые остаются на поле и становятся ракетами (4 в ряд), бомбами (пересечение) или цветными бомбами (5 в ряд)
        public List<(Tile Tile, TileSpecial Special)> RocketsToCreate { get; } = new List<(Tile, TileSpecial)>();
        // Комбо цветной бомбы: обычные фишки, которые на время становятся ракетами или бомбами и срабатывают
        public List<(Tile Tile, TileSpecial Special)> Converted { get; } = new List<(Tile, TileSpecial)>();
        public Vector2Int? ComboPartner { get; private set; }
        // Сработавшие ракеты и бомбы: откуда и какие фишки убрали сверх совпадений
        public List<(Vector2Int Origin, TileSpecial Special, List<Tile> Cleared)> RocketBlasts { get; } =
            new List<(Vector2Int, TileSpecial, List<Tile>)>();

        /// <summary>Есть ли что убирать: совпадения на поле или сработавшая ракета.</summary>
        /// <param name="moved">Клетки только что сделанного хода: туда встанет новая ракета,
        /// а ракета в этих клетках срабатывает даже без совпадения.</param>
        public bool CheckBoardForMatches(Grid grid, IList<Vector2Int> moved = null)
        {
            ClearTilesToRemove();

            var groups = MatchGrouping.FindGroups(grid.Width, grid.Height, (x, y) => GetMatchKey(grid.GetValue(x, y)));
            var resolution = SpecialTiles.Resolve(grid.Width, grid.Height, groups,
                (x, y) => SpecialAt(grid, x, y),
                (x, y) => GetMatchKey(grid.GetValue(x, y)) != null,
                moved, moved,
                (x, y) => ColorAt(grid, x, y));
            if (resolution.Remove.Count == 0) return false;

            foreach (var group in groups)
            {
                var tiles = new List<Tile>(group.Cells.Count);
                foreach (var cell in group.Cells)
                {
                    tiles.Add(grid.GetValue(cell.x, cell.y));
                }

                AllMatchResults.Add(new MatchResult(tiles, group.Direction));
            }

            foreach (var cell in resolution.Remove)
            {
                TilesToRemove.Add(grid.GetValue(cell.x, cell.y));
            }

            foreach (var spawn in resolution.Spawns)
            {
                RocketsToCreate.Add((grid.GetValue(spawn.Cell.x, spawn.Cell.y), spawn.Special));
            }

            ComboPartner = resolution.ComboPartner;
            foreach (var convert in resolution.Converted)
            {
                Converted.Add((grid.GetValue(convert.Cell.x, convert.Cell.y), convert.Special));
            }

            foreach (var blast in resolution.Blasts)
            {
                var cleared = new List<Tile>(blast.Cleared.Count);
                foreach (var cell in blast.Cleared) cleared.Add(grid.GetValue(cell.x, cell.y));
                RocketBlasts.Add((blast.Origin, blast.Special, cleared));
            }

            // Помечаем плитки для анимации удаления
            foreach (var t in TilesToRemove)
            {
                t.SetMatched(true);
            }

            return true;
        }

        private static TileSpecial SpecialAt(Grid grid, int x, int y)
        {
            var tile = grid.GetValue(x, y);
            return tile != null ? tile.Special : TileSpecial.None;
        }

        // Цвет обычной фишки для цветной бомбы; у особых и неподвижных фишек цвета нет
        private static object ColorAt(Grid grid, int x, int y)
        {
            var tile = grid.GetValue(x, y);
            if (tile == null || !tile.IsInteractable || tile.Special != TileSpecial.None) return null;
            return tile.TileConfig;
        }

        public static object GetMatchKey(Tile tile)
        {
            if (tile == null || !tile.IsInteractable) return null;
            // У ракеты и бомбы нет цвета: они ни с чем не собираются в линию, но их можно двигать и убирать
            if (tile.Special != TileSpecial.None) return tile;
            return tile.TileConfig;
        }

        public void ClearTilesToRemove()
        {
            foreach (var tile in TilesToRemove) tile.SetMatched(false);
            TilesToRemove.Clear();
            AllMatchResults.Clear();
            RocketsToCreate.Clear();
            RocketBlasts.Clear();
            Converted.Clear();
            ComboPartner = null;
        }
    }
}
