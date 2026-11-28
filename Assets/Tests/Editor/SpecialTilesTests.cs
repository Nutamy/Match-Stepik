using System.Collections.Generic;
using System.Linq;
using Game.MatchTiles;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Editor
{
    public class SpecialTilesTests
    {
        // Строки сверху вниз. '.' — пустая клетка; '#' — неподвижная клетка уровня;
        // строчная буква — обычная фишка; H — горизонтальная ракета (убирает строку),
        // V — вертикальная (убирает столбец), B — бомба (убирает 3×3), C — цветная бомба (убирает все фишки одного цвета). У особых фишек нет цвета: ключ уникален, в линии они не собираются.
        private class Board
        {
            private readonly string[] _rows;
            public int Width => _rows[0].Length;
            public int Height => _rows.Length;

            public Board(params string[] rows) => _rows = rows;

            private char At(int x, int y) => _rows[Height - 1 - y][x];

            public object Key(int x, int y)
            {
                char c = At(x, y);
                if (c == '.' || c == '#') return null;
                if (c == 'H' || c == 'V' || c == 'B' || c == 'C') return new Vector2Int(x, y);
                return c;
            }

            public TileSpecial Special(int x, int y)
            {
                switch (At(x, y))
                {
                    case 'H': return TileSpecial.RocketHorizontal;
                    case 'V': return TileSpecial.RocketVertical;
                    case 'B': return TileSpecial.Bomb;
                    case 'C': return TileSpecial.ColorBomb;
                    default: return TileSpecial.None;
                }
            }

            public bool Clearable(int x, int y) => Key(x, y) != null;

            // Цвет — только у обычных фишек (строчные буквы)
            public object Color(int x, int y) => char.IsLower(At(x, y)) ? (object)At(x, y) : null;

            public SpecialResolution Resolve(IList<Vector2Int> moved = null, IList<Vector2Int> forced = null)
            {
                var groups = MatchGrouping.FindGroups(Width, Height, Key);
                return SpecialTiles.Resolve(Width, Height, groups, Special, Clearable, moved, forced, Color);
            }
        }

        private static Vector2Int C(int x, int y) => new Vector2Int(x, y);

        [Test]
        public void FourInRow_HorizontalLine_GivesVerticalRocket()
        {
            Assert.AreEqual(TileSpecial.RocketVertical, SpecialTiles.RocketFor(MatchDirection.LongHorizontal));
            Assert.AreEqual(TileSpecial.RocketHorizontal, SpecialTiles.RocketFor(MatchDirection.LongVertical));
            Assert.AreEqual(TileSpecial.None, SpecialTiles.RocketFor(MatchDirection.Horizontal));
            Assert.AreEqual(TileSpecial.None, SpecialTiles.RocketFor(MatchDirection.FiveInARow));
            Assert.AreEqual(TileSpecial.None, SpecialTiles.RocketFor(MatchDirection.Multiply));
        }

        [Test]
        public void FourInRow_LeavesOneTileAsRocket_AndRemovesThree()
        {
            var r = new Board("baaaab").Resolve();

            Assert.AreEqual(1, r.Spawns.Count);
            Assert.AreEqual(TileSpecial.RocketVertical, r.Spawns[0].Special);
            Assert.AreEqual(3, r.Remove.Count);
            Assert.That(r.Remove, Has.No.Member(r.Spawns[0].Cell));
            Assert.IsEmpty(r.Blasts);
        }

        [Test]
        public void PlayerMove_RocketAppearsWhereTileWasMoved()
        {
            var r = new Board("baaaab").Resolve(moved: new[] { C(3, 0), C(3, 1) });
            Assert.AreEqual(C(3, 0), r.Spawns[0].Cell);
        }

        [Test]
        public void ThreeInRow_GivesNoRocket()
        {
            var r = new Board("baaab").Resolve();
            Assert.IsEmpty(r.Spawns);
            Assert.AreEqual(3, r.Remove.Count);
        }

        [Test]
        public void Rocket_HasNoColor_AndDoesNotMatch()
        {
            var r = new Board("aHa").Resolve();
            Assert.IsEmpty(r.Remove);
        }

        [Test]
        public void HorizontalRocket_ClearsWholeRow()
        {
            var r = new Board(
                "bcdbc",
                "aHacd",
                "cdbcb").Resolve(forced: new[] { C(1, 1) });

            CollectionAssert.AreEquivalent(new[] { C(0, 1), C(1, 1), C(2, 1), C(3, 1), C(4, 1) }, r.Remove);
            Assert.AreEqual(1, r.Blasts.Count);
            Assert.AreEqual(C(1, 1), r.Blasts[0].Origin);
            Assert.AreEqual(4, r.Blasts[0].Cleared.Count);
        }

        [Test]
        public void VerticalRocket_ClearsWholeColumn()
        {
            var r = new Board(
                "bcd",
                "dce",
                "aVa",
                "cdb").Resolve(forced: new[] { C(1, 1) });

            CollectionAssert.AreEquivalent(new[] { C(1, 0), C(1, 1), C(1, 2), C(1, 3) }, r.Remove);
        }

        [Test]
        public void Rocket_DoesNotClearFixedCells()
        {
            var r = new Board("aHa#bc").Resolve(forced: new[] { C(1, 0) });
            Assert.That(r.Remove, Has.No.Member(C(3, 0)));
            Assert.That(r.Remove, Has.Member(C(4, 0)));
            Assert.That(r.Remove, Has.Member(C(5, 0)));
        }

        [Test]
        public void RocketHitByRocket_FiresToo()
        {
            // Горизонтальная ракета в средней строке задевает вертикальную в столбце 4
            var r = new Board(
                "bcdbc",
                "cHabV",
                "cdbcb").Resolve(forced: new[] { C(1, 1) });

            Assert.AreEqual(2, r.Blasts.Count);
            Assert.That(r.Remove, Has.Member(C(4, 2)));
            Assert.That(r.Remove, Has.Member(C(4, 0)));
            Assert.AreEqual(7, r.Remove.Count);
        }

        [Test]
        public void MovedRocket_FiresWithoutMatch()
        {
            var board = new Board(
                "bcd",
                "cHb",
                "dbc");
            Assert.IsEmpty(board.Resolve().Remove);

            var r = board.Resolve(moved: new[] { C(1, 1), C(1, 0) }, forced: new[] { C(1, 1), C(1, 0) });
            CollectionAssert.AreEquivalent(new[] { C(0, 1), C(1, 1), C(2, 1) }, r.Remove);
            Assert.AreEqual(1, r.Blasts.Count);
        }

        [Test]
        public void NewRocket_SurvivesBlastOfOldRocket()
        {
            // Вертикальная четвёрка 'a' в столбце 0 даёт ракету во второй клетке (0,1);
            // в тот же шаг срабатывает ракета H в строке 1 и не задевает новую ракету
            var r = new Board(
                "ab",
                "ac",
                "aH",
                "ac").Resolve(forced: new[] { C(1, 1) });

            Assert.AreEqual(1, r.Spawns.Count);
            Assert.AreEqual(C(0, 1), r.Spawns[0].Cell);
            Assert.That(r.Remove, Has.No.Member(C(0, 1)));
            Assert.AreEqual(1, r.Blasts.Count);
            Assert.IsEmpty(r.Blasts[0].Cleared);
        }

        [Test]
        public void Line_GoesFromNearToFar()
        {
            var cells = SpecialTiles.Line(C(1, 0), TileSpecial.RocketHorizontal, 5, 1).ToList();
            CollectionAssert.AreEqual(new[] { C(0, 0), C(2, 0), C(3, 0), C(4, 0) }, cells);
        }

        [Test]
        public void Crossing_GivesBomb()
        {
            Assert.AreEqual(TileSpecial.Bomb, SpecialTiles.SpecialFor(MatchDirection.Multiply));
            Assert.AreEqual(TileSpecial.RocketVertical, SpecialTiles.SpecialFor(MatchDirection.LongHorizontal));
            Assert.AreEqual(TileSpecial.None, SpecialTiles.SpecialFor(MatchDirection.Horizontal));
        }

        [Test]
        public void LShape_LeavesBombInCorner()
        {
            var r = new Board(
                "abc",
                "acb",
                "aaa").Resolve();

            Assert.AreEqual(1, r.Spawns.Count);
            Assert.AreEqual(TileSpecial.Bomb, r.Spawns[0].Special);
            Assert.AreEqual(C(0, 0), r.Spawns[0].Cell);
            Assert.AreEqual(4, r.Remove.Count);
            Assert.That(r.Remove, Has.No.Member(C(0, 0)));
        }

        [Test]
        public void TShape_LeavesBombAtJunction()
        {
            var r = new Board(
                "aaa",
                "bac",
                "cab").Resolve();

            Assert.AreEqual(TileSpecial.Bomb, r.Spawns[0].Special);
            Assert.AreEqual(C(1, 2), r.Spawns[0].Cell);
            Assert.AreEqual(4, r.Remove.Count);
        }

        [Test]
        public void Bomb_ClearsSquareAroundItself()
        {
            var r = new Board(
                "bcdbc",
                "cdbcd",
                "dbBdb",
                "bcdbc",
                "cdbcd").Resolve(forced: new[] { C(2, 2) });

            Assert.AreEqual(9, r.Remove.Count);
            for (int x = 1; x <= 3; x++)
            for (int y = 1; y <= 3; y++)
                Assert.That(r.Remove, Has.Member(C(x, y)));
            Assert.AreEqual(1, r.Blasts.Count);
            Assert.AreEqual(8, r.Blasts[0].Cleared.Count);
        }

        [Test]
        public void BombInCorner_ClearsOnlyCellsOnBoard()
        {
            var r = new Board(
                "bcd",
                "cdb",
                "Bbc").Resolve(forced: new[] { C(0, 0) });

            CollectionAssert.AreEquivalent(new[] { C(0, 0), C(1, 0), C(0, 1), C(1, 1) }, r.Remove);
        }

        [Test]
        public void RocketHitsBomb_BombExplodesToo()
        {
            // Ракета во второй строке снизу задевает бомбу, бомба убирает квадрат вокруг себя
            var r = new Board(
                "bcdbc",
                "cdbcd",
                "HcdBc",
                "bcdbd").Resolve(forced: new[] { C(0, 1) });

            Assert.AreEqual(2, r.Blasts.Count);
            Assert.That(r.Remove, Has.Member(C(4, 2)));
            Assert.That(r.Remove, Has.Member(C(2, 0)));
            Assert.That(r.Remove, Has.No.Member(C(1, 2)));
        }

        [Test]
        public void Square_GoesFromSidesToCorners()
        {
            var cells = SpecialTiles.Square(C(1, 1), 1, 3, 3).ToList();
            Assert.AreEqual(8, cells.Count);
            foreach (var c in cells.Take(4))
                Assert.AreEqual(1, Mathf.Abs(c.x - 1) + Mathf.Abs(c.y - 1));
        }
    
        [Test]
        public void FiveInRow_GivesColorBomb()
        {
            Assert.AreEqual(TileSpecial.ColorBomb, SpecialTiles.SpecialFor(MatchDirection.FiveInARow));

            var r = new Board("baaaaab").Resolve(moved: new[] { C(2, 0), C(2, 1) });

            Assert.AreEqual(1, r.Spawns.Count);
            Assert.AreEqual(TileSpecial.ColorBomb, r.Spawns[0].Special);
            Assert.AreEqual(C(2, 0), r.Spawns[0].Cell);
            Assert.AreEqual(4, r.Remove.Count);
        }

        [Test]
        public void TWithFiveLine_GivesColorBombInsteadOfBomb()
        {
            var r = new Board(
                "..a..",
                "..a..",
                "aaaaa").Resolve();

            Assert.AreEqual(1, r.Spawns.Count);
            Assert.AreEqual(TileSpecial.ColorBomb, r.Spawns[0].Special);
        }

        [Test]
        public void ColorBombSwappedWithTile_ClearsThatColor()
        {
            var r = new Board(
                "abcab",
                "bCbcb",
                "cbaba").Resolve(moved: new[] { C(1, 1), C(2, 1) }, forced: new[] { C(1, 1), C(2, 1) });

            var expected = new List<Vector2Int> { C(1, 1) };
            var board = new Board("abcab", "bCbcb", "cbaba");
            for (int y = 0; y < board.Height; y++)
            for (int x = 0; x < board.Width; x++)
                if (Equals(board.Color(x, y), 'b')) expected.Add(C(x, y));

            CollectionAssert.AreEquivalent(expected, r.Remove);
            Assert.AreEqual(1, r.Blasts.Count);
            Assert.AreEqual(TileSpecial.ColorBomb, r.Blasts[0].Special);
            Assert.AreEqual(expected.Count - 1, r.Blasts[0].Cleared.Count);
        }

        [Test]
        public void ColorBombDoubleTap_ClearsMostCommonColor()
        {
            // c встречается 6 раз, b — 3, a — 2
            var r = new Board(
                "cacb",
                "cCbc",
                "acbc").Resolve(moved: new[] { C(1, 1), C(1, 1) }, forced: new[] { C(1, 1), C(1, 1) });

            Assert.AreEqual(7, r.Remove.Count);
            Assert.That(r.Blasts[0].Cleared.All(c => c != C(1, 1)));
            Assert.That(r.Remove, Has.Member(C(1, 1)));
        }

        [Test]
        public void RocketHitsColorBomb_ColorBombFiresToo()
        {
            var r = new Board(
                "cbcb",
                "HaCa",
                "bcbc").Resolve(forced: new[] { C(0, 1) });

            Assert.AreEqual(2, r.Blasts.Count);
            Assert.AreEqual(TileSpecial.ColorBomb, r.Blasts[1].Special);
            // Ракета убрала свою строку, цветная бомба — все оставшиеся фишки самого частого цвета
            Assert.That(r.Remove, Has.Member(C(1, 1)).And.Member(C(3, 1)));
            Assert.That(r.Blasts[1].Cleared, Is.Not.Empty);
        }

        [Test]
        public void ColorBomb_SpareOtherSpecials()
        {
            var r = new Board("aCaBa").Resolve(forced: new[] { C(1, 0) });

            Assert.AreEqual(1, r.Blasts.Count);
            Assert.That(r.Remove, Has.No.Member(C(3, 0)));
            Assert.AreEqual(4, r.Remove.Count);
        }
    }
}
