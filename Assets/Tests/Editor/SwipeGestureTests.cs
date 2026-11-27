using Input;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Editor
{
    public class SwipeGestureTests
    {
        private const float Threshold = 0.4f;

        [Test]
        public void SmallMovement_IsNotSwipe()
        {
            Assert.AreEqual(Vector2Int.zero, SwipeGesture.Direction(new Vector2(0.2f, 0.25f), Threshold));
        }

        [TestCase(1f, 0f, 1, 0)]
        [TestCase(-0.6f, 0.1f, -1, 0)]
        [TestCase(0.1f, 0.9f, 0, 1)]
        [TestCase(-0.2f, -0.5f, 0, -1)]
        public void Swipe_UsesDominantAxis(float dx, float dy, int ex, int ey)
        {
            Assert.AreEqual(new Vector2Int(ex, ey), SwipeGesture.Direction(new Vector2(dx, dy), Threshold));
        }

        [Test]
        public void LongSwipe_StillMovesOnlyOneCell()
        {
            Assert.AreEqual(new Vector2Int(1, 0), SwipeGesture.Direction(new Vector2(3.5f, 0.4f), Threshold));
        }
    }
}
