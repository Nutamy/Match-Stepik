using System.Linq;
using Game.MatchTiles;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Editor
{
    public class MoveFinderTests
    {
        // Строки сверху вниз; '.' — пустая клетка; буква — тип фишки.
        private static System.Func<int, int, object> Keys(string[] rows) => (x, y) =>
        {
            char c = rows[rows.Length - 1 - y][x];
            return c == '.' ? null : (object)c;
        };

        private static bool HasMove(params string[] rows) =>
            MoveFinder.HasPossibleMove(rows[0].Length, rows.Length, Keys(rows));

        // Шахматная раскраска из трёх цветов без единого хода
        private static readonly string[] Deadlock =
        {
            "abcabc",
            "bcabca",
            "cabcab",
            "abcabc",
        };

        [Test]
        public void HorizontalSwapCompletingRow_IsFound()
        {
            Assert.IsTrue(HasMove("aaba", "cdcd"));
        }

        [Test]
        public void VerticalSwapIntoColumn_IsFound()
        {
            Assert.IsTrue(HasMove(
                "ab",
                "ac",
                "ca"));
        }

        [Test]
        public void DeadlockBoard_HasNoMove()
        {
            Assert.IsFalse(HasMove(Deadlock));
        }

        [Test]
        public void EmptyCellBreaksPotentialLine()
        {
            // Обмен b и a дал бы "aa.a" — пустая клетка не соединяет линию
            Assert.IsFalse(HasMove("aa.ba", "cdecd"));
        }

        [Test]
        public void EmptyCellIsNeverSwapped()
        {
            Assert.IsFalse(HasMove("a.a", "bab"));
        }

        [Test]
        public void TryFindMove_ReturnsAdjacentCellsThatMakeLine()
        {
            var rows = new[] { "aaba", "cdcd" };
            Assert.IsTrue(MoveFinder.TryFindMove(4, 2, Keys(rows), out var from, out var to));
            Assert.AreEqual(1, Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y));
        }

        [Test]
        public void Shuffle_OfDeadlock_GivesPlayableBoardWithSameTiles()
        {
            int width = Deadlock[0].Length, height = Deadlock.Length;
            var original = Keys(Deadlock);
            var map = MoveFinder.TryShuffle(width, height, original, new System.Random(1));
            Assert.IsNotNull(map);

            System.Func<int, int, object> shuffled = (x, y) =>
            {
                var cell = new Vector2Int(x, y);
                return map.TryGetValue(cell, out var source) ? original(source.x, source.y) : null;
            };

            Assert.IsEmpty(MatchGrouping.FindGroups(width, height, shuffled));
            Assert.IsTrue(MoveFinder.HasPossibleMove(width, height, shuffled));
            CollectionAssert.AreEquivalent(map.Keys, map.Values);
            Assert.AreEqual(width * height, map.Count);
        }

        [Test]
        public void Shuffle_KeepsEmptyCellsInPlace()
        {
            var rows = new[] { ".abcab", "bcabc.", "cabcab", ".bcab." };
            var map = MoveFinder.TryShuffle(6, 4, Keys(rows), new System.Random(7));
            Assert.IsNotNull(map);
            Assert.IsFalse(map.ContainsKey(new Vector2Int(0, 3)));
            Assert.IsFalse(map.Values.Contains(new Vector2Int(5, 2)));
        }

        [Test]
        public void Shuffle_LargeBoardWithHole_AlwaysPlayableWithoutMatches()
        {
            // 9x9, 4 типа, пустой квадрат 3x3 в центре — как уровень со скриншота.
            // Случайная перестановка почти никогда не обходится без троек, поэтому раскладка должна быть построена.
            const int size = 9;
            for (int seed = 0; seed < 50; seed++)
            {
                var rng = new System.Random(seed);
                var colors = new object[size, size];
                for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    bool hole = x >= 3 && x <= 5 && y >= 3 && y <= 5;
                    colors[x, y] = hole ? null : (object)"abcd"[rng.Next(4)];
                }
                System.Func<int, int, object> original = (x, y) => colors[x, y];

                var map = MoveFinder.TryShuffle(size, size, original, rng);
                Assert.IsNotNull(map, $"seed {seed}");

                System.Func<int, int, object> shuffled = (x, y) =>
                    map.TryGetValue(new Vector2Int(x, y), out var source) ? original(source.x, source.y) : null;
                Assert.IsEmpty(MatchGrouping.FindGroups(size, size, shuffled), $"seed {seed}");
                Assert.IsTrue(MoveFinder.HasPossibleMove(size, size, shuffled), $"seed {seed}");
            }
        }

        [Test]
        public void CreatesLineWithPlaced_DetectsLeftAndBelowPairs()
        {
            var rows = new[] { "a..", "a..", "bb." };
            var keys = Keys(rows);
            Assert.IsTrue(MoveFinder.CreatesLineWithPlaced(keys, 3, 3, 2, 0, 'b'));
            Assert.IsFalse(MoveFinder.CreatesLineWithPlaced(keys, 3, 3, 2, 0, 'a'));
            Assert.IsFalse(MoveFinder.CreatesLineWithPlaced(keys, 3, 3, 0, 0, 'a'));
            Assert.IsTrue(MoveFinder.CreatesLineWithPlaced((x, y) => x == 0 && y < 2 ? (object)'a' : null, 3, 3, 0, 2, 'a'));
        }

        [Test]
        public void Shuffle_ImpossibleBoard_ReturnsNull()
        {
            // Два цвета по одной фишке: ни одна перестановка не даёт хода
            Assert.IsNull(MoveFinder.TryShuffle(2, 1, Keys(new[] { "ab" }), new System.Random(1), 10));
        }
    }
}
