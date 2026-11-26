using System.Collections.Generic;
using Game.MatchTiles;
using Game.Score;
using Game.Tiles;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Grid = Game.GridSystem.Grid;

namespace Tests.Editor
{
    // Связка MatchFinder + ScoreCalculator на настоящих Tile и Grid
    public class MatchFinderScoreTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly Dictionary<char, TileConfig> _configs = new Dictionary<char, TileConfig>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _created) Object.DestroyImmediate(o);
            _created.Clear();
            _configs.Clear();
        }

        private TileConfig Config(char kind)
        {
            if (_configs.TryGetValue(kind, out var existing)) return existing;
            var config = ScriptableObject.CreateInstance<TileConfig>();
            var so = new SerializedObject(config);
            so.FindProperty("_isInteractable").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(config);
            _configs[kind] = config;
            return config;
        }

        // Строки сверху вниз, как на экране
        private Grid BuildGrid(params string[] rows)
        {
            var grid = new Grid();
            int height = rows.Length, width = rows[0].Length;
            grid.SetupGrid(width, height);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var go = new GameObject($"Tile {x},{y}", typeof(SpriteRenderer), typeof(Tile));
                go.transform.position = grid.GridToWorld(x, y);
                _created.Add(go);
                var tile = go.GetComponent<Tile>();
                tile.SetTileConfig(Config(rows[height - 1 - y][x]));
                grid.SetValue(x, y, tile);
            }
            return grid;
        }

        private static int ScoreFor(Grid grid, MatchFinder finder)
        {
            var progress = new GameProgress();
            var calculator = new ScoreCalculator(progress);
            Assert.IsTrue(finder.CheckBoardForMatches(grid));
            foreach (var result in finder.AllMatchResults) calculator.CalculateScoreToAdd(result.Direction);
            return progress.Score;
        }

        [Test]
        public void ReportCase_TwoDifferentTriplesInRow_Gives40Points()
        {
            // Строка из отчёта после обмена: зелёная, 3 коричневые, 3 оранжевые
            var grid = BuildGrid("gbbbooo");
            var finder = new MatchFinder();

            Assert.AreEqual(40, ScoreFor(grid, finder));
            Assert.AreEqual(6, finder.TilesToRemove.Count);
            Assert.That(finder.TilesToRemove, Has.No.Member(grid.GetValue(0, 0)));
        }

        [Test]
        public void RealFiveInRow_Gives150Points()
        {
            Assert.AreEqual(150, ScoreFor(BuildGrid("gbbbbbo"), new MatchFinder()));
        }

        [Test]
        public void CrossAndSeparateLine_Gives100Plus20()
        {
            var grid = BuildGrid(
                "abcccd",
                "aaabdb",
                "abcdbc");
            Assert.AreEqual(120, ScoreFor(grid, new MatchFinder()));
        }
    }
}
