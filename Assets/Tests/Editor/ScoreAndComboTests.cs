using Game.MatchTiles;
using Game.Score;
using NUnit.Framework;

namespace Tests.Editor
{
    public class ScoreAndComboTests
    {
        [TestCase(MatchDirection.Horizontal, 20)]
        [TestCase(MatchDirection.Vertical, 20)]
        [TestCase(MatchDirection.LongHorizontal, 50)]
        [TestCase(MatchDirection.LongVertical, 50)]
        [TestCase(MatchDirection.Multiply, 100)]
        [TestCase(MatchDirection.FiveInARow, 150)]
        [TestCase(MatchDirection.None, 0)]
        public void PointsFor_MatchesScoreTable(MatchDirection direction, int expected)
        {
            Assert.AreEqual(expected, ScoreCalculator.PointsFor(direction));
        }

        [Test]
        public void CalculateScoreToAdd_ReturnsAndAddsSamePoints()
        {
            var progress = new GameProgress();
            var calculator = new ScoreCalculator(progress);
            Assert.AreEqual(50, calculator.CalculateScoreToAdd(MatchDirection.LongVertical));
            Assert.AreEqual(0, calculator.CalculateScoreToAdd(MatchDirection.None));
            Assert.AreEqual(50, progress.Score);
        }

        [Test]
        public void Combo_PraiseOnlyOnThirdCascadeStep_AndResetsEachTurn()
        {
            var combo = new TurnCombo();
            combo.Next();
            Assert.IsFalse(combo.IsPraiseStep);
            combo.Next();
            Assert.IsFalse(combo.IsPraiseStep);
            combo.Next();
            Assert.IsTrue(combo.IsPraiseStep);
            combo.Next();
            Assert.IsFalse(combo.IsPraiseStep, "похвала один раз за ход, не на каждом следующем шаге");

            combo.Reset();
            Assert.AreEqual(0, combo.Step);
            combo.Next();
            Assert.AreEqual(1, combo.Step);
        }
    }
}
