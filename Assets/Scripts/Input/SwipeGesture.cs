using UnityEngine;

namespace Input
{
    public static class SwipeGesture
    {
        /// <summary>
        /// Направление свайпа по смещению указателя в клетках поля.
        /// Возвращает соседнее направление (±1 по одной оси) или Vector2Int.zero, если палец сдвинулся меньше порога.
        /// </summary>
        public static Vector2Int Direction(Vector2 delta, float threshold)
        {
            if (delta.magnitude < threshold) return Vector2Int.zero;
            return Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                ? new Vector2Int(delta.x > 0 ? 1 : -1, 0)
                : new Vector2Int(0, delta.y > 0 ? 1 : -1);
        }
    }
}
