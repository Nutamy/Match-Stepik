using Game.MatchTiles;
using NUnit.Framework;
using UI;
using UnityEngine;

namespace Tests.Editor
{
    public class TileFXStyleTests
    {
        [TestCase(MatchDirection.Horizontal, false)]
        [TestCase(MatchDirection.Vertical, false)]
        [TestCase(MatchDirection.None, false)]
        [TestCase(MatchDirection.LongHorizontal, true)]
        [TestCase(MatchDirection.LongVertical, true)]
        [TestCase(MatchDirection.FiveInARow, true)]
        [TestCase(MatchDirection.Multiply, true)]
        public void IsBig_OnlyForFourFiveAndCross(MatchDirection direction, bool expected)
        {
            Assert.AreEqual(expected, TileFXStyle.IsBig(direction));
        }

        [Test]
        public void BigScale_GrowsWithMatchSize()
        {
            float three = TileFXStyle.BigScale(MatchDirection.Horizontal);
            float four = TileFXStyle.BigScale(MatchDirection.LongHorizontal);
            float cross = TileFXStyle.BigScale(MatchDirection.Multiply);
            float five = TileFXStyle.BigScale(MatchDirection.FiveInARow);

            Assert.AreEqual(1f, three, 1e-5f);
            Assert.Greater(four, three);
            Assert.Greater(cross, four);
            Assert.Greater(five, cross);
            Assert.AreEqual(four, TileFXStyle.BigScale(MatchDirection.LongVertical), 1e-5f);
        }

        [Test]
        public void Tint_MultipliesColorAndKeepsPrefabAlpha()
        {
            var result = TileFXStyle.Tint(new Color(1f, 0.8f, 0.5f, 0.4f), new Color(0.9f, 0.5f, 0.2f, 1f));

            Assert.AreEqual(0.9f, result.r, 1e-5f);
            Assert.AreEqual(0.4f, result.g, 1e-5f);
            Assert.AreEqual(0.1f, result.b, 1e-5f);
            Assert.AreEqual(0.4f, result.a, 1e-5f);
        }

        [Test]
        public void Tint_WhiteTileKeepsPrefabColor()
        {
            var baseColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            Assert.AreEqual(baseColor, TileFXStyle.Tint(baseColor, Color.white));
        }
    }
}
