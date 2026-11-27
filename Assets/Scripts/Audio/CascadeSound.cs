using System.Collections.Generic;
using Game.MatchTiles;
using UnityEngine;

namespace Audio
{
    /// <summary>Какой звук у группы совпадения.</summary>
    public enum GroupSound
    {
        Three,
        Four,
        Cross,
        Five
    }

    /// <summary>
    /// Чистая логика звука каскада: тон растёт с шагом каскада по мажорной гамме,
    /// у 4 в ряд, 5 в ряд и пересечения свои звуки, один звук на группу.
    /// </summary>
    public static class CascadeSound
    {
        // Полутоны мажорной гаммы: шаг 1 — исходный тон, шаг 8 и дальше — октава выше
        private static readonly int[] StepSemitones = { 0, 2, 4, 5, 7, 9, 11, 12 };

        // Больше звуков за шаг не играем, чтобы не было каши
        public const int MaxSoundsPerStep = 4;

        // Задержка между группами одного шага, с
        public const float GroupStagger = 0.07f;

        public static GroupSound For(MatchDirection direction)
        {
            switch (direction)
            {
                case MatchDirection.LongHorizontal:
                case MatchDirection.LongVertical:
                    return GroupSound.Four;
                case MatchDirection.FiveInARow:
                    return GroupSound.Five;
                case MatchDirection.Multiply:
                    return GroupSound.Cross;
                default:
                    return GroupSound.Three;
            }
        }

        public static int SemitonesForStep(int step)
        {
            int index = Mathf.Clamp(step - 1, 0, StepSemitones.Length - 1);
            return StepSemitones[index];
        }

        public static float PitchForStep(int step) => Mathf.Pow(2f, SemitonesForStep(step) / 12f);

        /// <summary>
        /// Порядок звучания групп одного шага: от простых к особым, чтобы яркий звук был последним.
        /// Если групп больше лимита, отбрасываются самые простые.
        /// </summary>
        public static List<GroupSound> PlaybackOrder(IEnumerable<MatchDirection> directions)
        {
            var sounds = new List<GroupSound>();
            foreach (var direction in directions)
            {
                if (direction == MatchDirection.None) continue;
                sounds.Add(For(direction));
            }

            sounds.Sort();
            if (sounds.Count > MaxSoundsPerStep)
                sounds.RemoveRange(0, sounds.Count - MaxSoundsPerStep);
            return sounds;
        }
    }
}
