using System.Collections.Generic;
using System.Linq;
using Data;
using Game.MatchTiles;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Editor
{
    public class MatchGroupingTests
    {
        // Строки задаются сверху вниз; '.' — пустая клетка; буква — тип фишки.
        private static List<MatchGroup> Find(params string[] rows)
        {
            int height = rows.Length;
            int width = rows[0].Length;
            return MatchGrouping.FindGroups(width, height, (x, y) =>
            {
                char c = rows[height - 1 - y][x];
                return c == '.' ? null : (object)c;
            });
        }

        private static MatchDirection[] Directions(List<MatchGroup> groups) =>
            groups.Select(g => g.Direction).OrderBy(d => d).ToArray();

        [Test]
        public void NoMatches_ReturnsEmpty()
        {
            Assert.IsEmpty(Find("abab", "baba"));
        }

        [Test]
        public void TwoDifferentTriplesInRow_AreTwoSeparateMatches()
        {
            // Случай из отчёта: G B B B O O O -> 20 + 20, а не 150
            var groups = Find("gbbbooo");
            CollectionAssert.AreEqual(
                new[] { MatchDirection.Horizontal, MatchDirection.Horizontal },
                Directions(groups));
            Assert.That(groups.All(g => g.Cells.Count == 3));
        }

        [Test]
        public void TwoDifferentTriplesInColumn_AreTwoSeparateMatches()
        {
            var groups = Find("a", "a", "a", "b", "b", "b");
            CollectionAssert.AreEqual(
                new[] { MatchDirection.Vertical, MatchDirection.Vertical },
                Directions(groups));
        }

        [Test]
        public void SameTypeTriplesWithGap_AreTwoSeparateMatches()
        {
            var groups = Find("aaabaaa");
            CollectionAssert.AreEqual(
                new[] { MatchDirection.Horizontal, MatchDirection.Horizontal },
                Directions(groups));
        }

        [Test]
        public void TriplesSeparatedByEmptyCell_AreNotJoined()
        {
            var groups = Find("aa.aaa");
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(3, groups[0].Cells.Count);
        }

        [Test]
        public void FourInRow_IsLongHorizontal()
        {
            var groups = Find("baaaab");
            CollectionAssert.AreEqual(new[] { MatchDirection.LongHorizontal }, Directions(groups));
        }

        [Test]
        public void FourInColumn_IsLongVertical()
        {
            var groups = Find("a", "a", "a", "a", "b");
            CollectionAssert.AreEqual(new[] { MatchDirection.LongVertical }, Directions(groups));
        }

        [Test]
        public void FiveInRow_IsFiveInARow()
        {
            var groups = Find("aaaaab");
            CollectionAssert.AreEqual(new[] { MatchDirection.FiveInARow }, Directions(groups));
            Assert.AreEqual(5, groups[0].Cells.Count);
        }

        [Test]
        public void CrossingLines_AreOneMultiplyGroupWithoutDuplicateCells()
        {
            var groups = Find(
                "bab",
                "aaa",
                "bab");
            CollectionAssert.AreEqual(new[] { MatchDirection.Multiply }, Directions(groups));
            Assert.AreEqual(5, groups[0].Cells.Count);
            Assert.AreEqual(5, groups[0].Cells.Distinct().Count());
        }

        [Test]
        public void UnrelatedLineElsewhere_IsNotMergedIntoMultiply()
        {
            var groups = Find(
                "abcccd",
                "aaabdb",
                "abcdbc");
            CollectionAssert.AreEqual(
                new[] { MatchDirection.Horizontal, MatchDirection.Multiply },
                Directions(groups));
        }

        [Test]
        public void SameTypeLinesWithoutSharedCell_AreSeparate()
        {
            var groups = Find(
                "aaab",
                "bbba",
                "aaab");
            Assert.AreEqual(3, groups.Count);
            Assert.That(groups.All(g => g.Direction == MatchDirection.Horizontal));
        }
    }

    public class GameDataProgressTests
    {
        private bool _hadKey;
        private int _savedValue;

        [SetUp]
        public void SetUp()
        {
            _hadKey = PlayerPrefs.HasKey(GameData.UnlockedLevelKey);
            _savedValue = PlayerPrefs.GetInt(GameData.UnlockedLevelKey);
            PlayerPrefs.DeleteKey(GameData.UnlockedLevelKey);
        }

        [TearDown]
        public void TearDown()
        {
            if (_hadKey) PlayerPrefs.SetInt(GameData.UnlockedLevelKey, _savedValue);
            else PlayerPrefs.DeleteKey(GameData.UnlockedLevelKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void NewPlayer_StartsAtLevelOne()
        {
            Assert.AreEqual(1, new GameData().CurrentLevellIndex);
        }

        [Test]
        public void OpenedLevel_IsRestoredByNewInstance()
        {
            var first = new GameData();
            first.OpenNextLevel();
            first.OpenNextLevel();

            Assert.AreEqual(3, new GameData().CurrentLevellIndex);
        }

        [Test]
        public void InvalidStoredValue_FallsBackToLevelOne()
        {
            PlayerPrefs.SetInt(GameData.UnlockedLevelKey, 0);
            Assert.AreEqual(1, new GameData().CurrentLevellIndex);
        }
    }
}
