using System;
using Levels;

namespace Data
{
    public class GameData
    {
        public LevelConfig CurrentLevel { get; private set; }
        public int CurrentLevellIndex { get; private set; }
        public bool IsEnabledSound { get; private set; }

        public GameData()
        {
            IsEnabledSound = true;
            CurrentLevellIndex = 1;
        }

        public void SetCurrentLevelIndex(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            CurrentLevellIndex = index;
        }

        public void OpenNextLevel() => CurrentLevellIndex++;

        public void SetEnabledSound(bool value) => IsEnabledSound = value;

        public void SetCurrentLevelConfig(LevelConfig level)
        {
            if (level != null) CurrentLevel = level;
        }
    }
}