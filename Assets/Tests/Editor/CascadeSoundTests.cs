using Audio;
using Game.MatchTiles;
using NUnit.Framework;

namespace Tests.Editor
{
    public class CascadeSoundTests
    {
        [TestCase(MatchDirection.Horizontal, GroupSound.Three)]
        [TestCase(MatchDirection.Vertical, GroupSound.Three)]
        [TestCase(MatchDirection.LongHorizontal, GroupSound.Four)]
        [TestCase(MatchDirection.LongVertical, GroupSound.Four)]
        [TestCase(MatchDirection.FiveInARow, GroupSound.Five)]
        [TestCase(MatchDirection.Multiply, GroupSound.Cross)]
        public void For_GivesOwnSoundToSpecialGroups(MatchDirection direction, GroupSound expected)
        {
            Assert.AreEqual(expected, CascadeSound.For(direction));
        }

        [Test]
        public void PitchForStep_RisesEveryStepAndStopsAtOctave()
        {
            Assert.AreEqual(1f, CascadeSound.PitchForStep(1), 1e-5f);
            for (int step = 2; step <= 8; step++)
                Assert.Greater(CascadeSound.PitchForStep(step), CascadeSound.PitchForStep(step - 1), $"step {step}");

            Assert.AreEqual(2f, CascadeSound.PitchForStep(8), 1e-4f);
            Assert.AreEqual(2f, CascadeSound.PitchForStep(20), 1e-4f);
            Assert.AreEqual(1f, CascadeSound.PitchForStep(0), 1e-5f);
        }

        [Test]
        public void PlaybackOrder_OneSoundPerGroupSpecialLast()
        {
            var order = CascadeSound.PlaybackOrder(new[]
            {
                MatchDirection.FiveInARow, MatchDirection.Horizontal, MatchDirection.Multiply, MatchDirection.None
            });
            CollectionAssert.AreEqual(new[] { GroupSound.Three, GroupSound.Cross, GroupSound.Five }, order);
        }

        [Test]
        public void PlaybackOrder_DropsPlainGroupsOverLimit()
        {
            var order = CascadeSound.PlaybackOrder(new[]
            {
                MatchDirection.Horizontal, MatchDirection.Vertical, MatchDirection.Horizontal,
                MatchDirection.LongVertical, MatchDirection.Vertical, MatchDirection.FiveInARow
            });
            Assert.AreEqual(CascadeSound.MaxSoundsPerStep, order.Count);
            CollectionAssert.AreEqual(new[] { GroupSound.Three, GroupSound.Three, GroupSound.Four, GroupSound.Five }, order);
        }
    }
}
